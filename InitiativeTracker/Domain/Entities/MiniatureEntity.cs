using InitiativeTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InitiativeTracker.Domain.Entities;

public class MiniatureEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; } = [];
    public CreatureSize Size { get; set; }
    public int PrintedCount { get; set; }
    public string? Link { get; set; }

    public double CropX { get; set; }
    public double CropY { get; set; }
    public double CropWidth { get; set; }
    public double CropHeight { get; set; }

    public double NaturalWidth { get; set; }
    public double NaturalHeight { get; set; }
}

public class MiniatureEntityConfiguration : IEntityTypeConfiguration<MiniatureEntity>
{
    public void Configure(EntityTypeBuilder<MiniatureEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.ImageData);
    }
}
