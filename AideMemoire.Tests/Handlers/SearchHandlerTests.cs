using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Domain;
using AideMemoire.Handlers;
using AideMemoire.Tests.Utilities;

namespace AideMemoire.Tests.Handlers;

public class SearchHandlerTests {
    private readonly TestRealmRepository _realmRepository = new();

    private readonly TestMemoryRepository _memoryRepository = new();

    private readonly TestConsole _console = new();

    [Fact]
    public void RegisterCommand_ShouldAddSearchCommand() {
        // arrange
        var rootCommand = new RootCommand();

        // act
        new SearchHandler().RegisterCommand(rootCommand);

        // assert
        var searchCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == "search");
        Assert.NotNull(searchCommand);
        Assert.Equal("Search memories by term", searchCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_NoResults_ShouldDisplayNoResultsMessage() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news-feed", "BBC News", "BBC News RSS Feed"));
        await _memoryRepository.AddAsync(new Memory(realm, "article1", "First Article", "First article content"));

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "nonexistent", null);

        // assert
        Assert.Contains("No memories found for search term: nonexistent", _console.Out.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_SearchByTitle_ShouldDisplayResults() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news-feed", "BBC News", "BBC News RSS Feed"));
        var memory = await _memoryRepository.AddAsync(new Memory(realm, "article1", "Test Article", "Some content") { 
            Uri = new Uri("https://example.com/article") 
        });

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "Test", null);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("Found 1 result(s) for 'Test'", output);
        Assert.Contains("Test Article", output);
        Assert.Contains("https://example.com/article", output);
        Assert.Contains("Some content", output);
        Assert.Contains("BBC News", output);
    }

    [Fact]
    public async Task ExecuteAsync_SearchByContent_ShouldDisplayResults() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news-feed", "BBC News", "BBC News RSS Feed"));
        await _memoryRepository.AddAsync(new Memory(realm, "article1", "Article Title", "This contains special keyword"));

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "keyword", null);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("Found 1 result(s) for 'keyword'", output);
        Assert.Contains("Article Title", output);
    }

    [Fact]
    public async Task ExecuteAsync_SearchInSpecificRealm_ShouldFilterByRealm() {
        // arrange
        var realm1 = await _realmRepository.AddAsync(new Realm("news", "News", "News Feed"));
        var realm2 = await _realmRepository.AddAsync(new Realm("tech", "Tech", "Tech Feed"));
        
        await _memoryRepository.AddAsync(new Memory(realm1, "news1", "News Article", "News content"));
        await _memoryRepository.AddAsync(new Memory(realm2, "tech1", "Tech Article", "Tech content"));

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "Article", "tech");

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("Found 1 result(s) for 'Article'", output);
        Assert.Contains("Tech Article", output);
        Assert.DoesNotContain("News Article", output);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidRealm_ShouldDisplayErrorMessage() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news", "News", "News Feed"));
        await _memoryRepository.AddAsync(new Memory(realm, "article1", "Test Article", "Content"));

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "Test", "invalid-realm");

        // assert
        Assert.Contains("Could not find any realm matching 'invalid-realm'", _console.Error.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_LongDescription_ShouldTruncateAt200Characters() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news", "News", "News Feed"));
        var longContent = new string('a', 250); // 250 characters
        await _memoryRepository.AddAsync(new Memory(realm, "article1", "Test Article", longContent));

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "Test", null);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains($"{new string('a', 200)}...", output);
    }

    [Fact]
    public async Task ExecuteAsync_ManyResults_ShouldShowOnlyTitles() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news", "News", "News Feed"));
        
        // Add more than 10 memories to trigger title-only mode
        for (int i = 1; i <= 12; i++) {
            await _memoryRepository.AddAsync(new Memory(realm, $"article{i}", $"Test Article {i}", $"Simulated Long Content {i}"));
        }

        // act
        await SearchHandler.ExecuteAsync(_console, _realmRepository, _memoryRepository, "Test", null);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("Found 12 result(s) for 'Test'", output);
        Assert.DoesNotContain("Simulated Long Content", output);
    }
}
