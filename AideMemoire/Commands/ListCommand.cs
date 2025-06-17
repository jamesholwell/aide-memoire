using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Infrastructure.Repositories;

namespace AideMemoire.Commands;

public class ListCommand : IApplicationCommand {
    public void RegisterCommand(RootCommand root) {
        var listCommand = new Command("list", "List all memories by realm");
        listCommand.SetHandler(
            ExecuteAsync,
            new ConsoleBinder(),
            new RealmRepositoryBinder(),
            new MemoryRepositoryBinder());

        root.AddCommand(listCommand);
    }

    internal static async Task ExecuteAsync(
        IConsole console,
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository) {
        var realms = await realmRepository.GetAllAsync();
        var realmList = realms.ToArray();

        if (realmList.Length == 0) {
            console.WriteLine("No memory realms found.");
            return;
        }

        foreach (var realm in realmList.OrderBy(r => r.Name)) {
            var memories = await memoryRepository.GetAllForRealmAsync(realm);
            if (!memories.Any())
                continue;

            console.WriteLine($"{realm.Name} ({realm.Key})");

            foreach (var memory in memories)
                console.WriteLine($"- {memory.Title}");

            console.WriteLine(string.Empty);
        }
    }
}
