using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodersBlock.SuperFeed
{
    public abstract class FeedModule
    {
        // constants
        protected const int MAX_ATTEMPTS = 3;
        protected const int RETRY_DELAY = 500; // 0.5 seconds

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
    }
}