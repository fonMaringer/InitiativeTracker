namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class SpellsSearchResponseItem
{
    public Name Name { get; set; } = null!;
    public int Level { get; set; }
    public string School { get; set; } = null!;
    public SpellComponentsSearch Components { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Source Source { get; set; } = null!;
}