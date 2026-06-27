namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class MagicItemsSearchResponseItem
{
    public Name Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public Source Source { get; set; } = null!;
    public Rarity Rarity { get; set; } = null!;
}