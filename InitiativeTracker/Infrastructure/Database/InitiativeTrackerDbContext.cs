using InitiativeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Infrastructure.Database;

public class InitiativeTrackerDbContext(DbContextOptions<InitiativeTrackerDbContext> options) : DbContext(options)
{
    public DbSet<InitiativeEntity> Initiatives => Set<InitiativeEntity>();
    public DbSet<MiniatureEntity> Miniatures => Set<MiniatureEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<SpellEntity> Spells => Set<SpellEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InitiativeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.SourceId).IsRequired();
        });

        modelBuilder.ApplyConfiguration(new MiniatureEntityConfiguration());

        modelBuilder.Entity<ItemEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });

        modelBuilder.Entity<SpellEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });
    }
}
