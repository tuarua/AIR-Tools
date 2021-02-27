using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AIRTools
{
    public static class GoogleServices
    {
        private static XmlDocument _doc;

        public static async Task FromJson(string path, bool addToAne = true)
        {
            var googleServices =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(await File.ReadAllTextAsync(path));

            var projectInfo = JObject.FromObject(googleServices["project_info"]);
            var projectNumber = projectInfo["project_number"]?.ToString();
            var firebaseUrl = projectInfo["firebase_url"]?.ToString();
            var projectId = projectInfo["project_id"]?.ToString();
            var storageBucket = projectInfo["storage_bucket"]?.ToString();
            var client = JArray.FromObject(googleServices["client"]).First;
            var apiKey = JArray.FromObject(client?["api_key"]!).First;
            var oauthClient = JArray.FromObject(client?["oauth_client"]!).First;
            var clientInfo = JObject.FromObject(client?["client_info"]!);
            var mobilesdkAppId = JObject.FromObject(clientInfo!)["mobilesdk_app_id"]?.ToString();
            var currentKey = JObject.FromObject(apiKey!)["current_key"]?.ToString();
            var clientId = JObject.FromObject(oauthClient!)["client_id"]?.ToString();

            _doc = new XmlDocument();
            var xmlDeclaration = _doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = _doc.DocumentElement;
            _doc.InsertBefore(xmlDeclaration, root);
            var rootNode = _doc.CreateElement("resources");

            XmlNode node1 = _doc.CreateElement("string");
            var attr1 = _doc.CreateAttribute("name");
            attr1.Value = "app_name";
            node1.Attributes?.Append(attr1);
            node1.InnerText = "FirebaseANE";

            XmlNode node2 = _doc.CreateElement("string");
            var attr2 = _doc.CreateAttribute("name");
            attr2.Value = "default_web_client_id";
            node2.Attributes?.Append(attr2);
            node2.Attributes?.Append(GetTranslatableAttribute());
            node2.InnerText = clientId ?? string.Empty;

            XmlNode node3 = _doc.CreateElement("string");
            node3.InnerText = firebaseUrl ?? string.Empty;
            var attr3 = _doc.CreateAttribute("name");
            attr3.Value = "firebase_database_url";
            node3.Attributes?.Append(attr3);
            node3.Attributes?.Append(GetTranslatableAttribute());

            XmlNode node4 = _doc.CreateElement("string");
            node4.InnerText = projectNumber ?? string.Empty;
            var attr4 = _doc.CreateAttribute("name");
            attr4.Value = "gcm_defaultSenderId";
            node4.Attributes?.Append(attr4);
            node4.Attributes.Append(GetTranslatableAttribute());

            XmlNode node5 = _doc.CreateElement("string");
            node5.InnerText = currentKey;
            var attr5 = _doc.CreateAttribute("name");
            attr5.Value = "google_api_key";
            node5.Attributes.Append(attr5);
            node5.Attributes.Append(GetTranslatableAttribute());

            XmlNode node6 = _doc.CreateElement("string");
            node6.InnerText = mobilesdkAppId;
            var attr6 = _doc.CreateAttribute("name");
            attr6.Value = "google_app_id";
            node6.Attributes.Append(attr6);
            node6.Attributes.Append(GetTranslatableAttribute());

            XmlNode node7 = _doc.CreateElement("string");
            node7.InnerText = currentKey;
            var attr7 = _doc.CreateAttribute("name");
            attr7.Value = "google_crash_reporting_api_key";
            node7.Attributes.Append(attr7);
            node7.Attributes.Append(GetTranslatableAttribute());

            XmlNode node8 = _doc.CreateElement("string");
            node8.InnerText = storageBucket;
            var attr8 = _doc.CreateAttribute("name");
            attr8.Value = "google_storage_bucket";
            node8.Attributes.Append(attr8);
            node8.Attributes.Append(GetTranslatableAttribute());

            XmlNode node9 = _doc.CreateElement("string");
            node9.InnerText = projectId;
            var attr9 = _doc.CreateAttribute("name");
            attr9.Value = "project_id";
            node9.Attributes.Append(attr9);
            node9.Attributes.Append(GetTranslatableAttribute());

            rootNode.AppendChild(node1);
            rootNode.AppendChild(node2);
            rootNode.AppendChild(node3);
            rootNode.AppendChild(node4);
            rootNode.AppendChild(node5);
            rootNode.AppendChild(node6);
            rootNode.AppendChild(node7);
            rootNode.AppendChild(node8);
            rootNode.AppendChild(node9);

            _doc.AppendChild(rootNode);

            if (addToAne)
            {
                _doc.Save("tmp/values.xml");

                const string fn = "FirebaseANE";
                var anePath = $"extensions/{fn}.ane";
                var zipPath = Path.ChangeExtension(anePath, "zip");

                File.Move(anePath, zipPath, true);
                try
                {
                    await using var zipToOpen = new FileStream(zipPath, FileMode.Open);
                    using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update);

                    archive.GetEntry("META-INF/ANE/Android-ARM/com.tuarua.firebase.FirebaseANE-res/values/values.xml")
                        ?.Delete();
                    archive.GetEntry("META-INF/ANE/Android-ARM64/com.tuarua.firebase.FirebaseANE-res/values/values.xml")
                        ?.Delete();
                    archive.GetEntry("META-INF/ANE/Android-x86/com.tuarua.firebase.FirebaseANE-res/values/values.xml")
                        ?.Delete();

                    archive.CreateEntryFromFile("tmp/values.xml",
                        "META-INF/ANE/Android-ARM/com.tuarua.firebase.FirebaseANE-res/values/values.xml",
                        CompressionLevel.NoCompression);
                    archive.CreateEntryFromFile("tmp/values.xml",
                        "META-INF/ANE/Android-ARM64/com.tuarua.firebase.FirebaseANE-res/values/values.xml",
                        CompressionLevel.NoCompression);
                    archive.CreateEntryFromFile("tmp/values.xml",
                        "META-INF/ANE/Android-x86/com.tuarua.firebase.FirebaseANE-res/values/values.xml",
                        CompressionLevel.NoCompression);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                File.Move(zipPath, anePath, true);
            }
            else
            {
                Directory.CreateDirectory("res/values");
                _doc.Save("res/values/values.xml");
            }
        }

        private static XmlAttribute GetTranslatableAttribute()
        {
            var ret = _doc.CreateAttribute("translatable");
            ret.Value = "false";
            return ret;
        }
    }
}