namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class MagicItemsDetailsResponse
{
    public Name Name { get; set; }
    public MagicItemType Type { get; set; }
    public Source Source { get; set; }
    public Rarity Rarity { get; set; }
    public bool Customization { get; set; }
    public string Description { get; set; }
    /// <summary>
    /// List of image urls
    /// </summary>
    // public string[] Images { get; set; }
}