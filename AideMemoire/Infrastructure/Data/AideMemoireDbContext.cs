using AideMemoire.Domain;
using Microsoft.EntityFrameworkCore;

namespace AideMemoire.Infrastructure.Data;

public class AideMemoireDbContext : DbContext {
    public AideMemoireDbContext(DbContextOptions<AideMemoireDbContext> options) : base(options) { }

    public DbSet<Realm> Realms { get; set; }

    public DbSet<Memory> Memories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // realm configuration
        modelBuilder.Entity<Realm>(entity => {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id)
                .HasField("id")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .ValueGeneratedOnAdd();

            entity.Property(r => r.Key)
                .HasField("key")
                .HasMaxLength(255)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.CreatedAt)
                .HasField("createdAt")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.LastUpdatedAt)
                .HasField("lastUpdatedAt")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(255);
        });

        // memory configuration
        modelBuilder.Entity<Memory>(entity => {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Realm)
                .WithMany()
                .HasForeignKey("realmId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(r => r.Id)
                .HasField("id")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .ValueGeneratedOnAdd();

            entity.Property(r => r.Key)
                .HasField("key")
                .HasMaxLength(255)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.CreatedAt)
                .HasField("createdAt")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.LastUpdatedAt)
                .HasField("lastUpdatedAt")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(m => m.Uri)
                .HasConversion(
                    uri => uri != null ? uri.ToString() : null,
                    str => str != null ? new Uri(str) : null);

            entity.Property(m => m.EnclosureUri)
                .HasConversion(
                    uri => uri != null ? uri.ToString() : null,
                    str => str != null ? new Uri(str) : null);

            entity.Property(m => m.ImageUri)
                .HasConversion(
                    uri => uri != null ? uri.ToString() : null,
                    str => str != null ? new Uri(str) : null);
        });

        base.OnModelCreating(modelBuilder);
    }
}
