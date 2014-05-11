super-feed
==========

Fetches content from various social media feeds to combine into one super feed.

###Example

You can see SuperFeed in action on my website: http://codersblock.com/.

###Technical Overview

The focal point of SuperFeed is the `FeedCoordinator`, a singleton object that provides you with a merged list of `FeedItems` from your various social media feeds. It does this by coordinating multiple `FeedModules`, one for each social media site you want to include in your super feed. `FeedModules` run in a separate thread, asynchronously loading feed data from their respective APIs, such that things happening in your main thread (for example, web page requests) are not blocked.

SuperFeed also takes care of various details for you, such as staggered API requests, fault tolerance, and text massaging.

###Usage

Feeds are added and kicked off by passing a `FeedModule` instance into `FeedCoordinator.StartFeedModule()`, like this:

    FeedCoordinator.StartFeedModule(new BloggerFeedModule(20, "codersblock"));

If you are using SuperFeed in a web app, then the `Application_Start()` method of your `Global.asax.cs` is a good place for this.

After that, you can retrieve your super feed by calling `GetTopFeed()`. This takes all of your feeds, merges them together, thins things out by applying a weighting algorithm, and gives you the resulting list.

`FeedCoordinator` has several public properties that you can tweak to customize its behavior:

- StaggerDelay: stagger time between initial API calls to sources (in ms, default is 10000)
- IntervalDelay: pause between repeating API calls to same source (in ms, default is 900000)
- MaxPerSource: max items returned from GetFeed() (default is 100)
- MaxAllSources: max items returned from GetTopFeed() (default is 20)
- PromoteRepresentation: helps GetTopFeed() include items from all sources (default is 2)
- PromoteStreakLimit: helps GetTopFeed() limit consecutive items from the same source (default is 2);
- PromoteRecent: helps GetTopFeed() include the freshest items (in days, default is 365)

Take a look at `SuperFeedConsole.Program.Main()` for sample code: https://github.com/lonekorean/super-feed/blob/master/SuperFeedConsole/Program.cs

###Supported Social Media Sites

SuperFeed currently supports 8 social media sites:

- Blogger
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
