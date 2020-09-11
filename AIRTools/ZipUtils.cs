using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AIRTools
{
    internal static class ZipUtils
    {
        public static bool HasEntry(string path, string entryPath)
        {
            using var archive = ZipFile.OpenRead(path);
            return archive.Entries.Any(entry => entryPath == entry.FullName);
        }

        public static void Extract(string path, string to, string entryPath)
        {
            {
                using var archive = ZipFile.OpenRead(path);
                foreach (var entry in archive.Entries)
                {
                    if (entryPath != entry.FullName)
                    {
                        continue;
                    }

                    var fullPath = Path.Combine(to, entry.Name);
                    entry.ExtractToFile(fullPath, true);
                }
            }
        }
    }
}