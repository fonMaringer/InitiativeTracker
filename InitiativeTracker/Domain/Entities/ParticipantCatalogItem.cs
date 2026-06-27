namespace InitiativeTracker.Domain.Entities;

public class ParticipantCatalogItem
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Hits { get; set; }
    public int ArmorClass { get; set; }
    public int Dexterity { get; set; } = 10;
}
