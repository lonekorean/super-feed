using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodersBlock.SuperFeed
{
    public static class FeedCoordinator
    {
        // variables
        private static ConcurrentDictionary<string, List<FeedItem>> _feeds = new ConcurrentDictionary<string, List<FeedItem>>();

        // properties
        public static DateTime StartTime { get; private set; }
        public static int StartWaitTime { get; set; }
        public static int IntervalDelay { get; set; }
        public static int RetryAttempts { get; set; }
        public static int RetryDelay { get; set; }
        public static int WeightedMinPerSource { get; set; }
        public static int WeightedStreakLimit { get; set; }

        // static constructor
        static FeedCoordinator()
        {
            StartTime = DateTime.Now;

            // defaults
            StartWaitTime = 3000;
            IntervalDelay = 900000; // 15 minutes
            RetryAttempts = 3;
            RetryDelay = 500;
            WeightedMinPerSource = 2;
            WeightedStreakLimit = 2;
        }
        
        /// <summary>
        /// Kicks off multiple feed modules together.
        /// </summary>
        public static void StartFeeds(params FeedModule[] feedModules)
        {
            var tasks = new List<Task>(feedModules.Length);
            foreach (var feedModule in feedModules)
            {
                tasks.Add(Task.Factory.StartNew(() => FeedCoordinator.StartFeed(feedModule)));
            }
            Task.WaitAll(tasks.ToArray(), StartWaitTime);
        }

        /// <summary>
        /// Get feed items from a single source, or null if source doesn't exist.
        /// </summary>
        public static List<FeedItem> GetFeed(string sourceName, int count)
        {
            List<FeedItem> feed;
            if (_feeds.TryGetValue(sourceName, out feed))
            {
                // get list of clones, not shared references
                feed = feed.Take(count).Select(i => i.Clone()).ToList();
            }
            return feed;
        }

        /// <summary>
        /// Get feed items from all sources merged together.
        /// </summary>
        public static List<FeedItem> GetMergedFeed(int count)
        {
            var mergedFeed = new List<FeedItem>();
            foreach (var sourceName in _feeds.Keys)
            {
                mergedFeed.AddRange(GetFeed(sourceName, int.MaxValue));
            }

            // sort by published date descending
            mergedFeed.Sort((a, b) => b.Published.CompareTo(a.Published));
            return mergedFeed.Take(count).ToList();
        }
        
        /// <summary>
        /// Get feed items from all sources merged together, with weighting applied.
        /// </summary>
        public static List<FeedItem> GetWeightedFeed(int count)
        {
            var weightedFeed = GetMergedFeed(int.MaxValue);

            // apply min per source
            foreach (var sourceName in _feeds.Keys)
            {
                weightedFeed.Where(i => i.SourceName == sourceName).Take(WeightedMinPerSource).ToList().ForEach(i => i.Weight++);
            }

            // apply streak limit
            var streakCount = 0;
            var mostRecentSourceName = "";
            foreach (var feedItem in weightedFeed)
            {
                if (feedItem.SourceName == mostRecentSourceName)
                {
                    streakCount++;
                }
                else {
                    streakCount = 1;
                    mostRecentSourceName = feedItem.SourceName;
                }

                if (streakCount <= WeightedStreakLimit)
                {
                    feedItem.Weight++;
                }
            }

            // sort by weight, take the top, resort by published
            weightedFeed.Sort((a, b) => b.CompareTo(a));
            weightedFeed = weightedFeed.Take(count).ToList();
            weightedFeed.Sort((a, b) => b.Published.CompareTo(a.Published));
            return weightedFeed;
        }

        /// <summary>
        /// Kicks off a feed module to run in a loop in its own thread.
        /// </summary>
        private static void StartFeed(FeedModule feedModule)
        {
            if(_feeds.TryAdd(feedModule.SourceName, new List<FeedItem>()))
            {
                // update immediately, then kick off task to keep updating
                UpdateFeed(feedModule);
                Task.Factory.StartNew(() => RunFeedLoop(feedModule), TaskCreationOptions.LongRunning);
            }
        }


        /// <summary>
        /// Continues loading a list of feed items in a set interval forever.
        /// </summary>
        private static void RunFeedLoop(FeedModule feedModule)
        {
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
            _feeds[feedModule.SourceName] = feedModule.GetItems();
        }
    }
}