using System;

namespace CodersBlock.SuperFeed
{
    public class FeedItem
    {
        // TODO: plan to do more with this FeedModule reference in the future
        private FeedModule Source { get; set; }
        public string SourceName {
            get { return Source.SourceName; }
        }

        public DateTime Published { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public string ImageThumbnailUri { get; set; }
        public string ImagePreviewUri { get; set; }
        public string ViewUri { get; set; }
        public int Weight { get; set; }

        public FeedItem(FeedModule source)
        {
            Source = source;
        }

        public int CompareTo(FeedItem otherFeedItem)
        {
            if(this.Weight != otherFeedItem.Weight)
            {
                return this.Weight.CompareTo(otherFeedItem.Weight);
            }
            else
            {
                return this.Published.CompareTo(otherFeedItem.Published);
            }
        }
    }
}