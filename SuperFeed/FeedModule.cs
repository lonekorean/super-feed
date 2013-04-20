using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodersBlock.SuperFeed
{
    public abstract class FeedModule
    {
        // constants
        protected const int MAX_ATTEMPTS = 3;
        protected const int RETRY_DELAY = 500; // 0.5 seconds
        protected const int SNIPPET_LENGTH = 400;

        // variables
        protected int _totalLimit;

        // properties
        public abstract string SourceName { get; }
        public abstract string SourceUri { get; }

        public FeedModule(int totalLimit)
        {
            _totalLimit = totalLimit;
        }

        public abstract List<FeedItem> GetItems();

        protected virtual string GetSnippet(string content)
        {
            content = Regex.Replace(content, @"<.+?>", " ");            // remove tags
            content = Regex.Replace(content, @"\s+", " ");              // consolidate whitespace
            content = content.Trim();                                   // trim whitespace
            if (content.Length > SNIPPET_LENGTH)                        // check if too long
            {
                content = content.Substring(0, SNIPPET_LENGTH);         // reduce length
                content = content.TrimEnd();                            // trim ending whitespace (again)
                content = Regex.Replace(content, @" [^ ]+$", "...");    // replace last (possibly hanging) word with ellipses
            }
            return content;
        }
    }
}