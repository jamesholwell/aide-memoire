using AideMemoire.Domain;
using AideMemoire.Infrastructure.Repositories;

namespace AideMemoire.Tests.Utilities;

public class TestMemoryRepository : IMemoryRepository {
    private readonly List<Memory> _memories = [];

    private long _nextId = 1;

    public Task<Memory?> GetByIdAsync(long id) => Task.FromResult(_memories.FirstOrDefault(m => m.Id == id));

    public Task<bool> ExistsAsync(Realm realm, string key) => Task.FromResult(_memories.Any(m => m.Realm.Id == realm.Id && m.Key == key));

    public Task<IEnumerable<Memory>> GetAllForRealmAsync(Realm realm) => Task.FromResult<IEnumerable<Memory>>(_memories.Where(m => m.Realm.Id == realm.Id).ToArray());

    public Task<IEnumerable<Memory>> SearchAsync(string term) {
        var results = _memories
            .Where(m => (m.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || (m.Content?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToArray();

        return Task.FromResult<IEnumerable<Memory>>(results);
    }

    public Task<IEnumerable<Memory>> SearchInRealmAsync(Realm realm, string term) {
        var results = _memories
            .Where(m => m.Realm.Id == realm.Id && 
                       ((m.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) || 
                        (m.Content?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
            .ToArray();

        return Task.FromResult<IEnumerable<Memory>>(results);
    }

    public Task<Memory> AddAsync(Memory memory) {
        // simulate the primary key generation
        typeof(Memory).GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(memory, _nextId++);

        _memories.Add(memory);
        return Task.FromResult(memory);
    }

    public IReadOnlyList<Memory> GetAllMemories() => _memories.AsReadOnly();

    public IReadOnlyList<Memory> GetMemoriesForRealm(Realm realm) => _memories.Where(m => m.Realm.Id == realm.Id).ToList().AsReadOnly();
}
