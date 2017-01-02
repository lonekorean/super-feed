using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class WordPressFeedModule : FeedModuleXml
    {
        private string _url;

        public override string SourceName
        {
            get { return "WordPress"; }
        }

        public override string SourceUri
        {
            get { return _url; }
        }

        public WordPressFeedModule(int totalLimit, string url) : base(totalLimit)
        {
            _url = url;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                items = (
                    from item in doc.Descendants("item")
                    select new FeedItem(this)
                    {
                        Published = GetPublished(item.Element("pubDate").Value),
                        Title = item.Element("title").Value,
                        Snippet = GetSnippet(item.Element("description").Value),
                        ImagePreviewUri = GetImagePreviewUri(item.Element("enclosure")),
                        ViewUri = item.Element("link").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string pubDate)
        {
            // sample: "Thu, 06 Feb 2014 04:59:01 +0000"
            var parsed = DateTime.ParseExact(pubDate, @"ddd, dd MMM yyyy HH\:mm\:ss zzzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetSnippet(string description)
        {
            description = description.Replace(" [&#8230;]", "...");             // remove trailing ellipses
            if (description.Length > 250)                                       // check if too long
            {
                description = description.Substring(0, 250);                    // reduce length
                description = description.TrimEnd();                            // trim ending whitespace
                description = Regex.Replace(description, @" [^ ]+$", "...");    // replace last (possibly hanging) word with ellipses
            }
            return description;
        }

        private string GetImagePreviewUri(XElement enclosure)
        {
            return enclosure == null ? null : enclosure.Attribute("url").Value;
        }
    }
}