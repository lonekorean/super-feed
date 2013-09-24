using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class FlickrFeedModule : FeedModuleXml
    {
        private string _id;
        private string _username;

        public override string SourceName
        {
            get { return "Flickr"; }
        }

        public override string SourceUri
        {
            get { return string.Format("http://api.flickr.com/services/feeds/photos_public.gne?id={0}", _id); }
        }

        public FlickrFeedModule(int totalLimit, string id, string username) : base(totalLimit)
        {
            _id = id;
            _username = username;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                XNamespace atom = "http://www.w3.org/2005/Atom";

                items = (
                    from entry in doc.Descendants(atom + "entry")
                    select new FeedItem(this)
                    {
                        Published = GetPublished(entry.Element(atom + "published").Value),
                        Title = entry.Element(atom + "title").Value,
                        Snippet = GetSnippet(entry.Element(atom + "content").Value),
                        ImagePreviewUri = GetImagePreviewUri((
                            from link in entry.Elements(atom + "link")
                            where link.Attribute("rel").Value == "enclosure"
                            select link.Attribute("href").Value
                        ).Single()),
                        ViewUri = (
                            from link in entry.Elements(atom + "link")
                            where link.Attribute("rel").Value == "alternate"
                            select link.Attribute("href").Value
                        ).Single()
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string published)
        {
            // sample: "2013-06-25T06:05:46Z"
            var parsed = DateTime.ParseExact(published, @"yyyy-MM-ddTHH\:mm\:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetSnippet(string content)
        {
            content = Regex.Replace(content, @"<.+?>", " ");
            content = Regex.Replace(content, @"\s+", " ");
            content = Regex.Replace(content, _username + " posted a photo:", "");
            content = content.Trim();
            return content;
        }

        private string GetImagePreviewUri(string href)
        {
            return href.Replace("_b.", "_z.");
        }
    }
}