super-feed
==========

Fetches content from various social media feeds to combine into one super feed.

###Technical Overview

The focal point of SuperFeed is the `FeedCoordinator`, a singleton object that provides you with a merged list of `FeedItem`s from your various social media feeds. It does this by coordinating multiple `FeedModule`s, one for each social media site you want to include in your super feed. `FeedModule`s run in a separate thread, asynchronously loading feed data from their respective APIs, such that things happening in your main thread (for example, web page requests) are not blocked.

SuperFeed also takes care of various details for you, such as staggered API requests, fault tolerance, and text massaging.

###Usage

Feeds are added and kicked off by passing a `FeedModule` instance into `FeedCoordinator.StartFeedModule()`, like this:

    FeedCoordinator.StartFeedModule(new TwitterFeedModule(80, "lonekorean"));

After that, you can retrieve your super feed by calling either `GetMergedFeed()` or `GetMergedAndBalancedFeed()`. The former gives you all your feeds merged together. The latter takes this one step further by "balancing" your super feed to curb the number of consecutive 'FeedItem's from any single social media site. This is useful if, for example, you have many more tweets than anything else, but don't want your super feed to be dominated by tweets.

Take a look at 'SuperFeedConsole.Program.Main()' for an implementation example: https://github.com/lonekorean/super-feed/blob/master/SuperFeedConsole/Program.cs

###Supported Social Media Feeds

SuperFeed currently supports five social media sites:

- Twitter
- Blogger
- Flickr
- DeviantArt
- Instagram

