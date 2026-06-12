using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InitiativeTracker.Domain.Entities;

public class MiniatureEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; } = [];
    public CreatureSize Size { get; set; }
    public double CroppedRegionX { get; set; }
    public double CroppedRegionY { get; set; }
    public double CroppedRegionWidth { get; set; }
    public double CroppedRegionHeight { get; set; }
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
