using System;
using CodersBlock.SuperFeed;
using CodersBlock.SuperFeed.Modules;

namespace SuperFeedConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting feed modules...");

            FeedCoordinator.StartFeeds(
                new BloggerFeedModule(20, "codersblock"),
                new DribbbleFeedModule(20, "lonekorean"),
                new FlickrFeedModule(20, "44589846@N00", "lonekorean"),
                new GitHubFeedModule(20, "lonekorean"),
                //new InstagramFeedModule(20, ""),
                new LastFMFeedModule(20, "25121b1fa3ed5de846be9573da41d858", "lonekorean"),
                //new TwitterFeedModule(20, "", "", "", "", ""),
                new WordPressFeedModule(20, "http://codersblock.com/blog/feed/")
            );

            while (true)
            {
                var list = FeedCoordinator.GetWeightedFeed(20);
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    Console.WriteLine("Item #" + (i + 1));
                    Console.WriteLine(GetDisplayLine("Weight", item.Weight.ToString()));
                    Console.WriteLine(GetDisplayLine("SourceName", item.SourceName));
                    Console.WriteLine(GetDisplayLine("Published", item.Published.ToString()));
                    Console.WriteLine(GetDisplayLine("Title", item.Title));
                    Console.WriteLine(GetDisplayLine("Snippet", item.Snippet));
                    Console.WriteLine(GetDisplayLine("ImagePreviewUri", item.ImagePreviewUri));
                    Console.WriteLine(GetDisplayLine("ViewUri", item.ViewUri));
                    Console.WriteLine();
                }

                Console.WriteLine("Press any key to refresh...");
                Console.ReadKey(true);
            }
        }

        static string GetDisplayLine(string label, string value)
        {
            label = "  " + (label + ":").PadRight(19);
            value = value ?? "";
            value = (value.Length <= 58 ? value : value.Substring(0, 55) + "...");

            return label + value;
        }
    }
}
