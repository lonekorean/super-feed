using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodersBlock.SuperFeed
{
    public abstract class FeedModule
    {
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