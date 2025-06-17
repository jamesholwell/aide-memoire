using System.CommandLine;
using AideMemoire.Binding;
using AideMemoire.Domain.Events;
using AideMemoire.Infrastructure.Repositories;
using MediatR;

namespace AideMemoire.Commands;

public class ReindexCommand : IApplicationCommand {
    public void RegisterCommand(RootCommand root) {
        var reindexCommand = new Command("reindex", "Reindex all memories");
        reindexCommand.SetHandler(
            ExecuteAsync,
            new ConsoleBinder(),
            new MediatorBinder(),
            new RealmRepositoryBinder(),
            new MemoryRepositoryBinder());

        root.AddCommand(reindexCommand);
    }

    internal static async Task ExecuteAsync(
        IConsole console,
        IMediator mediator,
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository) {
        var realms = await realmRepository.GetAllAsync();

        foreach (var realm in realms) {
            var memories = await memoryRepository.GetAllForRealmAsync(realm);

            foreach (var memory in memories)
                await mediator.Publish(new MemoryUpdated(memory));
        }
    }
}
