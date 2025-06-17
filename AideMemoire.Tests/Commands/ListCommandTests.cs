using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Domain;
using AideMemoire.Commands;
using AideMemoire.Tests.Utilities;

namespace AideMemoire.Tests.Commands;

public class ListCommandTests {
    private readonly TestRealmRepository _realmRepository = new();

    private readonly TestMemoryRepository _memoryRepository = new();

    private readonly TestConsole _console = new();

    [Fact]
    public void RegisterCommand_ShouldAddListCommand() {
        // arrange
        var rootCommand = new RootCommand();

        // act
        new ListCommand().RegisterCommand(rootCommand);

        // assert
        var listCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == "list");
        Assert.NotNull(listCommand);
        Assert.Equal("List all memories by realm", listCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_NoRealms_ShouldDisplayNoRealmsMessage() {
        // act
        await ListCommand.ExecuteAsync(_console, _realmRepository, _memoryRepository);

        // assert
        Assert.Contains("No memory realms found.", _console.Out.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_SingleRealmWithMemories_ShouldDisplayRealmAndMemories() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("news-feed", "BBC News", "BBC News RSS Feed"));
        await _memoryRepository.AddAsync(new Memory(realm, "article1", "First Article", "First article content"));
        await _memoryRepository.AddAsync(new Memory(realm, "article2", "Second Article", "Second article content"));

        // act
        await ListCommand.ExecuteAsync(_console, _realmRepository, _memoryRepository);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("BBC News (news-feed)", output);
        Assert.Contains("- First Article", output);
        Assert.Contains("- Second Article", output);
    }

    [Fact]
    public async Task ExecuteAsync_MixedRealmsWithAndWithoutMemories_OnlyShowsRealmsWithMemories() {
        // arrange
        var emptyRealm = await _realmRepository.AddAsync(new Realm("empty-feed", "Empty Realm", "No memories here"));
        var fullRealm = await _realmRepository.AddAsync(new Realm("full-feed", "Full Realm", "Has memories"));
        
        await _memoryRepository.AddAsync(new Memory(fullRealm, "memory1", "Important Memory", "Important content"));

        // act
        await ListCommand.ExecuteAsync(_console, _realmRepository, _memoryRepository);

        // assert
        var output = _console.Out.ToString();
        Assert.Contains("Full Realm (full-feed)", output);
        Assert.Contains("- Important Memory", output);
        Assert.DoesNotContain("Empty Realm", output);
        Assert.DoesNotContain("empty-feed", output);
    }
}
