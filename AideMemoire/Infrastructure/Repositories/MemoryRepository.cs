using AideMemoire.Domain;
using AideMemoire.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AideMemoire.Infrastructure.Repositories;

public class MemoryRepository(AideMemoireDbContext context) : IMemoryRepository {
    private readonly AideMemoireDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<Memory?> GetByIdAsync(long id) =>
        _context.Memories
            .Include(m => m.Realm)
            .SingleOrDefaultAsync(m => m.Id == id);

    public Task<bool> ExistsAsync(Realm realm, string key) =>
        _context.Memories.AnyAsync(m => EF.Property<long>(m, "realmId") == realm.Id && m.Key == key);

    public Task<IEnumerable<Memory>> GetAllForRealmAsync(Realm realm) =>
        _context.Memories
            .Include(m => m.Realm)
            .Where(m => EF.Property<long>(m, "realmId") == realm.Id)
            .OrderByDescending(m => m.LastUpdatedAt)
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<Memory>)t.Result);

    public Task<IEnumerable<Memory>> SearchAsync(string term) =>
        _context.Memories
            .Include(m => m.Realm)
            .Where(m => m.Title.Contains(term) || (m.Content != null && m.Content.Contains(term)))
            .OrderByDescending(m => m.LastUpdatedAt)
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<Memory>)t.Result);

    public Task<IEnumerable<Memory>> SearchInRealmAsync(Realm realm, string term) =>
        _context.Memories
            .Include(m => m.Realm)
            .Where(m => EF.Property<long>(m, "realmId") == realm.Id && 
                       (m.Title.Contains(term) || (m.Content != null && m.Content.Contains(term))))
            .OrderByDescending(m => m.LastUpdatedAt)
            .ToListAsync()
            .ContinueWith(t => (IEnumerable<Memory>)t.Result);

    public async Task<Memory> AddAsync(Memory memory) {
        _context.Memories.Add(memory);
        await _context.SaveChangesAsync();
        return memory;
    }
}
