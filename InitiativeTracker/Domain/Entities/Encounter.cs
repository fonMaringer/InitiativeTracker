namespace InitiativeTracker.Domain.Entities;

public class Encounter
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int CurrentRound { get; set; }
    public int? CurrentActiveParticipantId { get; set; }
}
