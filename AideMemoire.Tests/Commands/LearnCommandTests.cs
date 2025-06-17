using System.CommandLine;
using System.CommandLine.IO;
using System.Net;
using System.Xml;
using AideMemoire.Commands;
using AideMemoire.Domain.Events;
using AideMemoire.Tests.Utilities;

namespace AideMemoire.Tests.Commands;

public class LearnCommandTests : IDisposable {
    private readonly TestHttpClientFactory _http = new();
    private readonly TestRealmRepository _realmRepository = new();
    private readonly TestMemoryRepository _memoryRepository = new();
    private readonly TestConsole _console = new();
    private readonly TestMediator _mediator = new();

    [Fact]
    public void RegisterCommand_ShouldAddLearnCommands() {
        // arrange
        var rootCommand = new RootCommand();

        // act
        new LearnCommand().RegisterCommand(rootCommand);

        // assert - learn command
        var learnCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == "learn");
        Assert.NotNull(learnCommand);

        // assert - rss subcommand
        var rssCommand = learnCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == "rss");
        Assert.NotNull(rssCommand);
        Assert.Equal("url", rssCommand.Arguments.FirstOrDefault()?.Name);
    }

    [Fact]
    public async Task ReadRssFeedAsync_SampleFeed_IsLearned() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-bbc-news.xml"));
        var testUrl = "https://feeds.bbci.co.uk/news/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);

        // assert - verify realm was created
        var realms = _realmRepository.GetAllRealms();
        Assert.Single(realms);
        var realm = realms.First();
        Assert.Equal("BBC News", realm.Name);

        // assert - verify memories were created
        var memories = _memoryRepository.GetMemoriesForRealm(realm);
        Assert.True(memories.Count > 0);
        Assert.Contains(memories, m => m.Title.Contains("Amber thunderstorm warning"));
    }

    [Fact]
    public async Task ReadRssFeedAsync_ExampleFeed_IsLearned() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-test.xml"));
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);

        // assert - verify realm was created
        var realms = _realmRepository.GetAllRealms();
        Assert.Single(realms);
        var realm = realms.First();
        Assert.Equal("Test RSS Feed", realm.Name);
        Assert.Equal("A simple test RSS feed for unit testing", realm.Description);

        // assert - verify memories were created (should be 3 articles from test RSS)
        var memories = _memoryRepository.GetMemoriesForRealm(realm);
        Assert.Equal(3, memories.Count);

        // assert - verify specific articles were learned
        Assert.Contains(memories, m => m.Title == "First Test Article" && m.Key == "https://example.com/article1");
        Assert.Contains(memories, m => m.Title == "Second Test Article" && m.Key == "https://example.com/article2");
        Assert.Contains(memories, m => m.Title == "Third Test Article" && m.Key == "https://example.com/article3");

        // assert - verify article content
        var firstArticle = memories.First(m => m.Title == "First Test Article");
        Assert.Equal("This is the first test article description", firstArticle.Content);
        Assert.Equal(new Uri("https://example.com/article1"), firstArticle.Uri);
    }

    [Fact]
    public async Task ReadRssFeedAsync_WithNetworkError_ShouldThrowHttpRequestException() {
        // arrange
        var testUrl = "https://example.com/rss.xml";
        _http.SetupException(testUrl, new HttpRequestException("Network error"));

        // act & assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await LearnCommand.ReadRssFeedAsync(_http, testUrl));
    }

    [Fact]
    public async Task ReadRssFeedAsync_HttpClientTimeout_ShouldThrowException() {
        // arrange
        var testUrl = "https://example.com/rss.xml";
        _http.SetupException(testUrl, new TaskCanceledException("The operation was canceled."));

        // act & assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await LearnCommand.ReadRssFeedAsync(_http, testUrl));
    }

    [Fact]
    public async Task ReadRssFeedAsync_WithInvalidXml_ShouldThrowXmlException() {
        // arrange
        var invalidXml = "<invalid>xml content without proper RSS structure</invalid>";
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, invalidXml, "application/rss+xml");

        // act & assert
        await Assert.ThrowsAsync<XmlException>(async () =>
            await LearnCommand.ReadRssFeedAsync(_http, testUrl));
    }

    [Fact]
    public async Task ReadRssFeedAsync_DuplicateFeed_ShouldNotCreateDuplicateMemories() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-test.xml"));
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act - process the same feed twice
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);

        // assert - should still only have one realm and 3 memories
        var realms = _realmRepository.GetAllRealms();
        Assert.Single(realms);

        var memories = _memoryRepository.GetMemoriesForRealm(realms.First());
        Assert.Equal(3, memories.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublishMemoryUpdatedEvents() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-test.xml"));
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);

        // assert - verify MemoryUpdated events were published
        var memoryUpdatedEvents = _mediator.PublishedNotifications.OfType<MemoryUpdated>().ToList();
        Assert.Equal(3, memoryUpdatedEvents.Count); // Should have 3 events for 3 articles in test RSS

        // assert - verify each event contains the correct memory
        var memories = _memoryRepository.GetMemoriesForRealm(_realmRepository.GetAllRealms().First());
        Assert.All(memoryUpdatedEvents, eventObj => {
            Assert.Contains(memories, m => m.Key == eventObj.Memory.Key);
        });

        // assert - verify specific events for known articles
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Title == "First Test Article");
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Title == "Second Test Article");
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Title == "Third Test Article");
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateFeed_ShouldNotPublishEventsForExistingMemories() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-test.xml"));
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act - process the same feed twice
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);
        await LearnCommand.ExecuteAsync(_console, _mediator, _http, _realmRepository, _memoryRepository, testUrl);

        // assert - should only have 3 MemoryUpdated events from the first run
        var memoryUpdatedEvents = _mediator.PublishedNotifications.OfType<MemoryUpdated>().ToList();
        Assert.Equal(3, memoryUpdatedEvents.Count);
    }

    void IDisposable.Dispose() {
        ((IDisposable)_http).Dispose();
    }
}
