namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class SpellsDetailsResponse
{
    public Name Name { get; set; }
    public int Level { get; set; }
    public string School { get; set; }
    public SpellComponentsDetails Components { get; set; }
    public string Url { get; set; }
    public Source Source { get; set; }
    public string Range { get; set; }
    public string Duration { get; set; }
    public string Time { get; set; }
    public SpellClass[] Classes { get; set; } = [];
    public SpellClass[] Subclasses { get; set; } = [];
    public string Description { get; set; }
    public string Upper { get; set; }
    public bool Concentration { get; set; }
}