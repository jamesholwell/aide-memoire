using AideMemoire.Domain;
using AideMemoire.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AideMemoire.Infrastructure.Repositories;

public class RealmRepository(AideMemoireDbContext context) : IRealmRepository {
    private readonly AideMemoireDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<Realm?> GetByIdAsync(long id) =>
        _context.Realms.SingleOrDefaultAsync(r => r.Id == id);

    public Task<Realm?> GetByKeyAsync(string key) =>
        _context.Realms.SingleOrDefaultAsync(r => r.Key == key);

    public Task<IEnumerable<Realm>> GetAllAsync() =>
        _context.Realms
            .OrderBy(r => r.Name)
            .ToArrayAsync()
            .ContinueWith(t => (IEnumerable<Realm>)t.Result);

    public async Task<Realm> AddAsync(Realm realm) {
        _context.Realms.Add(realm);
        await _context.SaveChangesAsync();
        return realm;
    }
}
