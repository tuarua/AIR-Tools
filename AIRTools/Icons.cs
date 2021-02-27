using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System.Xml;

namespace AIRTools
{
    public static class Icons
    {
        public static async Task Create(string iconPath, string xmlPath = null)
        {
            if (Directory.Exists("icons"))
            {
                Directory.Delete("icons", true);
            }
            Directory.CreateDirectory("icons");

            var sizes = new List<int>
                {16, 29, 32, 36, 40, 44, 48, 50, 57, 58, 60, 66, 72, 75, 76, 80, 87, 96, 
                    100, 114, 120, 128, 144, 152, 167, 180, 192, 512, 1024};

            foreach (var size in sizes)
            {
                using var image = await Image.LoadAsync(iconPath);
                var fileName = $"icons/icon{size}x{size}.png";
                image.Mutate(x => x
                    .Resize(size, size));
                image.Metadata.HorizontalResolution = 72;
                image.Metadata.VerticalResolution = 72;
                await image.SaveAsync(fileName, new PngEncoder());
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Icons created in /icons");
            Console.ResetColor();

            if (xmlPath != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Updating App Descriptor XML with Icons");
                Console.ResetColor();

                var doc = new XmlDocument();
                doc.Load(xmlPath);
                var node = doc.DocumentElement;
                var androidNodeTest = node?["icon"];
                XmlNode iconNode;
                if (androidNodeTest == null)
                {
                    iconNode = doc.CreateElement("icon", doc.DocumentElement?.NamespaceURI);
                    doc.DocumentElement?.AppendChild(iconNode);
                }

                iconNode = node?["icon"];
                iconNode?.RemoveAll();
                foreach (var size in sizes)
                {
                    var child = doc.CreateElement($"image{size}x{size}", doc.DocumentElement?.NamespaceURI);
                    child.InnerText = $"icon{size}x{size}.png";
                    child.Attributes.RemoveAll();
                    iconNode?.AppendChild(child);
                }

                doc.Save(xmlPath);
            }
        }
    }
}