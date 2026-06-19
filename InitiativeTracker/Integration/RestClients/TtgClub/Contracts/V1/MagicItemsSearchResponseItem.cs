namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class MagicItemsSearchResponseItem
{
    public Name Name { get; set; }
    public string Url { get; set; }
    public Source Source { get; set; }
    public Rarity Rarity { get; set; }
}