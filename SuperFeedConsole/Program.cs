using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodersBlock.SuperFeed;
using CodersBlock.SuperFeed.Modules;

namespace SuperFeedConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting feed modules...");

            FeedCoordinator.StartFeedModule(new BloggerFeedModule(20, "codersblock"));
            FeedCoordinator.StartFeedModule(new FlickrFeedModule(20, "44589846@N00", "lonekorean"));
            FeedCoordinator.StartFeedModule(new DeviantArtFeedModule(20, "lonekorean"));
            FeedCoordinator.StartFeedModule(new GitHubFeedModule(30, "lonekorean"));

            // the following are commented out because they requires private tokens or keys
            //FeedCoordinator.StartFeedModule(new TwitterFeedModule(80, ...));
            //FeedCoordinator.StartFeedModule(new InstagramFeedModule(20, ...));

            while (true)
            {
                Console.Clear();
                //var list = FeedCoordinator.GetMergedAndBalancedFeed();
                var list = FeedCoordinator.GetTopFeed();
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    Console.WriteLine("Item #" + (i + 1));
                    Console.WriteLine(GetDisplayLine("SourceName", item.SourceName));
                    Console.WriteLine(GetDisplayLine("Published", item.Published.ToString()));
                    Console.WriteLine(GetDisplayLine("Title", item.Title));
                    Console.WriteLine(GetDisplayLine("Snippet", item.Snippet));
                    Console.WriteLine(GetDisplayLine("ImageThumbnailUri", item.ImageThumbnailUri));
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
