using System;
using System.Collections.Generic;
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
                        Snippet = GetSnippet(item.Element(mrss + "description").Value),
                        ImageThumbnailUri = (
                            from thumbnail in item.Elements(mrss + "thumbnail")
                            where thumbnail.Attribute("width").Value == "150"
                            select thumbnail.Attribute("url").Value
                        ).Single(),
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

        private DateTime GetPublished(string pubDate)
        {
            // massage the incoming string into something .NET is more inclined to parse
            string massagedPubDate = pubDate
                .Substring(5)                   // remove day of week stuff from beginning
                .Replace("PST", "-08:00")       // replace pacific standard time abbreviation with hour offset to UTC
                .Replace("PDT", "-07:00");      // replace pacific daylight time abbreviation with hour offset to UTC

            return DateTime.Parse(massagedPubDate);
        }
    }
}