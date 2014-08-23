using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class CodePenFeedModule : FeedModuleXml
    {
        private string _username;

        public override string SourceName
        {
            get { return "CodePen"; }
        }

        public override string SourceUri
        {
            get { return string.Format("http://codepen.io/{0}/public/feed/", _username); }
        }

        public CodePenFeedModule(int totalLimit, string username) : base(totalLimit)
        {
            _username = username;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                XNamespace dc = "http://purl.org/dc/elements/1.1/";

                items = (
                    from item in doc.Descendants("item")
                    select new FeedItem(this)
                    {
                        Published = GetPublished(item.Element(dc + "date").Value),
                        Title = item.Element("title").Value,
                        Snippet = GetSnippet(item.Element("description").Value),
                        ImagePreviewUri = GetImagePreviewUri(item.Element("link").Value),
                        ViewUri = item.Element("link").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string date)
        {
            // sample: "2014-07-22T05:35:16-07:00"
            date = Regex.Replace(date, @"\s+", "");
            var parsed = DateTime.ParseExact(date, @"yyyy-MM-ddTHH\:mm\:sszzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetSnippet(string description)
        {
            // only want a particular paragraph
            var paragraphs = Regex.Split(description, @"\s*</p>\s*<p>\s*");
            description = paragraphs[2];

            // consolidate whitespace
            description = Regex.Replace(description, @"\s+", " ");

            // markdown for links
            description = Regex.Replace(description, @"\[(.*?)\]\((.*?)\)", "<a href=\"$2\" target=\"_blank\">$1</a>");

            return description;
        }

        private string GetImagePreviewUri(string link)
        {
            return link + "/image/large.png";
        }
    }
}