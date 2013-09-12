using System;

namespace CodersBlock.SuperFeed
{
    [Serializable]
    public class FeedItem : IComparable<FeedItem>
    {
        public string SourceName { get; set; }
        public DateTime Published { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public string ImagePreviewUri { get; set; }
        public string ViewUri { get; set; }
        public int Weight { get; set; }

        public FeedItem(FeedModule source)
        {
            SourceName = source.SourceName;
        }

        private FeedItem()
        {
            // for cloning
        }

        public FeedItem Clone()
        {
            var clone = new FeedItem()
            {
                SourceName = this.SourceName,
                Published = this.Published,
                Title = this.Title,
                Snippet = this.Snippet,
                ImagePreviewUri = this.ImagePreviewUri,
                ViewUri = this.ViewUri,
                Weight = this.Weight
            };
            return clone;
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