using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            return description.Replace(" [&#8230;]", "...");
        }
    }
}