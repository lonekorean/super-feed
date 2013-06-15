using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace CodersBlock.SuperFeed.Modules
{
    public class InstagramFeedModule : FeedModuleJson
    {
        private class Model
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
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                var serializer = new JavaScriptSerializer();
                var model = serializer.ConvertToType<Model>(doc);
                for (var i = 0; i < Math.Min(_totalLimit, model.data.Count); i++)
                {
                    var data = model.data[i];
                    items.Add(new FeedItem()
                    {
                        SourceName = SourceName,
                        Published = GetPublished(data.created_time),
                        Title = "Via Instagram",
                        Snippet = data.caption.text,
                        ImageThumbnailUri = data.link,
                        ImagePreviewUri = data.images.standard_resolution.url,
                        ViewUri = data.link
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