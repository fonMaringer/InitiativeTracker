namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class BestiaryDetailsResponse
{
    public long Id { get; set; }
    public Name Name { get; set; } = null!;
    public Size Size { get; set; } = null!;
    public string ChallengeRating { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Source Source { get; set; } = null!;
    public int Experience { get; set; }
    public string ProficiencyBonus { get; set; } = null!;
    public int ArmorClass { get; set; }
    public Hits Hits { get; set; } = null!;
    public Speed[] Speed { get; set; } = [];
    public Ability Ability { get; set; } = null!;
    public string[] Images { get; set; } = [];
}

