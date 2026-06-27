namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class SpellsDetailsResponse
{
    public Name Name { get; set; } = null!;
    public int Level { get; set; }
    public string School { get; set; } = null!;
    public SpellComponentsDetails Components { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Source Source { get; set; } = null!;
    public string Range { get; set; } = null!;
    public string Duration { get; set; } = null!;
    public string Time { get; set; } = null!;
    public SpellClass[] Classes { get; set; } = [];
    public SpellClass[] Subclasses { get; set; } = [];
    public string Description { get; set; } = null!;
    public string Upper { get; set; } = null!;
    public bool Concentration { get; set; }
}