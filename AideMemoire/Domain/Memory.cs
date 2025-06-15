namespace AideMemoire.Domain;

public class Memory {
#pragma warning disable 0649
    private readonly long id;
#pragma warning restore 0649

    private readonly Realm realm;

    private readonly string key;

    private readonly DateTime createdAt;

    private DateTime lastUpdatedAt;

    public Memory(Realm realm, string key, string title, string? content) {
        this.createdAt = DateTime.UtcNow;
        this.lastUpdatedAt = this.createdAt;

        this.realm = realm ?? throw new ArgumentNullException(nameof(realm), "Realm cannot be null");
        this.key = key ?? throw new ArgumentNullException(nameof(key), "Memory key cannot be null");
        this.Title = title;
        this.Content = content;
    }

    private Memory(string key, string title) {
        // ef core constructor for deserialization
        this.key = key;
        this.Title = title;
        this.realm = null!; // realm will be set later
        this.createdAt = DateTime.MinValue;
        this.lastUpdatedAt = DateTime.MinValue;
    }

    public long Id => id;

    public Realm Realm => realm;

    public string Key => key;

    public DateTime CreatedAt => createdAt;

    public DateTime LastUpdatedAt => lastUpdatedAt;

    public string Title { get; init; }

    public string? Content { get; init; }

    public Uri? Uri { get; init; }

    public Uri? EnclosureUri { get; init; }

    public Uri? ImageUri { get; init; }
}
