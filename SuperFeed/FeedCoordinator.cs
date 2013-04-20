using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CodersBlock.SuperFeed
{
    public static class FeedCoordinator
    {
        // constants
        private const int STAGGER_DELAY = 10000; // 10 seconds
        private const int INTERVAL_DELAY = 900000; // 15 minutes
        private const int CONSECUTIVE_LIMIT = 2;
        private const int MAX_FINAL_ITEMS = 20;

        // variables
        private static readonly object _lockable = new object();
        private static Dictionary<string, List<FeedItem>> _feeds = new Dictionary<string, List<FeedItem>>();

        /// <summary>
        /// Kicks off a feed module to run in a loop in its own thread.
        /// </summary>
        public static void StartFeedModule(FeedModule feedModule)
        {
            // make sure this feed module isn't already added and started
            if (!_feeds.ContainsKey(feedModule.SourceName))
            {
                // add feed module
                _feeds.Add(feedModule.SourceName, new List<FeedItem>());

                // run immediately on main thread in a blocking manner
                // this ensures that we have feeds ready to go before any page requests are handled
                UpdateFeed(feedModule);

                // start async loop
                Task.Factory.StartNew(() => RunFeedLoop(feedModule, _feeds.Count * STAGGER_DELAY));
            }
        }

        /// <summary>
        /// Get list of feed items from a particular source.
        /// </summary>
        public static List<FeedItem> GetFeed(string sourceName)
        {
            List<FeedItem> feed;
            lock (_lockable)
            {
                feed = new List<FeedItem>(_feeds[sourceName]);
            }
            return feed;
        }

        /// <summary>
        /// Get list of feed items from all sources merged together, sorted by published date.
        /// </summary>
        public static List<FeedItem> GetMergedFeed()
        {
            var feed = new List<FeedItem>();
            foreach (var source in _feeds.Keys)
            {
                feed.AddRange(GetFeed(source));
            }

            // sort by published date descending
            feed.Sort((a, b) => b.Published.CompareTo(a.Published));

            return feed;
        }

        /// <summary>
        /// Returns a merged feed, but additionally processed to limit consecutive items from the same source.
        /// </summary>
        /// <returns></returns>
        public static List<FeedItem> GetMergedAndBalancedFeed()
        {
            var mergedFeed = GetMergedFeed();
            var balancedFeed = new List<FeedItem>();

            string mostRecentSourceName = "";
            int consecutiveCount = 0;
            foreach (var feedItem in mergedFeed)
            {
                if (feedItem.SourceName != mostRecentSourceName)
                {
                    consecutiveCount = 0;
                    mostRecentSourceName = feedItem.SourceName;
                }

                if (++consecutiveCount <= CONSECUTIVE_LIMIT)
                {
                    balancedFeed.Add(feedItem);
                }
            }

            return balancedFeed.GetRange(0, Math.Min(balancedFeed.Count, MAX_FINAL_ITEMS));
        }

        /// <summary>
        /// Continues loading a list of feed items in a set interval forever.
        /// </summary>
        private static void RunFeedLoop(FeedModule feedModule, int startDelay)
        {
            Thread.Sleep(startDelay);
            while (true)
            {
                Thread.Sleep(INTERVAL_DELAY);
                UpdateFeed(feedModule);
            }
        }

        /// <summary>
        /// Tells feed to get its items for update.
        /// </summary>
        private static void UpdateFeed(FeedModule feedModule)
        {
            var feed = feedModule.GetItems();
            if (feed.Count != 0)
            {
                lock (_lockable)
                {
                    _feeds[feedModule.SourceName] = feed;
                }
            }
        }
    }
}