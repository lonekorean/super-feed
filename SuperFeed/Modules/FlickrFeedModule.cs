using System;
using System.Collections.Generic;
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
                        Published = DateTime.Parse(entry.Element(atom + "published").Value),
                        Title = entry.Element(atom + "title").Value,
                        Snippet = GetSnippet(entry.Element(atom + "content").Value),
                        ImageThumbnailUri = GetImageThumbnailUri((
                            from link in entry.Elements(atom + "link")
                            where link.Attribute("rel").Value == "enclosure"
                            select link.Attribute("href").Value
                        ).Single()),
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

        protected override string GetSnippet(string content)
        {
            content = base.GetSnippet(content);
            content = Regex.Replace(content, @"^" + _username + " posted a photo: ?", "");
            return content;
        }

        private string GetImageThumbnailUri(string href)
        {
            return href.Replace("_b.", "_t.");
        }

        private string GetImagePreviewUri(string href)
        {
            return href.Replace("_b.", "_z.");
        }
    }
}