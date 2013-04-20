using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class BloggerFeedModule : FeedModuleXml
    {
        // variables
        private string _subdomain;

        public override string SourceName
        {
            get { return "Blogger"; }
        }

        public override string SourceUri
        {
            get { return string.Format("http://{0}.blogspot.com/feeds/posts/default?max-results={1}", _subdomain, _totalLimit); }
        }

        public BloggerFeedModule(int totalLimit, string subdomain) : base(totalLimit)
        {
            _subdomain = subdomain;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                XNamespace atom = "http://www.w3.org/2005/Atom";

                items = (
                    from entry in doc.Descendants(atom + "entry")
                    select new FeedItem
                    {
                        SourceName = SourceName,
                        Published = DateTime.Parse(entry.Element(atom + "published").Value),
                        Title = entry.Element(atom + "title").Value,
                        Snippet = GetSnippet(entry.Element(atom + "content").Value),
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
            content = Regex.Replace(content, @"(^|<br />)<b>.+?</b><br />", "");
            content = base.GetSnippet(content);
            return content;
        }
    }
}