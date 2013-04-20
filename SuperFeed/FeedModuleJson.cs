using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed
{
    public abstract class FeedModuleJson : FeedModule
    {
        public FeedModuleJson(int totalLimit) : base(totalLimit) { }

        public override List<FeedItem> GetItems()
        {
            var client = new WebClient();
            var serializer = new JavaScriptSerializer();
            Dictionary<string, object> doc = null;
            int attempts = 0;
            do
            {
                try
                {
                    var json = client.DownloadString(SourceUri);
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

        protected abstract List<FeedItem> ParseDocument(Dictionary<string, object> doc);
    }
}