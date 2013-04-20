using System;
using System.Collections;
using System.Collections.Generic;

namespace CodersBlock.SuperFeed.Modules
{
    public class InstagramFeedModule : FeedModuleJson
    {
        // variables
        private string _token;

        public override string SourceName
        {
            get { return "Instagram"; }
        }

        public override string SourceUri
        {
            get { return string.Format("https://api.instagram.com/v1/users/self/feed?access_token={0}", _token); }
        }

        public InstagramFeedModule(int totalLimit, string token) : base(totalLimit)
        {
            _token = token;
        }

        protected override List<FeedItem> ParseDocument(Dictionary<string, object> doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                var shots = (ArrayList)doc["data"];
                for (var i = 0; i < Math.Min(_totalLimit, doc.Count); i++)
                {
                    var shot = (Dictionary<string, object>)shots[i];
                    items.Add(new FeedItem()
                    {
                        // TODO: this code is not fun, make it better
                        SourceName = SourceName,
                        Published = GetPublished((string)shot["created_time"]),
                        Title = "Via Instagram",
                        Snippet = (string)((Dictionary<string, object>)shot["caption"])["text"],
                        ImageThumbnailUri = (string)shot["link"],
                        ImagePreviewUri = (string)((Dictionary<string, object>)((Dictionary<string, object>)shot["images"])["standard_resolution"])["url"],
                        ViewUri = (string)shot["link"]
                    });
                }
            }
            
            return items;
        }

        private DateTime GetPublished(string pubDate)
        {
            var seconds = int.Parse(pubDate);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(seconds).AddHours(-4); // shift from GMT to EST
        }
    }
}