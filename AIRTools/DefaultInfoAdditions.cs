using System.Xml;

namespace AIRTools
{
    public static class DefaultInfoAdditions
    {
        public static void Create()
        {
            var doc = new XmlDocument();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            var docType = doc.CreateDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN",
                "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
            doc.AppendChild(docType);

            var rootNode = doc.CreateElement("plist");

            var xmlns = doc.CreateAttribute("version");
            xmlns.Value = "1.0";
            rootNode.Attributes.Append(xmlns);

            var dict = doc.CreateElement("dict");
            var key = doc.CreateElement("key");
            key.InnerText = "UIDeviceFamily";
            dict.AppendChild(key);

            var array = doc.CreateElement("array");
            var string1 = doc.CreateElement("string");
            string1.InnerText = "1";
            var string2 = doc.CreateElement("string");
            string2.InnerText = "2";
            
            array.AppendChild(string1);
            array.AppendChild(string2);
            dict.AppendChild(array);
            
            var key2 = doc.CreateElement("key");
            key2.InnerText = "MinimumOSVersion";
            dict.AppendChild(key2);
            
            var stringVersion = doc.CreateElement("string");
            stringVersion.InnerText = "9.0";
            dict.AppendChild(stringVersion);
            
            rootNode.AppendChild(dict);
            
            doc.AppendChild(rootNode);
            doc.Save("tmp/InfoAdditions-merged.plist");
        }
    }
}