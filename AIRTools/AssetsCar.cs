using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace AIRTools
{
    public static class AssetsCar
    {
        public static async Task Create(string path)
        {
            Directory.CreateDirectory("tmp/Assets.xcassets/AppIcon.appiconset");
            Directory.CreateDirectory("tmp/build");
            var assetsCar = new AssetsCarJson
            {
                images = new List<Dictionary<string, string>>()
            };


            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "20x20",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-20x20@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "20x20",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-20x20@3x.png",
                    ["scale"] = "3x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-29x29@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-29x29@3x.png",
                    ["scale"] = "3x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "40x40",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-40x40@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "40x40",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-40x40@3x.png",
                    ["scale"] = "3x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "60x60",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-60x60@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "60x60",
                    ["idiom"] = "iphone",
                    ["filename"] = "Icon-App-60x60@3x.png",
                    ["scale"] = "3x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "20x20",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-20x20@1x.png",
                    ["scale"] = "1x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "20x20",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-20x20@2x-1.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-29x29@1x.png",
                    ["scale"] = "1x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-29x29@2x-1.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "40x40",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-40x40@1x.png",
                    ["scale"] = "1x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "40x40",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-40x40@2x-1.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "76x76",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-76x76@1x.png",
                    ["scale"] = "1x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "76x76",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-76x76@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "83.5x83.5",
                    ["idiom"] = "ipad",
                    ["filename"] = "Icon-App-83.5x83.5@2x.png",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "1024x1024",
                    ["idiom"] = "ios-marketing",
                    ["filename"] = "ItunesArtwork@2x.png",
                    ["scale"] = "1x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "24x24",
                    ["idiom"] = "watch",
                    ["scale"] = "2x",
                    ["filename"] = "Icon-24@2x.png",
                    ["role"] = "notificationCenter",
                    ["subtype"] = "38mm"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "27.5x27.5",
                    ["idiom"] = "watch",
                    ["scale"] = "2x",
                    ["filename"] = "Icon-27.5@2x.png",
                    ["role"] = "notificationCenter",
                    ["subtype"] = "42mm"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "watch",
                    ["filename"] = "Icon-29@2x.png",
                    ["role"] = "companionSettings",
                    ["scale"] = "2x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "29x29",
                    ["idiom"] = "watch",
                    ["filename"] = "Icon-29@3x.png",
                    ["role"] = "companionSettings",
                    ["scale"] = "3x"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "40x40",
                    ["idiom"] = "watch",
                    ["scale"] = "2x",
                    ["filename"] = "Icon-40@2x.png",
                    ["role"] = "appLauncher",
                    ["subtype"] = "38mm"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "44x44",
                    ["idiom"] = "watch",
                    ["scale"] = "2x",
                    ["filename"] = "Icon-44@2x.png",
                    ["role"] = "longLook",
                    ["subtype"] = "42mm"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
                {
                    ["size"] = "86x86",
                    ["idiom"] = "watch",
                    ["scale"] = "2x",
                    ["filename"] = "Icon-86@2x.png",
                    ["role"] = "quickLook",
                    ["subtype"] = "38mm"
                }
            );
            assetsCar.images.Add(new Dictionary<string, string>
            {
                ["size"] = "98x98",
                ["idiom"] = "watch",
                ["scale"] = "2x",
                ["filename"] = "Icon-98@2x.png",
                ["role"] = "quickLook",
                ["subtype"] = "42mm"
            });


            assetsCar.info = new Dictionary<string, object>
            {
                ["version"] = 1,
                ["author"] = "fanstudio"
            };
            
            assetsCar.properties = new Dictionary<string, bool>
            {
                ["pre-rendered"] = true
            };

            await File.WriteAllTextAsync("tmp/Assets.xcassets/AppIcon.appiconset/Contents.json",
                JsonConvert.SerializeObject(assetsCar, new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                }));

            foreach (var asset in assetsCar.images)
            {
                var sizeArr = asset["size"].Split("x");
                var fileName = $"tmp/Assets.xcassets/AppIcon.appiconset/{asset["filename"]}";
                var scale = asset["scale"] switch
                {
                    "2x" => 2,
                    "3x" => 3,
                    _ => 1
                };
                
                var width = scale * float.Parse(sizeArr[0], CultureInfo.InvariantCulture);
                var height = scale * float.Parse(sizeArr[1], CultureInfo.InvariantCulture);
                using var image = await Image.LoadAsync(path);

                image.Mutate(x => x
                    .Resize((int) width, (int) height));
                image.Metadata.HorizontalResolution = 72;
                image.Metadata.VerticalResolution = 72;
                await image.SaveAsync(fileName, new PngEncoder());
            }

            const string actoolString = "actool tmp/Assets.xcassets --compile tmp/build --platform iphoneos --minimum-deployment-target 8.0 --app-icon AppIcon --output-partial-info-plist tmp/build/partial.plist";
            var mergeInfo = new ProcessStartInfo("xcrun")
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                WorkingDirectory = $"{Program.CurrentDirectory}",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = actoolString,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
                
            try
            {
                using var exeProcess = Process.Start(mergeInfo);
                exeProcess?.WaitForExit();
                
                File.Copy("tmp/build/Assets.car", "Assets.car", true);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Assets.car file created");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    class AssetsCarJson
    {
        public List<Dictionary<string, string>> images { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Dictionary<string, object> info { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Dictionary<string, bool> properties { get; set; }
    }
}