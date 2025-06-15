using AideMemoire.Domain;

namespace AideMemoire.Infrastructure.Repositories;

public interface IMemoryRepository {
    Task<Memory?> GetByIdAsync(long id);

    Task<Memory?> GetByKeyAsync(Realm realm, string key);

    Task<IEnumerable<Memory>> SearchAsync(string term);
    
    Task<Memory> AddAsync(Memory memory);
}
