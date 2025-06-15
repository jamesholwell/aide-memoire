using AideMemoire.Domain;

namespace AideMemoire.Infrastructure.Repositories;

public interface IMemoryRepository {
    Task<Memory?> GetByIdAsync(long id);

    Task<bool> ExistsAsync(Realm realm, string key);

    Task<IEnumerable<Memory>> GetAllForRealmAsync(Realm realm);

    Task<IEnumerable<Memory>> SearchAsync(string term);

    Task<IEnumerable<Memory>> SearchInRealmAsync(Realm realm, string term);

    Task<Memory> AddAsync(Memory memory);
}
