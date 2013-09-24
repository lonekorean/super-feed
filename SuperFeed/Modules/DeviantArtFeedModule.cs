using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class DeviantArtFeedModule : FeedModuleXml
    {
        private string _username;

        public override string SourceName
        {
            get { return "DeviantArt"; }
        }

        public override string SourceUri
        {
            get { return string.Format("http://backend.deviantart.com/rss.xml?q=gallery:{0}", _username); }
        }

        public DeviantArtFeedModule(int totalLimit, string username) : base(totalLimit)
        {
            _username = username;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                XNamespace mrss = "http://search.yahoo.com/mrss/";

                items = (
                    from item in doc.Descendants("item")
                    select new FeedItem(this)
                    {
                        Published = GetPublished(item.Element("pubDate").Value),
                        Title = item.Element("title").Value,
                        Snippet = item.Element(mrss + "description").Value,
                        ImagePreviewUri = (
                            from content in item.Elements(mrss + "content")
                            where content.Attribute("medium").Value == "image"
                            select content.Attribute("url").Value
                        ).Single(),
                        ViewUri = item.Element("link").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string published)
        {
            // sample: "Sun, 08 Sep 2013 14:17:34 PDT"
            published = published.Replace("PST", "-08:00").Replace("PDT", "-07:00");
            var parsed = DateTime.ParseExact(published, @"ddd, dd MMM yyyy HH\:mm\:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }
    }
}