using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AIRTools
{
    public static class Program
    {
        public static string CurrentDirectory => Directory.GetCurrentDirectory();
        private static AppDescription _appDescription;
        private static string _manifestMergerPath;
        private const string PlistBuddyPath = "/usr/libexec/PlistBuddy";
        private static string Shell => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "bash";

        private static readonly Dictionary<string, Package> DependencyPackage = new Dictionary<string, Package>();

        private static readonly Dictionary<string, string> DependencyManifest = new Dictionary<string, string>();

        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly Dictionary<string, string> DependencyEntitlements = new Dictionary<string, string>();

        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly Dictionary<string, string> DependencyInfoAdditions = new Dictionary<string, string>();

        private static Package _projectPackage;

        private static readonly Dictionary<string, Dictionary<string, Dictionary<string, string>>> RepositoryDict =
            new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        private static readonly List<string> Extensions = new List<string>();

        private static async Task Main(string[] args)
        {

            if (args.Length == 0)
            {
                PrintError("Pass the command as an argument");
                return;
            }

            var appDataFolder = GetAppDataFolder();

            if (!string.IsNullOrEmpty(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            Directory.CreateDirectory("tmp");

            var command = args[0];
            switch (command)
            {
                case "install":
                    Directory.CreateDirectory("libs");
                    Directory.CreateDirectory("extensions");
                    _projectPackage =
                        JsonConvert.DeserializeObject<Package>(await File.ReadAllTextAsync("air_package.json"));
                    PackageResolved.Load();
                    GetManifestMerger();
                    LoadAppDescriptor();
                    await Install();
                    break;
                case "clear-cache":
                    if (!string.IsNullOrEmpty(appDataFolder))
                    {
                        Directory.Delete(appDataFolder, true);
                        Directory.CreateDirectory(appDataFolder);
                    }
                    break;
                case "apply-firebase-config":
                    if (args.Length < 2)
                    {
                        PrintError(
                            "You need to pass the path to the google-services.json file after `apply-firebase-config`.");
                        return;
                    }

                    await ApplyFirebaseConfig(args[1]);
                    break;
                case "add-raw-asset":
                    if (args.Length < 2)
                    {
                        PrintError(
                            "You need to pass the path to the file after `add-raw-asset`.");
                        return;
                    }

                    await AddRawAsset(args[1]);
                    break;
                case "create-assets-car":
                    if (args.Length < 2)
                    {
                        PrintError(
                            "You need to pass the path to the 1024x1024px png file.");
                        return;
                    }

                    await AssetsCar.Create(args[1]);
                    break;
                default:
                    PrintError(
                        "Command not recognized.");
                    break;
            }

            Directory.Delete("tmp", true);
        }

        /**
         * Reads air_package.json and downloads and parses all dependencies
         */
        private static async Task Install()
        {
            await ParseDependencies(_projectPackage);
            _appDescription.UpdateExtensions(Extensions);
            MergeManifests();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && HasPlistBuddy)
            {
                MergeInfoAdditions();
                MergeEntitlements();
            }
            else
            {
                PrintError("Cannot find PListBuddy, this is available on macOS only.");
            }
        }

        /*
         * Applies the values from the provided google-services.json to FirebaseANE.ane
         */
        private static async Task ApplyFirebaseConfig(string path)
        {
            if (!File.Exists("extensions/FirebaseANE.ane"))
            {
                PrintError("You do not have FirebaseANE.ane inside the extensions folder.");
                return;
            }

            await GoogleServices.FromJson(path);
        }

        /**
         * Adds a raw asset to Firebase.ANE for Android
         */
        private static async Task AddRawAsset(string path)
        {
            if (!File.Exists("extensions/FirebaseANE.ane"))
            {
                PrintError("You do not have FirebaseANE.ane inside the extensions folder.");
                return;
            }

            const string fn = "FirebaseANE";
            var anePath = $"extensions/{fn}.ane";
            var zipPath = Path.ChangeExtension(anePath, "zip");
            File.Move(anePath, zipPath, true);
            try
            {
                await using var zipToOpen = new FileStream(zipPath, FileMode.Open);
                using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

                var fileName = Path.GetFileName(path);

                archive.GetEntry($"META-INF/ANE/Android-ARM/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}")
                    ?.Delete();
                archive.GetEntry($"META-INF/ANE/Android-ARM64/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}")
                    ?.Delete();
                archive.GetEntry($"META-INF/ANE/Android-x86/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}")
                    ?.Delete();

                archive.CreateEntryFromFile(path,
                    $"META-INF/ANE/Android-ARM/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}",
                    CompressionLevel.NoCompression);
                archive.CreateEntryFromFile(path,
                    $"META-INF/ANE/Android-ARM64/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}",
                    CompressionLevel.NoCompression);
                archive.CreateEntryFromFile(path,
                    $"META-INF/ANE/Android-x86/com.tuarua.firebase.FirebaseANE-res/raw/{fileName}",
                    CompressionLevel.NoCompression);
            }
            catch (Exception e)
            {
                PrintError(e.Message);
                throw;
            }

            File.Move(zipPath, anePath, true);
        }

        private static async Task ParseDependencies(Package package)
        {
            foreach (var (packageId, value) in package.Dependencies)
            {
                if (value.StartsWith("http"))
                {
                    try
                    {
                        await DownloadDependency(value);
                        await ProcessDependency(packageId, GetFileNameFromUrl(value));
                    }
                    catch (Exception e)
                    {
                        PrintError($"{e.Message} - {e.InnerException?.Message}");
                    }
                }
                else if (value.StartsWith("file:"))
                {
                    var path = value.Replace("file:", "");
                    var fn = Path.GetFileName(path);
                    var fileType = Path.GetExtension(fn);
                    var outputFolder = fileType switch
                    {
                        ".ane" => "extensions",
                        ".swc" => "libs",
                        _ => ""
                    };
                    
                    if (Path.GetFullPath(path) != Path.Combine(CurrentDirectory, outputFolder, fn))
                    {
                        File.Copy(path, Path.Combine(outputFolder, fn));
                    }

                    await ProcessDependency(packageId, GetFileNameFromUrl(value));
                }
                else
                {
                    var repoUrl = package.Repository["url"];
                    if (string.IsNullOrEmpty(repoUrl))
                    {
                        PrintError(
                            $"The {packageId} package file references versioned dependencies but has no repository url");
                        return;
                    }

                    if (!RepositoryDict.ContainsKey(repoUrl))
                    {
                        var client = new HttpClient();
                        RepositoryDict[repoUrl] =
                            JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
                                await client.GetStringAsync(new Uri(repoUrl)));
                    }

                    var packages = RepositoryDict[repoUrl];
                    if (!packages.ContainsKey(packageId))
                    {
                        PrintError($"Cannot find package for: {packageId}");
                        return;
                    }

                    if (!packages[packageId].ContainsKey(value))
                    {
                        PrintError($"Cannot find url for: {packageId} {value}");
                        return;
                    }

                    var url = packages[packageId][value];

                    if (PackageResolved.IsCurrent(packageId, value))
                    {
                        PrintInfo($"Using existing version of {packageId} version {value}");
                    }
                    else
                    {
                        var previousVersion = PackageResolved.PreviousVersion(packageId);
                        if (!string.IsNullOrEmpty(previousVersion))
                        {
                            var previousUrl = packages[packageId][previousVersion];
                            var previousFileName = GetFileNameFromUrl(previousUrl);
                            var fileType = Path.GetExtension(previousFileName);
                            var outputFolder = fileType switch
                            {
                                ".ane" => "extensions",
                                ".swc" => "libs",
                                _ => ""
                            };
                            var previousFilePath = Path.Combine(outputFolder, previousFileName);

                            if (!string.IsNullOrEmpty(previousFileName) &&
                                File.Exists(previousFilePath))
                            {
                                File.Delete(previousFilePath);
                            }

                            PrintInfo($"Upgrading: {packageId} from {previousVersion} to {value}");
                        }

                        try
                        {
                            var fileName = GetFileNameFromUrl(url);
                            var fileType = Path.GetExtension(fileName);
                            if (!string.IsNullOrEmpty(GetAppDataFolder())) // hasAppDataFolder
                            {
                                if (!File.Exists(Path.Combine(GetAppDataFolder(), packageId, value, fileName)))
                                {
                                    await DownloadDependencyToShared(url, packageId, value);
                                }
                                var outputFolder = fileType switch
                                {
                                    ".ane" => "extensions",
                                    ".swc" => "libs",
                                    _ => ""
                                };
                                PrintInfo($"Using cached AppData version of {packageId} version {value}");
                                File.Copy(Path.Combine(GetAppDataFolder(), packageId, value, fileName), Path.Combine(outputFolder, fileName), true);
                            }
                            else
                            {
                                await DownloadDependency(url);
                            }

                            PackageResolved.Update(packageId, value);
                            PackageResolved.Save();
                        }
                        catch (Exception e)
                        {
                            PrintError($"{e.Message} - {e.InnerException?.Message}");
                        }
                    }

                    await ProcessDependency(packageId, GetFileNameFromUrl(url));
                }
            }
        }

        private static async Task DownloadDependency(string url)
        {
            PrintInfo($"Downloading: {url}");
            var fileName = GetFileNameFromUrl(url);
            var fileType = Path.GetExtension(fileName);
            var outputFolder = fileType switch
            {
                ".ane" => "extensions",
                ".swc" => "libs",
                _ => ""
            };
            var client = new HttpClient();
            var response = await client.GetByteArrayAsync(new Uri(url));
            await File.WriteAllBytesAsync(Path.Combine(outputFolder, fileName), response);
        }

        private static async Task DownloadDependencyToShared(string url, string packageId, string version)
        {
            PrintInfo($"Downloading: {url} to AppData folder");
            var fileName = GetFileNameFromUrl(url);
            var client = new HttpClient();
            var response = await client.GetByteArrayAsync(new Uri(url));
            Directory.CreateDirectory(Path.Combine(GetAppDataFolder(), packageId, version));
            await File.WriteAllBytesAsync(Path.Combine(GetAppDataFolder(), packageId, version, fileName), response);
        }

        private static async Task ProcessDependency(string packageId, string fileName)
        {
            switch (Path.GetExtension(fileName))
            {
                case ".zip":
                    if (File.Exists(fileName))
                    {
                        ZipFile.ExtractToDirectory(fileName, CurrentDirectory, true);
                        File.Delete(fileName);
                    }

                    break;
                case ".ane":
                    // ANEs can contain same dependencies, so prevent duplicates
                    if (!Extensions.Contains(packageId))
                    {
                        Extensions.Add(packageId);
                        await ExtractAneFiles(packageId, fileName);
                    }
                    break;
            }
        }

        /**
         * Pulls needed files and values out of the ANE root folder for processing
         */
        private static async Task ExtractAneFiles(string packageId, string fileName)
        {
            var fn = fileName.Replace(".ane", "");
            var anePath = $"extensions/{fn}.ane";
            var zipPath = $"extensions/{fn}.zip";

            File.Move(anePath, zipPath, true);

            if (ZipUtils.HasEntry(zipPath, "air_package.json"))
            {
                ZipUtils.Extract(zipPath, $"{CurrentDirectory}/tmp", "air_package.json");

                File.Move("tmp/air_package.json",
                    $"tmp/{packageId}-air_package.json", true);

                var json = await File.ReadAllTextAsync($"tmp/{packageId}-air_package.json");
                DependencyPackage[packageId] = JsonConvert.DeserializeObject<Package>(json);
            }

            if (ZipUtils.HasEntry(zipPath, "AndroidManifest.xml"))
            {
                ZipUtils.Extract(zipPath, $"{CurrentDirectory}/tmp", "AndroidManifest.xml");
                File.Move("tmp/AndroidManifest.xml",
                    $"tmp/{packageId}-AndroidManifest.xml", true);
                DependencyManifest[packageId] = $"tmp/{packageId}-AndroidManifest.xml";
            }

            if (ZipUtils.HasEntry(zipPath, "InfoAdditions.plist"))
            {
                ZipUtils.Extract(zipPath, $"{CurrentDirectory}/tmp", "InfoAdditions.plist");
                File.Move($"{CurrentDirectory}/tmp/InfoAdditions.plist",
                    $"tmp/{packageId}-InfoAdditions.plist", true);
                DependencyInfoAdditions[packageId] = $"tmp/{packageId}-InfoAdditions.plist";
            }

            if (ZipUtils.HasEntry(zipPath, "Entitlements.entitlements"))
            {
                ZipUtils.Extract(zipPath, $"{CurrentDirectory}/tmp", "Entitlements.entitlements");
                File.Move("tmp/Entitlements.entitlements",
                    $"tmp/{packageId}-Entitlements.entitlements", true);
                DependencyEntitlements[packageId] = $"tmp/{packageId}-Entitlements.entitlements";
            }

            File.Move(zipPath, anePath, true);
            if (DependencyPackage.ContainsKey(packageId))
            {
                var package = DependencyPackage[packageId];
                await ParseDependencies(package);
            }
        }

        /**
         * Merges the Android manifests from all dependencies into app.xml
         */
        private static void MergeManifests()
        {
            if (DependencyManifest.Count == 0)
            {
                return;
            }

            PrintInfo("Merging Manifests");

            DefaultAndroidManifest.Create();

            var dependencyManifests = DependencyManifest
                .Select(manifest => manifest.Value.Replace("/", Path.DirectorySeparatorChar.ToString())).ToList();
            var mergerString =
                $"\"{_manifestMergerPath}\" --main {Path.Join("tmp", "DefaultAndroidManifest.xml")} --libs {string.Join(" --libs ", dependencyManifests)} --out {Path.Join("tmp", "AndroidManifest-merged.xml")} --log ERROR";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                mergerString = $"/c {mergerString}";
            }

            var mergeInfo = new ProcessStartInfo(Shell)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                WorkingDirectory = $"{CurrentDirectory}",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = mergerString
            };
            try
            {
                using var exeProcess = Process.Start(mergeInfo);
                exeProcess?.WaitForExit();

                var xmlStr = File.ReadAllText("tmp/AndroidManifest-merged.xml");

                xmlStr = xmlStr.Replace("android:name=\"#{applicationId}.mainActivity\"", "");
                xmlStr = xmlStr.Replace("#{applicationId}",
                    _projectPackage.AirDotPrefix ? "air." + _appDescription.Id : _appDescription.Id);
                xmlStr = _projectPackage.Variables.Aggregate(xmlStr,
                    (current, variable) => current.Replace("#{" + variable.Key + "}", variable.Value));
                xmlStr = xmlStr.Replace("#{", "${");
                _appDescription.UpdateManifestAdditions(xmlStr);
            }
            catch (Exception e)
            {
                PrintError(e.Message);
            }
        }

        /**
         * Merges the iOS InfoAdditions.plist from all dependencies into app.xml
         */
        private static void MergeInfoAdditions()
        {
            if (DependencyInfoAdditions.Count == 0)
            {
                return;
            }

            PrintInfo("Merging InfoAdditions");

            DefaultInfoAdditions.Create();

            foreach (var mergeInfo in DependencyInfoAdditions
                .Select(infoAddition => $"-x -c \"Merge {infoAddition.Value}\" tmp/InfoAdditions-merged.plist").Select(
                    mergerString => new ProcessStartInfo(PlistBuddyPath)
                    {
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        WorkingDirectory = "",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = mergerString
                    }))
            {
                try
                {
                    using var exeProcess = Process.Start(mergeInfo);
                    exeProcess?.WaitForExit();
                }
                catch (Exception e)
                {
                    PrintError(e.Message);
                }
            }

            var xmlStr = File.ReadAllText("tmp/InfoAdditions-merged.plist");
            xmlStr = xmlStr.Replace("${applicationId}", _appDescription.Id);
            if (!string.IsNullOrEmpty(_projectPackage.AppleTeamId))
            {
                xmlStr = xmlStr.Replace("${teamId}", _projectPackage.AppleTeamId);
            }

            xmlStr = _projectPackage.Variables.Aggregate(xmlStr,
                (current, variable) => current.Replace($"${{{variable.Key}}}", variable.Value));

            _appDescription.UpdateInfoAdditions(xmlStr);
        }

        /**
         * Merges the iOS Entitlements.entitlements from all dependencies into app.xml
         */
        private static void MergeEntitlements()
        {
            if (DependencyEntitlements.Count == 0)
            {
                return;
            }

            PrintInfo("Merging Entitlements");

            DefaultEntitlements.Create();

            foreach (var mergeInfo in DependencyEntitlements
                .Select(entitlements => $"-x -c \"Merge {entitlements.Value}\" tmp/Entitlements-merged.entitlements")
                .Select(
                    mergerString => new ProcessStartInfo(PlistBuddyPath)
                    {
                        CreateNoWindow = false,
                        UseShellExecute = false,
                        WorkingDirectory = "",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        Arguments = mergerString
                    }))
            {
                try
                {
                    using var exeProcess = Process.Start(mergeInfo);
                    exeProcess?.WaitForExit();
                }
                catch (Exception e)
                {
                    PrintError(e.Message);
                }
            }

            var xmlStr = File.ReadAllText("tmp/Entitlements-merged.entitlements");
            xmlStr = xmlStr.Replace("${applicationId}", _appDescription.Id);
            if (!string.IsNullOrEmpty(_projectPackage.AppleTeamId))
            {
                xmlStr = xmlStr.Replace("${teamId}", _projectPackage.AppleTeamId);
            }

            xmlStr = _projectPackage.Variables.Aggregate(xmlStr,
                (current, variable) => current.Replace($"${{{variable.Key}}}", variable.Value));

            _appDescription.UpdateEntitlements(xmlStr);
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void LoadAppDescriptor()
        {
            _appDescription = new AppDescription(_projectPackage.AppDescriptor);
        }

        private static void GetManifestMerger()
        {
            var exeFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
            var manifestMerger = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Join(exeFolder, "manifest-merger.bat")
                : Path.Join(exeFolder, "manifest-merger");

            _manifestMergerPath = manifestMerger;
        }

        private static bool HasPlistBuddy => File.Exists(PlistBuddyPath);

        private static string GetFileNameFromUrl(string url) => Path.GetFileName(url).Split("?").First();

        private static string GetAppDataFolder()
        {
            var userPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(userPath))
            {
                return null;
            }

            var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
            if (string.IsNullOrEmpty(assemblyName))
            {
                return null;
            }

            var path = Path.Combine(userPath, assemblyName);
            return path;
        }
    }
}