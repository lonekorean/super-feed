using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class BloggerFeedModule : FeedModuleXml
    {
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
                    select new FeedItem(this)
                    {
                        Published = GetPublished(entry.Element(atom + "published").Value),
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

        private DateTime GetPublished(string published)
        {
            // sample: "2013-09-18T20:49:00.000-07:00"
            var parsed = DateTime.ParseExact(published, @"yyyy-MM-ddTHH\:mm\:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetSnippet(string content)
        {
            content = Regex.Replace(content, @"<.+?>", " ");            // remove tags
            content = Regex.Replace(content, @"\s+", " ");              // consolidate whitespace
            content = content.Trim();                                   // trim whitespace
            if (content.Length > 400)                                   // check if too long (probably is)
            {
                content = content.Substring(0, 400);                    // reduce length
                content = content.TrimEnd();                            // trim ending whitespace (again)
                content = Regex.Replace(content, @" [^ ]+$", "...");    // replace last (possibly hanging) word with ellipses
            }
            return content;
        }
    }
}