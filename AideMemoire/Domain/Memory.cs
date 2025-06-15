namespace AideMemoire.Domain;

public class Memory {
#pragma warning disable 0649
    private readonly long id;
#pragma warning restore 0649

    private readonly string key;

    private readonly DateTime createdAt;

    private DateTime lastUpdatedAt;

    public Memory(Realm realm, string key, string title, string? content) {
        this.createdAt = DateTime.UtcNow;
        this.lastUpdatedAt = this.createdAt;

        this.Realm = realm ?? throw new ArgumentNullException(nameof(realm), "Realm cannot be null");
        this.realmId = realm.Id;
        this.key = key ?? throw new ArgumentNullException(nameof(key), "Memory key cannot be null");
        this.Title = title;
        this.Content = content;
    }

    private Memory() {
        // ef core constructor for deserialization
        this.key = string.Empty;
        this.Realm = null!;
        this.Title = string.Empty;
        this.createdAt = DateTime.MinValue;
        this.lastUpdatedAt = DateTime.MinValue;
    }

    public long Id => id;

    private long realmId { get; set; }
    
    public Realm Realm { get; private set; }

    public string Key => key;

    public DateTime CreatedAt => createdAt;

    public DateTime LastUpdatedAt => lastUpdatedAt;

    public string Title { get; init; }

    public string? Content { get; init; }

    public Uri? Uri { get; init; }

    public Uri? EnclosureUri { get; init; }

    public Uri? ImageUri { get; init; }
}
