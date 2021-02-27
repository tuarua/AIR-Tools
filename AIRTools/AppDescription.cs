using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace AIRTools
{
    public class AppDescription
    {
        public AppDescription(string path)
        {
            _path = path;
            _doc = new XmlDocument();
            _doc.Load(path);
            _node = _doc.DocumentElement;

            Id = _node?["id"]?.FirstChild.Value;
        }

        public void UpdateManifestAdditions(string xmlStr)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Updating App Descriptor XML with Manifest Additions");
            Console.ResetColor();

            var newXml = new XmlDocument();
            newXml.LoadXml(xmlStr);
            newXml.RemoveChild(newXml.FirstChild); // remove the xml definition

            var stringBuilder = new StringBuilder();
            var element = XElement.Parse(newXml.InnerXml);
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = false,
                IndentChars = "\t",
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };
            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            const string pattern = "<manifest(.*?)>";
            var cdataStr = Regex.Replace(stringBuilder.ToString(), pattern,
                "<manifest android:installLocation=\"auto\">");
            var cdata = _doc.CreateCDataSection(cdataStr);

            var androidNodeTest = _node["android"];
            XmlNode androidNode;
            if (androidNodeTest == null)
            {
                androidNode = _doc.CreateElement("android", _doc.DocumentElement?.NamespaceURI);
                _doc.DocumentElement?.AppendChild(androidNode);
            }

            androidNode = _node["android"];
            var manifestAdditionsNodeTest = androidNode?["manifestAdditions"];
            if (manifestAdditionsNodeTest == null)
            {
                var manifestAdditionsNode = _doc.CreateElement("manifestAdditions", _doc.DocumentElement?.NamespaceURI);
                androidNode?.AppendChild(manifestAdditionsNode);
            }

            _node["android"]["manifestAdditions"].InnerXml = cdata.OuterXml;
            _doc.Save(_path);
        }

        public void UpdateInfoAdditions(string xmlStr)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Updating App Descriptor XML with Info Additions");
            Console.ResetColor();

            var newXml = new XmlDocument();
            newXml.LoadXml(xmlStr);
            newXml.RemoveChild(newXml.FirstChild);
            newXml.RemoveChild(newXml.FirstChild);
            if (newXml.FirstChild.InnerXml == "<dict />")
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            var element = XElement.Parse(newXml.FirstChild.InnerXml);

            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                WriteEndDocumentOnClose = false,
                Indent = true,
                NewLineOnAttributes = true,
                IndentChars = "\t"
            };
            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }
            
            var cdata = _doc.CreateCDataSection(TrimDictTags(stringBuilder.ToString()));
            var iPhoneNode = GetIphoneNode();
            var infoAdditionsNodeTest = iPhoneNode["InfoAdditions"];
            if (infoAdditionsNodeTest == null)
            {
                var infoAdditionsNode = _doc.CreateElement("InfoAdditions", _doc.DocumentElement?.NamespaceURI);
                iPhoneNode.AppendChild(infoAdditionsNode);
            }

            _node["iPhone"]["InfoAdditions"].InnerXml = cdata.OuterXml;
            _doc.Save(_path);
        }

        private XmlNode GetIphoneNode()
        {
            var iPhoneNodeTest = _node["iPhone"];
            XmlNode iPhoneNode;
            if (iPhoneNodeTest == null)
            {
                iPhoneNode = _doc.CreateElement("iPhone", _doc.DocumentElement?.NamespaceURI);
                var requestedDisplayResolutionNode = _doc.CreateElement("requestedDisplayResolution", _doc.DocumentElement?.NamespaceURI);
                requestedDisplayResolutionNode.InnerText = "high";
                iPhoneNode.AppendChild(requestedDisplayResolutionNode);
                _doc.DocumentElement?.AppendChild(iPhoneNode);
            }

            iPhoneNode = _node["iPhone"];
            return iPhoneNode;
        }

        /**
         * Trim off the start and end <dict></dict> as these don't belong in the AIR CDATA.
         */
        private static string TrimDictTags(string s)
        {
            s = s.Substring(6);
            s = s.Substring(0, s.Length - 7);
            return s;
        }

        public void UpdateEntitlements(string xmlStr)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Updating App Descriptor XML with Entitlements");
            Console.ResetColor();

            var newXml = new XmlDocument();
            newXml.LoadXml(xmlStr);
            newXml.RemoveChild(newXml.FirstChild);
            newXml.RemoveChild(newXml.FirstChild);
            if (newXml.FirstChild.InnerXml == "<dict />")
            {
                return;
            }

            var stringBuilder = new StringBuilder();
            var element = XElement.Parse(newXml.FirstChild.InnerXml);
            var settings = new XmlWriterSettings
                {OmitXmlDeclaration = true, Indent = true, NewLineOnAttributes = true, IndentChars = "\t"};
            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            var cdata = _doc.CreateCDataSection(TrimDictTags(stringBuilder.ToString()));
            var iPhoneNode = GetIphoneNode();
            var entitlementsNodeTest = iPhoneNode["Entitlements"];
            if (entitlementsNodeTest == null)
            {
                var entitlementsNode = _doc.CreateElement("Entitlements", _doc.DocumentElement?.NamespaceURI);
                iPhoneNode.AppendChild(entitlementsNode);
            }

            _node["iPhone"]["Entitlements"].InnerXml = cdata.OuterXml;
            _doc.Save(_path);
        }

        public void UpdateExtensions(IEnumerable<string> extensions)
        {
            var extensionsNodeTest = _node["extensions"];
            XmlNode extensionsNode;
            if (extensionsNodeTest == null)
            {
                extensionsNode = _doc.CreateElement("extensions", _doc.DocumentElement?.NamespaceURI);
                _doc.DocumentElement?.AppendChild(extensionsNode);
            }

            extensionsNode = _node["extensions"];
            extensionsNode?.RemoveAll();
            foreach (var extension in extensions)
            {
                var newNode = _doc.CreateElement("extensionID", _doc.DocumentElement?.NamespaceURI);
                newNode.InnerText = extension;
                newNode.Attributes.RemoveAll();
                extensionsNode?.AppendChild(newNode);
            }

            _doc.Save(_path);
        }

        public string Id { get; }

        private readonly XmlNode _node;
        private readonly XmlDocument _doc;
        private readonly string _path;
    }
}