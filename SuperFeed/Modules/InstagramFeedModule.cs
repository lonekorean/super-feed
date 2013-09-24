using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace CodersBlock.SuperFeed.Modules
{
    public class InstagramFeedModule : FeedModuleJson
    {
        private class Model
        {
            public ModelRoot root { get; set; }
        }

        private class ModelRoot
        {
            public List<ModelData> data { get; set; }
        }

        private class ModelData
        {
            public string created_time { get; set; }
            public ModelCaption caption { get; set; }
            public string link { get; set; }
            public ModelImages images { get; set; }
        }

        private class ModelCaption
        {
            public string text { get; set; }
        }

        private class ModelImages
        {
            public ModelStandardResolution standard_resolution { get; set; }
        }

        private class ModelStandardResolution
        {
            public string url { get; set; }
        }

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

        protected override HttpWebRequest GetRequest()
        {
            var request = (HttpWebRequest)WebRequest.Create(SourceUri);
            return request;
        }

        protected override List<FeedItem> ParseDocument(Dictionary<string, object> doc)
        {
            var feedItems = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                var serializer = new JavaScriptSerializer();
                var model = serializer.ConvertToType<Model>(doc);
                for (var i = 0; i < Math.Min(_totalLimit, model.root.data.Count); i++)
                {
                    var modelItem = model.root.data[i];
                    feedItems.Add(new FeedItem(this)
                    {
                        Published = GetPublished(modelItem.created_time),
                        Title = "Via Instagram",
                        Snippet = modelItem.caption.text,
                        ImagePreviewUri = modelItem.images.standard_resolution.url,
                        ViewUri = modelItem.link
                    });
                }
            }

            return feedItems;
        }

        private DateTime GetPublished(string pubDate)
        {
            // sample: "1379203714"
            var seconds = int.Parse(pubDate);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(seconds);
        }
    }
}