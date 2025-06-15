using AideMemoire.Domain;

namespace AideMemoire.Infrastructure.Repositories;

public interface IRealmRepository {
    Task<Realm?> GetByIdAsync(long id);
    
    Task<Realm?> GetByKeyAsync(string key);

    Task<IEnumerable<Realm>> GetAllAsync();

    Task<Realm> AddAsync(Realm realm);
}
