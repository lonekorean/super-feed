super-feed
==========

Fetches content from various social media feeds to combine into one super feed.

###Example

You can see SuperFeed in action on my website: http://codersblock.com/.

###Technical Overview

The focal point of SuperFeed is the `FeedCoordinator`, a static singleton that provides you with a merged list of `FeedItems` from your various social media feeds. It does this by coordinating multiple `FeedModules`, one for each social media site you want to include in your super feed. `FeedModules` continue running in separate threads, asynchronously loading feed data from their respective APIs, such that things happening in your main thread (for example, web page requests) are not blocked.

###Usage

To kick things off, pass instances of the `FeedModules` you want to use to `FeedCoordinator`, like this:

    FeedCoordinator.StartFeeds(
        new DribbbleFeedModule(20, "lonekorean"),
        new GitHubFeedModule(20, "lonekorean"),
        new WordPressFeedModule(20, "http://codersblock.com/blog/feed/")
    );

`FeedCoordinator` will try to fetch the `FeedItems` for each `FeedModule` immediately, blocking the current thread. This just ensures that if you then immediately ask for loaded feeds, you'll have something to show for it. The maximum time allowed for the block can be configured (or eliminated) via `FeedCoordinator.StartWaitTime`.

If you are using SuperFeed in a web app, then the `Application_Start()` method of your `Global.asax.cs` is a good place to kick things off.

Once started, you can retrieve your super feed by calling `FeedCoordinator.GetWeightedFeed()`. This takes all of your feeds, merges them together, applies a weighting algorithm, and gives you the resulting list.

###Customization

`FeedCoordinator` has several public properties that you can tweak to customize its behavior:

- `StartWaitTime` - Maximum time allotted for initial loading of feeds (in ms, default = 3000)
- `IntervalDelay` - Delay between repeating API calls to same source (in ms, default = 900000)
- `RetryAttempts` - How many times to retry a failed API call (default = 3)
- `RetryDelay` - How long to wait before retrying a failed API call (in ms, default = 500)
- `WeightedMinPerSource` - Minimum items to gaurantee from each source (default = 2)
- `WeightedStreakLimit` - Maximum consecutive items from any single source (default = 2);

Take a look at `SuperFeedConsole.Program.Main()` for sample code: https://github.com/lonekorean/super-feed/blob/master/SuperFeedConsole/Program.cs

###Supported Social Media Sites

SuperFeed currently supports 9 social media sites:

- Blogger
- CodePen
- Dribbble
- Flickr
- GitHub
- Instagram
- Last.fm
- Twitter
- WordPress

The Flickr `FeedModule` requires your Flickr ID. You can get this here: http://idgettr.com/.

The Instagram `FeedModule` requires a private token. Go to the Instagram developer site: http://instagram.com/developer/. Register your app, then go to the API console: http://instagram.com/developer/api-console/. Change the Authentication dropdown to OAuth 2, allow access, then fire off any request in the console (doesn't matter if it fails). The private token will be displayed in the Request panel.

The Twitter `FeedModule` requires all kinds of stuff. Go to the Twitter dev site: https://dev.twitter.com/. Sign into your account, create a new app. At the bottom of the details page for your new app, you should see a button to quickly generate an access token for yourself. Do that. Now the details page should provide everything you need: consumer key, consumer secret, access token, access token secret.
