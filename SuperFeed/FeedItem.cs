using System;

namespace CodersBlock.SuperFeed
{
    public class FeedItem
    {
        public string SourceName { get; set; }
        public DateTime Published { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public string ImageThumbnailUri { get; set; }
        public string ImagePreviewUri { get; set; }
        public string ViewUri { get; set; }
    }
}