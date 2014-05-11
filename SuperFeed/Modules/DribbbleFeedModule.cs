using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class DribbbleFeedModule : FeedModuleXml
    {
        private string _username;

        public override string SourceName
        {
            get { return "Dribbble"; }
        }

        public override string SourceUri
        {
            get { return string.Format("https://dribbble.com/{0}/shots.rss", _username); }
        }

        public DribbbleFeedModule(int totalLimit, string username) : base(totalLimit)
        {
            _username = username;
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
                        ImagePreviewUri = GetImagePreviewUri(item.Element("description").Value),
                        ViewUri = item.Element("link").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string pubDate)
        {
            // sample: "Thu, 08 May 2014 11:19:01 -0400"
            var parsed = DateTime.ParseExact(pubDate, @"ddd, dd MMM yyyy HH\:mm\:ss zzzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetSnippet(string description)
        {
            return Regex.Match(description, @"<p>(.*?)</p>").Groups[1].Value;
        }

        private string GetImagePreviewUri(string description)
        {
            return Regex.Match(description, @"<img.*?src=""(.*?)""").Groups[1].Value;
        }
    }
}