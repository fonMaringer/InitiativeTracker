using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class Rarity
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Short { get; set; }

    public ItemRarity ItemRarity => Type switch
    {
        "VARIES" => ItemRarity.Varies,
        "COMMON" => ItemRarity.Common,
        "UNCOMMON" => ItemRarity.Uncommon,
        "RARE" => ItemRarity.Rare,
        "VERY_RARE" => ItemRarity.VeryRare,
        "LEGENDARY" => ItemRarity.Legendary,
        "ARTIFACT" => ItemRarity.Artifact,
        _ => ItemRarity.Unknown,
    };
}