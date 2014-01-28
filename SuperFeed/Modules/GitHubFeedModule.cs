using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class GitHubFeedModule : FeedModuleXml
    {
        private const int MAX_MESSAGES = 3;

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
                    select new FeedItem(this)
                    {
                        Published = GetPublished(entry.Element(atom + "published").Value),
                        Title = GetTitle(entry.Element(atom + "id").Value),
                        Snippet = GetSnippet(entry.Element(atom + "content").Value),
                        ViewUri = entry.Element(atom + "link").Attribute("href").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }
            
            return items;
        }

        private DateTime GetPublished(string published)
        {
            // sample: "2013-09-15T21:03:39Z"
            var parsed = DateTime.ParseExact(published, @"yyyy-MM-ddTHH\:mm\:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetTitle(string id)
        {
            id = Regex.Match(id, @":(\w+)/").Groups[1].Value;   // extract the part we want
            id = Regex.Replace(id, "([a-z])([A-Z])", "$1 $2");  // turn pascal casing into actual spaces
            return id;
        }

        private string GetSnippet(string content)
        {
            content = HttpUtility.HtmlDecode(content);

            // massage title line from content
            var titleRegex = new Regex(@"<div class=""title"">\s*(.+?)\s*</div>");
            var snippet = titleRegex.Match(content).Groups[1].Value
                .Replace("<span>", "")
                .Replace("</span>", "")
                .Replace(" class=\"css-truncate css-truncate-target\"", "")
                .Replace("href=\"/", "href=\"https://github.com/"); // make relative URLs absolute
            snippet = Regex.Replace(snippet, "<a (.*?)>", "<a $1 target=\"_blank\">");

            // massage commit messages (if present) from content
            var messagesRegex = new Regex(@"<div class=""message"">\s*<blockquote>\s*(.+?)\s*</blockquote>");
            var messageMatches = messagesRegex.Matches(content);
            if (messageMatches.Count > 0)
            {
                var limit = Math.Min(messageMatches.Count, MAX_MESSAGES);
                var messages = new List<string>(limit);
                for (var i = 0; i < limit; i++)
                {
                    messages.Add(messageMatches[i].Groups[1].Value);
                }
                snippet += "<ul><li>" + string.Join("</li><li>", messages) + "</li></ul>";
            }

            return snippet;
        }
    }
}