using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace CodersBlock.SuperFeed.Modules
{
    public class LastFMFeedModule : FeedModuleXml
    {
        private string _username;
        private string _apiKey;

        public override string SourceName
        {
            get { return "Last.fm"; }
        }

        public override string SourceUri
        {
            get { return string.Format("http://ws.audioscrobbler.com/2.0/?method=user.getrecenttracks&user={0}&api_key={1}&page=1&limit={2}", _username, _apiKey, _totalLimit.ToString()); }
        }

        public LastFMFeedModule(int totalLimit, string apiKey, string username) : base(totalLimit)
        {
            _username = username;
            _apiKey = apiKey;
        }

        protected override List<FeedItem> ParseDocument(XDocument doc)
        {
            var items = new List<FeedItem>(_totalLimit);
            if (doc != null)
            {
                items = (
                    from track in doc.Descendants("track")
                    where track.Attribute("nowplaying") == null
                    select new FeedItem(this)
                    {
                        Published = GetPublished(track.Element("date").Value),
                        Title = GetTitle(track.Element("name").Value),
                        Snippet = GetSnippet(track.Element("artist").Value, track.Element("album").Value),
                        ImagePreviewUri = (
                            from image in track.Elements("image")
                            where image.Attribute("size").Value == "extralarge"
                            select image.Value
                        ).Single(),
                        ViewUri = track.Element("url").Value
                    }
                ).Take(_totalLimit).ToList<FeedItem>();
            }

            return items;
        }

        private DateTime GetPublished(string date)
        {
            var parsed = DateTime.ParseExact(date, @"d MMM yyyy, HH\:mm", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return parsed;
        }

        private string GetTitle(string name)
        {
            return "Played: " + name;
        }

        private string GetSnippet(string artist, string album)
        {
            return "By " + artist + " from the album " + album + ".";
        }
    }
}