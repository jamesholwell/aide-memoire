using Microsoft.Extensions.VectorData;

namespace AideMemoire.Domain.Vectors;

public class MemoryContent {
    [VectorStoreKey]
    public long MemoryId { get; set; }

    [VectorStoreData]
    public long RealmId { get; set; }

    [VectorStoreData]
    public string? Title { get; set; }

    [VectorStoreData]
    public string? Content { get; set; }

    [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float>? ContentEmbedding { get; set; }
}
