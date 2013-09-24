using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodersBlock.SuperFeed
{
    public static class FeedCoordinator
    {
        // constants
        private const int STAGGER_DELAY = 10000;            // stagger time between initial API calls to sources (in ms)
        private const int INTERVAL_DELAY = 900000;          // pause between repeating API calls to same source (in ms)
        private const int MAX_PER_SOURCE = 40;              // max items returned from GetFeed()
        private const int MAX_ALL_SOURCES = 20;             // max items returned from GetTopFeed()
        private const int PROMOTE_REPRESENTATION = 2;       // helps GetTopFeed() include items from all sources
        private const int PROMOTE_STREAK_LIMIT = 3;         // helps GetTopFeed() limit consecutive items from the same source
        private const int PROMOTE_ROTATION = 2;             // helps GetTopFeed() rotate through more sources
        private const int PROMOTE_SOMEWHAT_RECENT = 365;    // helps GetTopFeed() include somewhat recent items (in days)
        private const int PROMOTE_VERY_RECENT = 7;          // helps GetTopFeed() include the freshest items (in days)
        
        // variables
        private static readonly object _lockable = new object();
        private static Dictionary<string, List<FeedItem>> _feeds = new Dictionary<string, List<FeedItem>>();

        // properties
        public static DateTime StartTime { get; private set; }

        // static constructor
        static FeedCoordinator()
        {
            StartTime = DateTime.Now;
        }

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
                Task.Factory.StartNew(() => RunFeedLoop(feedModule, _feeds.Count * STAGGER_DELAY), TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Get feed items from a single source.
        /// </summary>
        public static List<FeedItem> GetFeed(string sourceName)
        {
            List<FeedItem> feed;
            lock (_lockable)
            {
                feed = _feeds[sourceName];
            }

            feed = feed.GetRange(0, Math.Min(feed.Count, MAX_PER_SOURCE));

            // return a cloned list
            return feed.Select(x => x.Clone()).ToList();
        }

        /// <summary>
        /// Get feed items from all sources merged together.
        /// </summary>
        public static List<FeedItem> GetMergedFeed()
        {
            var mergedFeed = new List<FeedItem>();
            foreach (var source in _feeds.Keys)
            {
                mergedFeed.AddRange(GetFeed(source));
            }

            // sort by published date descending
            mergedFeed.Sort((a, b) => b.Published.CompareTo(a.Published));

            return mergedFeed;
        }
        
        /// <summary>
        /// Get feed items from all sources merged together, with weighting applied.
        /// </summary>
        public static List<FeedItem> GetTopFeed()
        {
            var mergedFeed = GetMergedFeed();

            // promote representation
            foreach (var feed in _feeds.Values)
            {
                foreach (var feedItem in feed.GetRange(0, Math.Min(feed.Count, PROMOTE_REPRESENTATION)))
                {
                    var mergedFeedItem = mergedFeed.First(i => i.ViewUri == feedItem.ViewUri);
                    if(mergedFeedItem != null)
                    {
                        mergedFeedItem.Weight++;
                    }
                }
            }

            // promote streak limit and rotation
            var streakCount = 0;
            var mostRecentSourceNames = new Queue<string>(PROMOTE_ROTATION);
            foreach (var feedItem in mergedFeed)
            {
                if (!mostRecentSourceNames.Contains(feedItem.SourceName))
                {
                    mostRecentSourceNames.Enqueue(feedItem.SourceName);
                    if (mostRecentSourceNames.Count > PROMOTE_ROTATION)
                    {
                        mostRecentSourceNames.Dequeue();
                        streakCount = 0;
                    }
                }
                if (++streakCount <= PROMOTE_STREAK_LIMIT)
                {
                    feedItem.Weight++;
                }
            }

            // promote somewhat recent
            var somewhatCutOff = DateTime.Now.AddDays(-PROMOTE_SOMEWHAT_RECENT);
            mergedFeed.Where(x => x.Published >= somewhatCutOff).ToList().ForEach(x => x.Weight++);

            // promote very recent
            var veryCutOff = DateTime.Now.AddDays(-PROMOTE_VERY_RECENT);
            mergedFeed.Where(x => x.Published >= somewhatCutOff).ToList().ForEach(x => x.Weight++);

            // sort by weight, take the top, resort by published
            mergedFeed.Sort((a, b) => b.CompareTo(a));
            mergedFeed = mergedFeed.Take(Math.Min(mergedFeed.Count, MAX_ALL_SOURCES)).ToList();
            mergedFeed.Sort((a, b) => b.Published.CompareTo(a.Published));

            return mergedFeed;
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