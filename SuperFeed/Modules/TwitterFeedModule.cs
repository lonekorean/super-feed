using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class TwitterFeedModule : FeedModuleJson
    {
        private class Model
        {
            public List<ModelRoot> root { get; set; }
        }

        private class ModelRoot
        {
            public string created_at { get; set; }
            public string source { get; set; }
            public string text { get; set; }
            public long id { get; set; }
        }

        private string _consumerKey;
        private string _consumerSecret;
        private string _accessToken;
        private string _accessTokenSecret;
        private string _username;

        public override string SourceName
        {
            get { return "Twitter"; }
        }

        public override string SourceUri
        {
            get { return string.Format("https://api.twitter.com/1/statuses/user_timeline.xml?screen_name={0}&include_rts=1&count={1}", _username, _totalLimit); }
        }

        public TwitterFeedModule(int totalLimit, string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string username) : base(totalLimit)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _username = username;
        }

        protected override HttpWebRequest GetRequest()
        {
            // oauth implementation details
            var oauth_version = "1.0";
            var oauth_signature_method = "HMAC-SHA1";

            // unique request details
            var oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            // message api details
            var resource_url = "https://api.twitter.com/1.1/statuses/user_timeline.json";
            var screen_name = _username;
            // create oauth signature
            var baseFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&screen_name={6}";

            var baseString = string.Format(baseFormat,
                                        _consumerKey,
                                        oauth_nonce,
                                        oauth_signature_method,
                                        oauth_timestamp,
                                        _accessToken,
                                        oauth_version,
                                         Uri.EscapeDataString(screen_name)
                                        );

            baseString = string.Concat("GET&", Uri.EscapeDataString(resource_url), "&", Uri.EscapeDataString(baseString));

            var compositeKey = string.Concat(Uri.EscapeDataString(_consumerSecret),
                                    "&", Uri.EscapeDataString(_accessTokenSecret));

            string oauth_signature;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(compositeKey)))
            {
                oauth_signature = Convert.ToBase64String(
                    hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(baseString)));
            }

            // create the request header
            var headerFormat = "OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"{1}\", " +
                               "oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", " +
                               "oauth_token=\"{4}\", oauth_signature=\"{5}\", " +
                               "oauth_version=\"{6}\"";

            var authHeader = string.Format(headerFormat,
                                    Uri.EscapeDataString(oauth_nonce),
                                    Uri.EscapeDataString(oauth_signature_method),
                                    Uri.EscapeDataString(oauth_timestamp),
                                    Uri.EscapeDataString(_consumerKey),
                                    Uri.EscapeDataString(_accessToken),
                                    Uri.EscapeDataString(oauth_signature),
                                    Uri.EscapeDataString(oauth_version)
                            );


            // make the request

            ServicePointManager.Expect100Continue = false;

            var postBody = "screen_name=" + Uri.EscapeDataString(screen_name);
            resource_url += "?" + postBody;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resource_url);
            request.Headers.Add("Authorization", authHeader);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            return request;
        }

        protected override List<FeedItem> ParseDocument(Dictionary<string, object> doc)
        {
            var feedItems = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                var serializer = new JavaScriptSerializer();
                var model = serializer.ConvertToType<Model>(doc);
                for (var i = 0; i < Math.Min(_totalLimit, model.root.Count); i++)
                {
                    var modelItem = model.root[i];
                    feedItems.Add(new FeedItem()
                    {
                        SourceName = SourceName,
                        Published = GetPublished(modelItem.created_at),
                        Title = GetTitle(modelItem.source),
                        Snippet = modelItem.text,
                        ViewUri = GetViewUri(modelItem.id)
                    });
                }
            }
            
            return feedItems;
        }

        protected override string GetSnippet(string title)
        {
            // disregard all that stuff in base class's GetSnippet()
            title = Regex.Replace(title, @"^" + _username + ": ", "");
            title = Regex.Replace(title, @"https?://\w+\.\w+/\w+", "<a href=\"$0\" target=\"_blank\">$0</a>");
            title = Regex.Replace(title, @"@(\w+)", "<a href=\"http://twitter.com/$1\" target=\"_blank\">$0</a>");
            title = Regex.Replace(title, @"#(\w+)", "<a href=\"http://twitter.com/search?q=%23$1\" target=\"_blank\">$0</a>");
            return title;
        }

        private DateTime GetPublished(string pubDate)
        {
            return DateTime.ParseExact(pubDate, "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture);
        }

        private string GetTitle(string source)
        {
            if (source == "web")
            {
                source = "Web";
            }
            source = "Via " + Regex.Replace(source, "<.+?>", "");
            return source;
        }

        private string GetViewUri(long id)
        {
            return string.Format("https://twitter.com/{0}/status/{1}", _username, id);
        }
    }
}