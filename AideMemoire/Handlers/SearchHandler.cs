using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Domain;
using AideMemoire.Infrastructure.Repositories;

namespace AideMemoire.Handlers;

public class SearchHandler : IApplicationHandler {
    public void RegisterCommand(RootCommand root) {
        var searchCommand = new Command("search", "Search memories by term");

        var termArgument = new Argument<string>("term", "Search term to look for in titles and content");
        var realmOption = new Option<string?>("--realm", "Optional realm key to search within a specific realm");
        realmOption.AddAlias("-r");

        searchCommand.AddArgument(termArgument);
        searchCommand.AddOption(realmOption);

        searchCommand.SetHandler(
            ExecuteAsync,
            new ConsoleBinder(),
            new RealmRepositoryBinder(),
            new MemoryRepositoryBinder(),
            termArgument,
            realmOption);

        root.AddCommand(searchCommand);
    }

    internal static async Task ExecuteAsync(
        IConsole console,
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository,
        string term,
        string? realm) {
        var results = await SearchMemoriesAsync(console, realmRepository, memoryRepository, term, realm);
        var resultList = results.ToArray();
        bool showDescriptions = resultList.Length < 10;

        if (resultList.Length == 0) {
            console.WriteLine($"No memories found for search term: {term}");
            return;
        }

        console.WriteLine($"Found {resultList.Length} result(s) for '{term}':");
        console.WriteLine(string.Empty);

        foreach (var memory in resultList) {
            console.WriteLine(memory.Title);
            console.WriteLine($"{memory.Realm.Name} {(memory.Uri == null ? string.Empty : $"[{memory.Uri}]")}");

            if (showDescriptions && !string.IsNullOrWhiteSpace(memory.Content))
                console.WriteLine(memory.Content.Length > 200 ? memory.Content.Substring(0, 200) + "..." : memory.Content);

            console.WriteLine(string.Empty);
        }
    }

    private static async Task<IEnumerable<Memory>> SearchMemoriesAsync(IConsole console, IRealmRepository realmRepository, IMemoryRepository memoryRepository, string term, string? realm) {
        if (string.IsNullOrWhiteSpace(realm))
            return await memoryRepository.SearchAsync(term);


        var realms = await realmRepository.GetAllAsync();
        var selectedRealm =
            realms.FirstOrDefault(r => r.Name.StartsWith(realm, StringComparison.OrdinalIgnoreCase))
            ?? realms.FirstOrDefault(r => r.Name.Contains(realm, StringComparison.OrdinalIgnoreCase));

        if (selectedRealm == null) {
            console.Error.WriteLine($"Could not find any realm matching '{realm}'");
            return [];
        }

        return await memoryRepository.SearchInRealmAsync(selectedRealm, term);
    }
}
