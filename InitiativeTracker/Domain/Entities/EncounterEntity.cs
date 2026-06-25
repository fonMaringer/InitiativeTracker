namespace InitiativeTracker.Domain.Entities;

public class EncounterEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
