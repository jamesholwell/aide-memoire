using System.CommandLine;
using System.CommandLine.IO;
using AideMemoire.Domain;
using AideMemoire.Domain.Vectors;
using AideMemoire.Infrastructure.Repositories;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AideMemoire.Commands;

public class SearchCommand : IApplicationCommand {
    public void RegisterCommand(RootCommand root) {
        var searchCommand = new Command("search", "Search memories by term");

        var termArgument = new Argument<string>("term", "Search term to look for in titles and content");

        var typeOption = new Option<SearchType>(["-t", "--type"], "Search type: 'Contains' for text-based search or 'AI' for semantic search");
        typeOption.SetDefaultValue(SearchType.Default);

        var realmOption = new Option<string?>("--realm", "Optional realm key to search within a specific realm");
        realmOption.AddAlias("-r");

        searchCommand.AddArgument(termArgument);
        searchCommand.AddOption(realmOption);
        searchCommand.AddOption(typeOption);

        searchCommand.SetHandler( context => {
            var console = context.Console;

            var logger = Program.Host.Services.GetRequiredService<ILogger<SearchCommand>>();
            var realmRepository = Program.Host.Services.GetRequiredService<IRealmRepository>();
            var memoryRepository = Program.Host.Services.GetRequiredService<IMemoryRepository>();
            var vectorStore = Program.Host.Services.GetRequiredService<SqliteVectorStore>();
            var embeddingGenerator = Program.Host.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

            var type = context.ParseResult.GetValueForOption(typeOption);
            var term = context.ParseResult.GetValueForArgument(termArgument);
            var realm = context.ParseResult.GetValueForOption(realmOption);

            return ExecuteAsync(console, logger, realmRepository, memoryRepository, vectorStore, embeddingGenerator, type, term, realm);
        });

        root.AddCommand(searchCommand);
    }

    internal static async Task ExecuteAsync(
        IConsole console,
        ILogger<SearchCommand> logger,
        IRealmRepository realmRepository,
        IMemoryRepository memoryRepository,
        SqliteVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        SearchType type,
        string term,
        string? realm) {
        var results = await SearchMemoriesAsync(console, logger, realmRepository, memoryRepository, vectorStore, embeddingGenerator, type, term, realm);
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

    private static async Task<IEnumerable<Memory>> SearchMemoriesAsync(IConsole console, ILogger<SearchCommand> logger, IRealmRepository realmRepository, IMemoryRepository memoryRepository, SqliteVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, SearchType type, string term, string? realm) {
        Realm? selectedRealm = null;
        if (!string.IsNullOrWhiteSpace(realm)) {
            var realms = await realmRepository.GetAllAsync();
            selectedRealm =
                realms.FirstOrDefault(r => r.Name.StartsWith(realm, StringComparison.OrdinalIgnoreCase))
                ?? realms.FirstOrDefault(r => r.Name.Contains(realm, StringComparison.OrdinalIgnoreCase));

            if (selectedRealm == null) {
                console.Error.WriteLine($"Could not find any realm matching '{realm}'");
                return [];
            }
        }

        switch (type) {
            case SearchType.Text:
                return await TextSearchMemoriesAsync(memoryRepository, term, selectedRealm);

            case SearchType.AI:
            default:
                return await VectorSearchMemoriesAsync(logger, memoryRepository, vectorStore, embeddingGenerator, term, selectedRealm);
        }
    }

    private static async Task<IEnumerable<Memory>> TextSearchMemoriesAsync(IMemoryRepository memoryRepository, string term, Realm? realm) {
        if (realm == null)
            return await memoryRepository.SearchAsync(term);

        return await memoryRepository.SearchInRealmAsync(realm, term);
    }

    private static async Task<IEnumerable<Memory>> VectorSearchMemoriesAsync(ILogger<SearchCommand> logger, IMemoryRepository memoryRepository, SqliteVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, string term, Realm? realm) {
        // initialize the vector collection
        var collection = vectorStore.GetCollection<long, MemoryContent>(nameof(MemoryContent));
        await collection.EnsureCollectionExistsAsync();

        // generate embedding for the search term
        var searchEmbedding = await embeddingGenerator.GenerateAsync(term);

        var vectorResults = collection.SearchAsync(searchEmbedding.Vector, 3, new VectorSearchOptions<MemoryContent> {
            Filter = realm == null ? null : mc => mc.RealmId == realm.Id
        });

        var memories = new List<Memory>();
        await foreach (var record in vectorResults) {
            logger.LogInformation("Vector search [{score}]: {title}", record.Score, record.Record.Title);

            if (record.Score > 0.75F)
                continue;

            var memory = await memoryRepository.GetByIdAsync(record.Record.MemoryId);
            if (memory != null)
                memories.Add(memory);
        }

        return memories;
    }

    public enum SearchType {
        Default,

        Text,

        AI
    }
}
