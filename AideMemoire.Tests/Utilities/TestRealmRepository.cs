using AideMemoire.Domain;
using AideMemoire.Infrastructure.Repositories;

namespace AideMemoire.Tests.Utilities;

public class TestRealmRepository : IRealmRepository {
    private readonly List<Realm> _realms = [];

    private long _nextId = 1;

    public Task<Realm?> GetByIdAsync(long id) => Task.FromResult(_realms.FirstOrDefault(r => r.Id == id));

    public Task<Realm?> GetByKeyAsync(string key) => Task.FromResult(_realms.FirstOrDefault(r => r.Key == key));

    public Task<IEnumerable<Realm>> GetAllAsync() => Task.FromResult<IEnumerable<Realm>>(_realms);

    public Task<Realm> AddAsync(Realm realm) {
        // simluate the primary key generation
        typeof(Realm).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(realm, _nextId++);

        _realms.Add(realm);
        return Task.FromResult(realm);
    }

    public IReadOnlyList<Realm> GetAllRealms() => _realms.AsReadOnly();
}
