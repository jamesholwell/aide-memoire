using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Commands;
using AideMemoire.Domain;
using AideMemoire.Domain.Events;
using AideMemoire.Tests.Utilities;

namespace AideMemoire.Tests.Commands;

public class ReindexCommandTests {
    private readonly TestRealmRepository _realmRepository = new();
    private readonly TestMemoryRepository _memoryRepository = new();
    private readonly TestConsole _console = new();
    private readonly TestMediator _mediator = new();

    [Fact]
    public void RegisterCommand_ShouldAddReindexCommand() {
        // arrange
        var rootCommand = new RootCommand();

        // act
        new ReindexCommand().RegisterCommand(rootCommand);

        // assert
        var reindexCommand = rootCommand.Children.OfType<Command>().FirstOrDefault(c => c.Name == "reindex");
        Assert.NotNull(reindexCommand);
        Assert.Equal("Reindex all memories", reindexCommand.Description);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleRealmsWithMemories_ShouldPublishEventsForAllMemories() {
        // arrange
        var realm1 = await _realmRepository.AddAsync(new Realm("realm1", "Realm 1", "First realm"));
        var realm2 = await _realmRepository.AddAsync(new Realm("realm2", "Realm 2", "Second realm"));

        var memory1 = await _memoryRepository.AddAsync(new Memory(realm1, "key1", "Memory 1", "Content 1"));
        var memory2 = await _memoryRepository.AddAsync(new Memory(realm1, "key2", "Memory 2", "Content 2"));
        var memory3 = await _memoryRepository.AddAsync(new Memory(realm2, "key3", "Memory 3", "Content 3"));
        var memory4 = await _memoryRepository.AddAsync(new Memory(realm2, "key4", "Memory 4", "Content 4"));

        // act
        await ReindexCommand.ExecuteAsync(_console, _mediator, _realmRepository, _memoryRepository);

        // assert
        var memoryUpdatedEvents = _mediator.PublishedNotifications.OfType<MemoryUpdated>().ToList();
        Assert.Equal(4, memoryUpdatedEvents.Count);

        // assert - verify events for realm1 memories
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Key == "key1" && e.Memory.Realm.Key == "realm1");
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Key == "key2" && e.Memory.Realm.Key == "realm1");

        // assert - verify events for realm2 memories
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Key == "key3" && e.Memory.Realm.Key == "realm2");
        Assert.Contains(memoryUpdatedEvents, e => e.Memory.Key == "key4" && e.Memory.Realm.Key == "realm2");
    }

    [Fact]
    public async Task ExecuteAsync_VerifyEventContainsCorrectMemoryData() {
        // arrange
        var realm = await _realmRepository.AddAsync(new Realm("test-realm", "Test Realm", "Test description"));
        var memory = await _memoryRepository.AddAsync(new Memory(realm, "test-key", "Test Title", "Test Content") {
            Uri = new Uri("https://example.com/test")
        });

        // act
        await ReindexCommand.ExecuteAsync(_console, _mediator, _realmRepository, _memoryRepository);

        // assert
        var memoryUpdatedEvent = _mediator.PublishedNotifications.OfType<MemoryUpdated>().Single();

        Assert.Equal(memory.Id, memoryUpdatedEvent.Memory.Id);
        Assert.Equal("test-key", memoryUpdatedEvent.Memory.Key);
        Assert.Equal("Test Title", memoryUpdatedEvent.Memory.Title);
        Assert.Equal("Test Content", memoryUpdatedEvent.Memory.Content);
        Assert.Equal(new Uri("https://example.com/test"), memoryUpdatedEvent.Memory.Uri);
        Assert.Equal("test-realm", memoryUpdatedEvent.Memory.Realm.Key);
    }
}
