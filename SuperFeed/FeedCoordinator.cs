using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodersBlock.SuperFeed
{
    public static class FeedCoordinator
    {
        // variables
        private static readonly object _lockable = new object();
        private static Dictionary<string, List<FeedItem>> _feeds = new Dictionary<string, List<FeedItem>>();

        // properties
        public static DateTime StartTime { get; private set; }
        public static int StaggerDelay { get; set; }            // stagger time between initial API calls to sources (in ms)
        public static int IntervalDelay { get; set; }           // pause between repeating API calls to same source (in ms)
        public static int MaxPerSource { get; set; }            // max items returned from GetFeed()
        public static int MaxAllSources { get; set; }           // max items returned from GetTopFeed()
        public static int PromoteRepresentation { get; set; }   // helps GetTopFeed() include items from all sources
        public static int PromoteStreakLimit { get; set; }      // helps GetTopFeed() limit consecutive items from the same source
        public static int PromoteRecent { get; set; }           // helps GetTopFeed() include the freshest items (in days)

        // static constructor
        static FeedCoordinator()
        {
            StartTime = DateTime.Now;

            // defaults
            StaggerDelay = 10000;
            IntervalDelay = 900000;
            MaxPerSource = 100;
            MaxAllSources = 20;
            PromoteRepresentation = 2;
            PromoteStreakLimit = 2;
            PromoteRecent = 365;      
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
                Task.Factory.StartNew(() => RunFeedLoop(feedModule, _feeds.Count * StaggerDelay), TaskCreationOptions.LongRunning);
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

            feed = feed.GetRange(0, Math.Min(feed.Count, MaxPerSource));

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
                foreach (var feedItem in feed.GetRange(0, Math.Min(feed.Count, PromoteRepresentation)))
                {
                    var mergedFeedItem = mergedFeed.First(i => i.ViewUri == feedItem.ViewUri);
                    if(mergedFeedItem != null)
                    {
                        mergedFeedItem.Weight++;
                    }
                }
            }

            // promote streak limit
            var streakCount = 0;
            var mostRecentSourceName = "";
            foreach (var feedItem in mergedFeed)
            {
                if (feedItem.SourceName == mostRecentSourceName)
                {
                    streakCount++;
                }
                else {
                    streakCount = 1;
                    mostRecentSourceName = feedItem.SourceName;
                }

                if (streakCount <= PromoteStreakLimit)
                {
                    feedItem.Weight++;
                }
            }

            // promote recent
            var somewhatCutOff = DateTime.Now.AddDays(-PromoteRecent);
            mergedFeed.Where(x => x.Published >= somewhatCutOff).ToList().ForEach(x => x.Weight++);

            // sort by weight, take the top, resort by published
            mergedFeed.Sort((a, b) => b.CompareTo(a));
            mergedFeed = mergedFeed.Take(Math.Min(mergedFeed.Count, MaxAllSources)).ToList();
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
                Thread.Sleep(IntervalDelay);
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