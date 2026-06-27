using InitiativeTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Infrastructure.Database;

public class InitiativeTrackerDbContext(DbContextOptions<InitiativeTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<EncounterParticipant> EncounterParticipants => Set<EncounterParticipant>();
    public DbSet<ParticipantCatalogItem> ParticipantCatalog => Set<ParticipantCatalogItem>();
    public DbSet<Miniature> Miniatures => Set<Miniature>();
    public DbSet<MagicItem> MagicItems => Set<MagicItem>();
    public DbSet<Spell> Spells => Set<Spell>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Encounter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<EncounterParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasOne(e => e.Encounter)
                .WithMany()
                .HasForeignKey(e => e.EncounterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParticipantCatalogItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<MagicItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });

        modelBuilder.Entity<Miniature>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.ImageData);
        });

        modelBuilder.Entity<Spell>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
        });
    }
}
