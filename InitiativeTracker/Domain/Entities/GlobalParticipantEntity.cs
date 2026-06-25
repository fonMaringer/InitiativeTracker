namespace InitiativeTracker.Domain.Entities;

public class GlobalParticipantEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Hp { get; set; }
    public int Ac { get; set; }
    public int Dexterity { get; set; } = 10;
}
