using AideMemoire.Domain.Events;
using AideMemoire.Domain.Vectors;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AideMemoire.Handlers;

public class MemoryUpdatedEmbeddingHandler : INotificationHandler<MemoryUpdated> {
    private readonly ILogger _logger;
    private readonly SqliteVectorStore _vectorStore;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

    public MemoryUpdatedEmbeddingHandler(ILoggerFactory loggerFactory, SqliteVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerationService) {
        _logger = loggerFactory.CreateLogger(nameof(MemoryUpdatedEmbeddingHandler));
        _vectorStore = vectorStore;
        _embeddingGenerator = embeddingGenerationService;
    }

    public async Task Handle(MemoryUpdated notification, CancellationToken cancellationToken) {
        if (notification.Memory.Content is null) return;

        var collection = _vectorStore.GetCollection<long, MemoryContent>(nameof(MemoryContent));
        await collection.EnsureCollectionExistsAsync(cancellationToken);

        var embedding = await _embeddingGenerator.GenerateAsync(notification.Memory.Content, cancellationToken: cancellationToken);
        _logger.LogTrace($"Embedding for {notification.Memory.Title}: {string.Join(", ", embedding.Vector.Slice(0, 5).ToArray())}...");

        var memoryContent = new MemoryContent {
            MemoryId = notification.Memory.Id,
            RealmId = notification.Memory.Realm.Id,
            Title = notification.Memory.Title,
            Content = notification.Memory.Content,
            ContentEmbedding = embedding.Vector,
        };

        await collection.UpsertAsync(memoryContent, cancellationToken);
    }
}
