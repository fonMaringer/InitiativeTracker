using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Domain.Entities;

public class ItemEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemRarity Rarity { get; set; }
    public bool RequiresAttunement { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PrintedCount { get; set; }
    public string? Link { get; set; } = string.Empty;
}
