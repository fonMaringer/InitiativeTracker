namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class SpellsSearchResponseItem
{
    public Name Name { get; set; }
    public int Level { get; set; }
    public string School { get; set; }
    public SpellComponentsSearch Components { get; set; }
    public string Url { get; set; }
    public Source Source { get; set; }
}