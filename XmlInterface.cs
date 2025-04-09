using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ranker
{
    internal class XmlInterface
    {
        private readonly XmlDocument doc;
        private XmlElement? root;
        private readonly string path;

        public XmlInterface(string xmlName)
        {
            path = $"../../../{xmlName}";
            doc = new XmlDocument();
            doc.Load(path);
            root = doc.DocumentElement;
        }

        public List<string> QuickReadXml(string nodeName)
        {
            List<string> result = [];
            if (root == null) { return result; }
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == nodeName)
                {
                    foreach (XmlNode subNode in node.ChildNodes) { result.Add(subNode.InnerText); }
                    break;
                }
            }
            return result;
        }

        public void Refresh()
        {
            doc.Save(path);
            doc.Load(path);
            root = doc.DocumentElement;
        }

        public bool IsItemPresent(List<string> itemParts, string nodeName)
        {
            List<string> compItemParts = [];
            if (root == null) { return false; }
            foreach (XmlNode node in root.ChildNodes)
            {
                if (compItemParts.Count > 0) { compItemParts.Clear(); }
                if (node.Name == nodeName)
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    { 
                        if (subNode.Name != "ELO") { compItemParts.Add(subNode.InnerText); }
                    }
                    for (int i = 0; i < itemParts.Count; i++)
                    {
                        if (itemParts[i] != compItemParts[i]) { break; }
                        else if (i == itemParts.Count - 1) { return true; }
                    }
                }
            }
            return false;
        }

        public XmlElement CreateElement(string name, string? value = null)
        {
            XmlElement newElement = doc.CreateElement(name);
            if (value != null) { newElement.InnerText = value; }
            return newElement;
        }

        public void AttachToRoot(XmlElement element)
        {
            if (root == null) { return; }
            root.AppendChild(element);
        }

        public List<XmlNode> GetNonTemplateNodes() 
        {
            List<XmlNode> nodes = [];
            if (root == null) return [];
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name != "Template") { nodes.Add(node); }
            }
            return nodes;
        }
    }
}
