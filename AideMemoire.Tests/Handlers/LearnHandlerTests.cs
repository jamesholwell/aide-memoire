using System.CommandLine;
using System.CommandLine.IO;
using System.Net;
using System.Xml;
using AideMemoire.Handlers;
using AideMemoire.Tests.Utilities;

namespace AideMemoire.Tests;

public class LearnHandlerTests : IDisposable {
    private readonly TestHttpClientFactory _http = new();

    private readonly TestConsole _console = new();

    [Fact]
    public void RegisterCommand_ShouldAddLearnCommands() {
        // arrange
        var rootCommand = new RootCommand();

        // act
        new LearnHandler().RegisterCommand(rootCommand);

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
        await LearnHandler.ExecuteAsync(_console, _http, testUrl);

        // assert
        Assert.Contains("Amber thunderstorm warning in force after UK's hottest day", _console.Out.ToString());
    }

    [Fact]
    public async Task ReadRssFeedAsync_ExampleFeed_IsLearned() {
        // arrange
        var testRssContent = File.ReadAllText(Path.Combine("TestData/LearnHandler", "rss-test.xml"));
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, testRssContent, "application/rss+xml");

        // act
        await LearnHandler.ExecuteAsync(_console, _http, testUrl);

        // assert
        Assert.Contains("A simple test RSS feed for unit testing", _console.Out.ToString());
    }

    [Fact]
    public async Task ReadRssFeedAsync_WithNetworkError_ShouldThrowHttpRequestException() {
        // arrange
        var testUrl = "https://example.com/rss.xml";
        _http.SetupException(testUrl, new HttpRequestException("Network error"));

        // act & assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await LearnHandler.ReadRssFeedAsync(_http, testUrl));
    }

    [Fact]
    public async Task ReadRssFeedAsync_HttpClientTimeout_ShouldThrowException() {
        // arrange
        var testUrl = "https://example.com/rss.xml";
        _http.SetupException(testUrl, new TaskCanceledException("The operation was canceled."));

        // act & assert
        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await LearnHandler.ReadRssFeedAsync(_http, testUrl));
    }

    [Fact]
    public async Task ReadRssFeedAsync_WithInvalidXml_ShouldThrowXmlException() {
        // arrange
        var invalidXml = "<invalid>xml content without proper RSS structure</invalid>";
        var testUrl = "https://example.com/rss.xml";
        _http.Setup(testUrl, HttpStatusCode.OK, invalidXml, "application/rss+xml");

        // act & assert
        await Assert.ThrowsAsync<XmlException>(async () =>
            await LearnHandler.ReadRssFeedAsync(_http, testUrl));
    }

    void IDisposable.Dispose() {
        ((IDisposable)_http).Dispose();
    }
}
