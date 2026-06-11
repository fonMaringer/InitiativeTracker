using Microsoft.EntityFrameworkCore;

namespace InitiativeTracker.Domain.Entities;

public class InitiativeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Initiative { get; set; }
    public int Dexterity { get; set; } = 10;
    public int HitsDefault { get; set; }
    public int HitsCurrent { get; set; }
    public int ArmorClass { get; set; }
    public int ArmorClassCurrent { get; set; }
    public string? Link { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
