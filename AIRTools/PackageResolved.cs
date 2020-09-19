using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace AIRTools
{
    public static class PackageResolved
    {
        private static Dictionary<string, string> Dependencies { get; set; }

        public static void Save()
        {
            var json = JsonConvert.SerializeObject(Dependencies,
                new JsonSerializerSettings {Formatting = Formatting.Indented});
            File.WriteAllText("air_package.resolved.json", json);
        }

        public static void Update(string key, string value)
        {
            Dependencies.Remove(key);
            Dependencies.Add(key, value);
        }

        public static void Load()
        {
            if (!File.Exists("air_package.resolved.json"))
            {
                File.WriteAllTextAsync("air_package.resolved.json", "{}");
            }

            Dependencies =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    File.ReadAllText("air_package.resolved.json"));
        }

        public static bool IsCurrent(string key, string version) =>
            Dependencies.ContainsKey(key) && Dependencies[key] == version;

        public static string PreviousVersion(string key) => Dependencies.ContainsKey(key) ? Dependencies[key] : null;
    }
}