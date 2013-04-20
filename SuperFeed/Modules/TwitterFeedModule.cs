using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class TwitterFeedModule : FeedModuleXml
    {
        // variables
        private string _username;

        public override string SourceName
        {
            get { return "Twitter"; }
        }

        public override string SourceUri
        {
            get { return string.Format("https://api.twitter.com/1/statuses/user_timeline.xml?screen_name={0}&include_rts=1&count={1}", _username, _totalLimit); }
        }

        public TwitterFeedModule(int totalLimit, string username) : base(totalLimit)
        {
            _username = username;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                items = (
                    from status in doc.Descendants("status")
                    select new FeedItem
                    {
                        SourceName = SourceName,
                        Published = GetPublished(status.Element("created_at").Value),
                        Title = GetTitle(status.Element("source").Value),
                        Snippet = GetSnippet(status.Element("text").Value),
                        ViewUri = string.Format("https://twitter.com/{0}/status/{1}", _username, status.Element("id").Value)
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string pubDate)
        {
            return DateTime.ParseExact(pubDate, "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
        }

        private string GetTitle(string source)
        {
            if (source == "web")
            {
                source = "Web";
            }
            source = "Via " + Regex.Replace(source, "<.+?>", "");
            return source;
        }

        protected override string GetSnippet(string title)
        {
            // disregard all that stuff in base class's GetSnippet()
            title = Regex.Replace(title, @"^" + _username + ": ", "");
            title = Regex.Replace(title, @"https?://\w+\.\w+/\w+", "<a href=\"$0\" target=\"_blank\">$0</a>");
            title = Regex.Replace(title, @"@(\w+)", "<a href=\"http://twitter.com/$1\" target=\"_blank\">$0</a>");
            title = Regex.Replace(title, @"#(\w+)", "<a href=\"http://twitter.com/search?q=%23$1\" target=\"_blank\">$0</a>");
            return title;
        }
    }
}