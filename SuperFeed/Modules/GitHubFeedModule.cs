using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class GitHubFeedModule : FeedModuleXml
    {
        // variables
        private string _username;

        public override string SourceName
        {
            get { return "GitHub"; }
        }

        public override string SourceUri
        {
            get { return string.Format("https://github.com/{0}.atom", _username); }
        }

        public GitHubFeedModule(int totalLimit, string username) : base(totalLimit)
        {
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
                    select new FeedItem
                    {
                        SourceName = SourceName,
                        Published = DateTime.Parse(entry.Element(atom + "published").Value),
                        Title = "Via GitHub",
                        Snippet = entry.Element(atom + "title").Value,
                        ViewUri = entry.Element(atom + "link").Attribute("href").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }
    }
}