using System.CommandLine;
using System.CommandLine.IO;
using System.ServiceModel.Syndication;
using System.Xml;

namespace AideMemoire.Handlers;

public class LearnHandler : IApplicationHandler {
    public void RegisterCommand(RootCommand root) {
        var learnCommand = new Command("learn", "Learn from an information source");

        // rss
        var rssCommand = new Command("rss", "Learn an RSS feed");
        learnCommand.AddCommand(rssCommand);

        var urlArgument = new Argument<string>("url", "Feed URL e.g. https://feeds.bbci.co.uk/news/rss.xml");
        rssCommand.AddArgument(urlArgument);
        rssCommand.SetHandler(
            LearnRssFeedAsync,
            new ConsoleBinder(),
            new HttpClientFactoryBinder(),
            urlArgument);

        root.AddCommand(learnCommand);
    }

    private static async Task LearnRssFeedAsync(IConsole console, IHttpClientFactory httpClientFactory, string url) {
        try {
            var feed = await ReadRssFeedAsync(httpClientFactory, url);

            console.WriteLine($"Feed Title: {feed.Title.Text}");
            console.WriteLine($"Feed Description: {feed.Description.Text}");
            console.WriteLine($"Number of items: {feed.Items.Count()}");
            console.WriteLine(string.Empty);

            foreach (var item in feed.Items) {
                console.WriteLine($"Title: {item.Title.Text}");
                console.WriteLine($"Published: {item.PublishDate:yyyy-MM-dd HH:mm}");
                if (item.Summary != null) {
                    console.WriteLine($"Summary: {item.Summary.Text}");
                }
                console.WriteLine($"Link: {item.Links.FirstOrDefault()?.Uri}");
                console.WriteLine(new string('-', 50));
            }
        }
        catch (HttpRequestException ex) {
            console.Error.WriteLine($"Error fetching RSS feed: {ex.Message}");
        }
        catch (XmlException ex) {
            console.Error.WriteLine($"Error parsing RSS feed: {ex.Message}");
        }
        catch (Exception ex) {
            console.Error.WriteLine($"Unexpected error: {ex.Message}");
        }
    }

    private static async Task<SyndicationFeed> ReadRssFeedAsync(IHttpClientFactory httpClientFactory, string url) {
        using var httpClient = httpClientFactory.CreateClient();
        var feedContent = await httpClient.GetStringAsync(url);

        using var stringReader = new StringReader(feedContent);
        using var xmlReader = XmlReader.Create(stringReader);

        return SyndicationFeed.Load(xmlReader);
    }
}
