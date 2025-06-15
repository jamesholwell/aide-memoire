namespace AideMemoire.Domain;

public class Realm {
#pragma warning disable 0649
    private readonly long id;
#pragma warning restore 0649

    private readonly string key;

    private readonly DateTime createdAt;

    private DateTime lastUpdatedAt;

    public Realm(string key, string name, string? description) {
        createdAt = DateTime.UtcNow;
        lastUpdatedAt = createdAt;

        this.key = key ?? throw new ArgumentNullException(nameof(key), "Realm key cannot be null");
        Name = name ?? throw new ArgumentNullException(nameof(name), "Realm name cannot be null");
        Description = description;
    }

    public long Id => id;

    public string Key => key;

    public DateTime CreatedAt => createdAt;

    public DateTime LastUpdatedAt => lastUpdatedAt;

    public string Name { get; init; }

    public string? Description { get; init; }
}
