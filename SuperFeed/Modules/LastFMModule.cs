using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules {
    public class LastFMFeedModule : FeedModuleXml {
        private string _username;
        private string _apiKey;

        public override string SourceName {
            get { return "Last.fm"; }
        }

        public override string SourceUri {
            get { return string.Format("http://ws.audioscrobbler.com/2.0/?method=user.getrecenttracks&user={0}&api_key={1}&page=1&limit={2}", _username, _apiKey, _totalLimit.ToString()); }
        }

        public LastFMFeedModule(int totalLimit,string apiKey, string username)
            : base(totalLimit) {
            _username = username;
            _apiKey = apiKey;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc) {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null) {

                items = (
                    from track in doc.Descendants("track")
                    select new FeedItem(this) {
                        Published = DateTime.Parse(track.Element("date").Value),
                        Title = track.Element("artist").Value + " - " + track.Element("name").Value,
                        Snippet = _username + " listened to " + track.Element("name").Value + " by " + track.Element("artist").Value + " from thier album " + track.Element("album").Value,
                        ImagePreviewUri = (
                            from image in track.Elements("image")
                            where image.Attribute("size").Value == "large"
                            select image.Value
                        ).Single(),
                        ImageThumbnailUri = (
                            from image in track.Elements("image")
                            where image.Attribute("size").Value == "medium"
                            select image.Value
                        ).Single(),
                        ViewUri = track.Element("url").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }

            return items;
        }
    }
}