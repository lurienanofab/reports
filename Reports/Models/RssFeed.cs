using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Reports.Models
{
    public class RssFeed
    {
        public string Version { get; set; }
        public RssChannel Channel { get; set; }

        public XmlDocument ToXml()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<rss/>");

            AddAttributes(xdoc.DocumentElement, new Dictionary<string, string> { { "version", Version } });

            XmlNode channel = AddNode(xdoc.DocumentElement, "channel");

            AddNode(channel, "title", Channel.Title);
            AddNode(channel, "link", Channel.Link);
            AddNode(channel, "description", Channel.Description);
            AddNode(channel, "pubDate", ConvertDate(Channel.PubDate));
            AddNode(channel, "webMaster", Channel.WebMaster);
            AddNode(channel, "lastBuildDate", ConvertDate(Channel.LastBuildDate));

            AddItems(channel);

            XmlDeclaration dec = xdoc.CreateXmlDeclaration("1.0", null, null);
            xdoc.InsertBefore(dec, xdoc.DocumentElement);

            return xdoc;
        }

        private XmlNode AddNode(XmlNode parent, string name, string text = null, IDictionary<string, string> attributes = null)
        {
            XmlNode node = parent.OwnerDocument.CreateElement(name);
            if (!string.IsNullOrEmpty(text))
                node.InnerText = text;
            AddAttributes(node, attributes);
            parent.AppendChild(node);
            return node;
        }

        private void AddAttributes(XmlNode node, IDictionary<string, string> attributes)
        {
            if (attributes == null)
                return;

            foreach (KeyValuePair<string, string> kvp in attributes)
            {
                XmlAttribute attr = node.OwnerDocument.CreateAttribute(kvp.Key);
                attr.Value = kvp.Value;
                node.Attributes.Append(attr);
            }
        }

        private string GetXmlString(XmlDocument xdoc)
        {
            MemoryStream ms = new MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            settings.NewLineChars = "\r\n";
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.Encoding = Encoding.UTF8;

            using (XmlWriter writer = XmlWriter.Create(ms, settings))
                xdoc.Save(writer);

            ms.Position = 0;

            using (StreamReader reader = new StreamReader(ms))
                return reader.ReadToEnd();
        }

        private string ConvertDate(DateTime d)
        {
            DateTime utc = d.ToUniversalTime();
            return utc.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'");
        }

        private void AddItems(XmlNode channel)
        {
            if (Channel.Items != null && Channel.Items.Count > 0)
            {
                foreach (RssItem i in Channel.Items)
                {
                    XmlNode item = AddNode(channel, "item");
                    AddNode(item, "title", i.Title);
                    AddNode(item, "link", i.Link);
                    AddNode(item, "description", i.Description);
                    AddNode(item, "pubDate", ConvertDate(i.PubDate));
                    AddNode(item, "guid", i.Guid);
                }
            }
        }

        public override string ToString()
        {
            XmlDocument xdoc = ToXml();
            return GetXmlString(xdoc);
        }
    }

    public class RssChannel
    {
        public string Title { get; set; }
        public DateTime PubDate { get; set; }
        public DateTime LastBuildDate { get; set; }
        public string WebMaster { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public List<RssItem> Items { get; set; }
    }

    public class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Guid { get; set; }
        public DateTime PubDate { get; set; }
    }
}
