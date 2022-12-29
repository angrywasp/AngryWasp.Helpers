using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AngryWasp.Helpers
{
    public static class XHelper
    {
        public delegate void NodeWalker(XElement node);

        public delegate bool ConditionalNodeWalker(XElement node);

        public static XDocument CreateDocument(string rootNode, Dictionary<string, object> namespaces = null, Dictionary<string, object> attributes = null, string[] headerLines = null)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

            if (headerLines != null)
                foreach (string h in headerLines)
                    doc.Add(new XComment(h));

            doc.Add(new XElement(rootNode));

            if (namespaces != null)
                foreach (KeyValuePair<string, object> ns in namespaces)
                    doc.Root.Add(new XAttribute(XNamespace.Xmlns + ns.Key, ns.Value));

            if (attributes != null)
                foreach (KeyValuePair<string, object> a in attributes)
                    CreateAttribute(doc.Root, a.Key, a.Value);

            return doc;
        }

        public static XElement CreateElement(XElement parent, string prefix, string name, string value = null, Dictionary<string, object> attributes = null)
        {
            string[] names = name.Split(new char[] { '.' });

            int i = 0;
            for (i = 0; i < names.Length - 1; i++)
            {
                XElement e;
                string[] nameParts = names[i].Split(new char[] { ':' });

                if (nameParts.Length == 1)
                    e = new XElement(names[i]);
                else
                    e = new XElement(parent.GetNamespaceOfPrefix(nameParts[0]) + nameParts[1]);

                XElement existing = GetNodeByName(parent, e.Name.LocalName);
                if (existing == null)
                {
                    parent.Add(e);
                    parent = e;
                }
                else
                    parent = existing;
            }

            XElement element = null;
            if (prefix == null)
                element = new XElement(names[i], value);
            else
                element = new XElement(parent.GetNamespaceOfPrefix(prefix) + names[i], value);

            parent.Add(element);

            if (attributes != null)
                foreach (KeyValuePair<string, object> a in attributes)
                    CreateAttribute(element, a.Key, a.Value);

            return element;
        }

        public static XAttribute CreateAttribute(XElement parent, string name, object value)
        {
            XAttribute a = null;
            string[] split = name.Split(new char[] { ':' });
            if (split.Length == 1)
                a = new XAttribute(name, value == null ? "null" : value);
            else
                a = new XAttribute(parent.GetNamespaceOfPrefix(split[0]) + split[1], value);

            parent.Add(a);

            return a;
        }

        public static XElement UpdateElement(XElement element, string newPrefix, string newName, string newValue, Dictionary<string, object> attributes = null)
        {
            if (newName != null)
            {
                if (newPrefix != null)
                    element.Name = element.GetNamespaceOfPrefix(newPrefix) + newName;
                else
                    element.Name = newName;
            }

            if (newValue != null)
                element.Value = newValue;

            return UpdateElement(element, attributes);
        }

        public static XElement UpdateElement(XElement element, Dictionary<string, object> attributes)
        {
            foreach (var a in attributes)
            {
                if (element.Attribute(a.Key) == null)
                    element.Add(new XAttribute(a.Key, a.Value));
                else
                    element.Attribute(a.Key).Value = a.Value.ToString();
            }
            return element;
        }

        public static XDocument LoadDocument(string path) =>
            XDocument.Load(path, LoadOptions.PreserveWhitespace);

        public static XDocument LoadText(string text) =>
            XDocument.Parse(text, LoadOptions.PreserveWhitespace);

        public static void Save(XDocument doc, string path)
        {
            XmlWriterSettings s = new XmlWriterSettings();
            s.Indent = true;
            s.NewLineOnAttributes = false;
            s.Encoding = Encoding.GetEncoding(new UTF8Encoding(false).CodePage);
            XmlWriter w = XmlWriter.Create(path, s);
            doc.WriteTo(w);
            w.Flush();
            w.Close();
        }

        public static XElement GetNodeByName(XElement parent, string name)
        {
            string[] names = name.Split(new char[] { '.' });

            for (int i = 0; i < names.Length; i++)
                parent = parent.Elements().Where(e => e.Name.LocalName == names[i]).First();

            return parent;
        }

        public static List<XElement> GetNodesByName(XElement parent, string name)
        {
            string[] names = name.Split(new char[] { '.' });

            int i;
            for (i = 0; i < names.Length - 1; i++)
                parent = parent.Elements().Where(e => e.Name.LocalName == names[i]).First();

            return new List<XElement>(parent.Elements().Where(e => e.Name.LocalName == names[i]));
        }

        public static bool TryParseAttributeData(XElement node, string attributeName, out string result)
        {
            if (!node.HasAttributes || node.Attribute(attributeName) == null)
            {
                result = string.Empty;
                return false;
            }

            result = node.Attribute(attributeName).Value;
            return true;
        }

        public static string GetPathToRoot(XElement node, XElement rootNode, bool qualified)
        {
            XElement p = node;
            string path = (qualified ? string.Format("{0}:{1}", p.GetPrefixOfNamespace(p.Name.NamespaceName), p.Name.LocalName) : p.Name.LocalName);

            while (p.Parent != null)
            {
                if (p.Parent.NodeType == XmlNodeType.Document)
                    break;

                if (rootNode != null && p.Parent == rootNode)
                    break;

                path = (qualified ? string.Format("{0}:{1}", p.Parent.GetPrefixOfNamespace(p.Parent.Name.NamespaceName), p.Parent.Name.LocalName) : p.Parent.Name.LocalName) + "." + path;
                p = p.Parent;
            }

            return path;
        }

        public static void WalkNode(XElement parent, NodeWalker walker, bool topLevelOnly)
        {
            foreach (XElement node in parent.Elements())
            {
                if (node.NodeType == XmlNodeType.Element)
                    walker(node);

                if (!topLevelOnly)
                    WalkNode(node, walker, topLevelOnly);
            }
        }

        /// <summary>
        /// Walks the nodes in a document. When true is returned by the delegate, child nodes of the current node are skipped
        /// To traverse all nodes, either ensure the delegate returns false, or use WalkNode
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="walker"></param>
        /// <param name="topLevelOnly"></param>
        public static void ConditionallyWalkNode(XElement parent, ConditionalNodeWalker walker, bool topLevelOnly)
        {
            foreach (XElement node in parent.Elements())
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    bool shouldSkip = walker(node);
                    if (shouldSkip)
                        continue;
                }

                if (!topLevelOnly)
                    ConditionallyWalkNode(node, walker, topLevelOnly);
            }
        }

        public static bool AttributeExists(XElement parent, string attributeName) =>
            parent.Attribute(attributeName) != null;

        public static string GetAttribute(XElement parent, string attributeName)
        {
            if (parent.Attribute(attributeName) == null)
                return null;

            return parent.Attribute(attributeName).Value;
        }
    }
}