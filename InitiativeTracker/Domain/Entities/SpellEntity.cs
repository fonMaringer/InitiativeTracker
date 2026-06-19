using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Domain.Entities;

public class SpellEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool VerbalComponent { get; set; }
    public bool SomaticComponent { get; set; }
    public string? MaterialComponent { get; set; }
    public string Range { get; set; }
    public string Duration { get; set; }
    public string Time { get; set; }
    public int Level { get; set; }
    public string[] Classes { get; set; } = [];
    public string[] Subclasses { get; set; } = [];
    public string Description { get; set; } = string.Empty;
    public string? Upper { get; set; }
    public int PrintedCount { get; set; }
    public string? Link { get; set; }
    public bool Concentration { get; set; }
    public Source Source { get; set; }
}
