namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class MagicItemsDetailsResponse
{
    public Name Name { get; set; } = null!;
    public MagicItemType Type { get; set; } = null!;
    public Source Source { get; set; } = null!;
    public Rarity Rarity { get; set; } = null!;
    public bool Customization { get; set; }
    public string Description { get; set; } = null!;

    public DetailType[] DetailType { get; set; } = [];

    /// <summary>
    /// List of image urls
    /// </summary>
    // public string[] Images { get; set; }
}