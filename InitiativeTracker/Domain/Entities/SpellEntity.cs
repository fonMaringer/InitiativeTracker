using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Domain.Entities;

public class SpellEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool VerbalComponent { get; set; }
    public bool SomaticComponent { get; set; }
    public bool MaterialComponent { get; set; }
    public SpellClass Class { get; set; }
    public string Description { get; set; } = string.Empty;
}
