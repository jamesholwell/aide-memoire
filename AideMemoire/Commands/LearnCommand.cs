using System.CommandLine;
using System.CommandLine.IO;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using AideMemoire.Domain;
using AideMemoire.Infrastructure.Repositories;

namespace AideMemoire.Commands;

public class LearnCommand : IApplicationCommand {
    public void RegisterCommand(RootCommand root) {
        var learnCommand = new Command("learn", "Learn from an information source");

        // rss
        var rssCommand = new Command("rss", "Learn an RSS feed");
        learnCommand.AddCommand(rssCommand);

        var urlArgument = new Argument<string>("url", "Feed URL e.g. https://feeds.bbci.co.uk/news/rss.xml");
        rssCommand.AddArgument(urlArgument);
        rssCommand.SetHandler(
            ExecuteAsync,
            new ConsoleBinder(),
            new HttpClientFactoryBinder(),
            new RealmRepositoryBinder(),
            new MemoryRepositoryBinder(),
            urlArgument);

        root.AddCommand(learnCommand);
    }

    internal static async Task ExecuteAsync(
        IConsole console,
        IHttpClientFactory httpClientFactory,
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository,
        string url) {
        try {
            var feed = await ReadRssFeedAsync(httpClientFactory, url);
            await LearnFeedAsync(realmRepository, memoryRepository, feed);
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

    internal static async Task<SyndicationFeed> ReadRssFeedAsync(IHttpClientFactory httpClientFactory, string url) {
        using var httpClient = httpClientFactory.CreateClient();
        var feedContent = await httpClient.GetStringAsync(url);

        using var stringReader = new StringReader(feedContent);
        using var xmlReader = XmlReader.Create(stringReader);

        return SyndicationFeed.Load(xmlReader);
    }

    internal static async Task LearnFeedAsync(
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository,
        SyndicationFeed feed) {
        string realmKey =
            feed.Links.FirstOrDefault(l => l.RelationshipType == "self")?.Uri.ToString()
            ?? feed.Title.Text
            ?? throw new InvalidOperationException("Feed must have a title or self link");

        var realm =
            await realmRepository.GetByKeyAsync(realmKey)
            ?? await realmRepository.AddAsync(new Realm(realmKey, feed.Title.Text, feed.Description?.Text));                

        foreach (var item in feed.Items) {
            var memoryKey = item.Id ?? item.Links.FirstOrDefault(l => l.RelationshipType == "self")?.Uri.ToString() ?? item.Title.Text;

            if (await memoryRepository.ExistsAsync(realm, memoryKey))
                continue;

            await memoryRepository.AddAsync(new Memory(realm, memoryKey, item.Title.Text, item.Summary?.Text) {
                Uri = item.Links.FirstOrDefault()?.Uri,
                EnclosureUri = item.Links.FirstOrDefault(l => l.RelationshipType == "enclosure")?.Uri
            });
        }
    }
}
