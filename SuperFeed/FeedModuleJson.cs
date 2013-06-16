using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;

namespace CodersBlock.SuperFeed
{
    public abstract class FeedModuleJson : FeedModule
    {
        public FeedModuleJson(int totalLimit) : base(totalLimit) { }

        public override List<FeedItem> GetItems()
        {
            var serializer = new JavaScriptSerializer();
            Dictionary<string, object> doc = null;
            int attempts = 0;
            do
            {
                try
                {
                    var stream = GetRequest().GetResponse().GetResponseStream();
                    var reader = new StreamReader(stream);
                    var json = reader.ReadToEnd();
                    json = "{root:" + json + "}"; // ensures JSON can always be deserialized into a dictionary
                    doc = serializer.Deserialize<Dictionary<string, object>>(json);
                }
                catch
                {
                    Thread.Sleep(RETRY_DELAY);
                }
                attempts++;
            }
            while (attempts < MAX_ATTEMPTS && doc == null);

            return ParseDocument(doc);
        }

        protected abstract HttpWebRequest GetRequest();
        protected abstract List<FeedItem> ParseDocument(Dictionary<string, object> doc);
    }
}