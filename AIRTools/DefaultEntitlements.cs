using System.Xml;

namespace AIRTools
{
    public static class DefaultEntitlements
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

            rootNode.AppendChild(doc.CreateElement("dict"));
            doc.AppendChild(rootNode);
            doc.Save("tmp/Entitlements-merged.entitlements");
        }
    }
}