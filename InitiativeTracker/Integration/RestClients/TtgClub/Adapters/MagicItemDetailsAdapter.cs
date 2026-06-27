using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class MagicItemDetailsAdapter
{
    public static MagicItem ToItemEntity(this MagicItemsDetailsResponse s,
        string link)
        => new()
        {
            Name = s.Name.Rus,
            Type = s.DetailType.Any()
                ? $"{s.Type.Name} ({string.Join(", ", s.DetailType.Select(d => d.Name))})"
                : s.Type.Name,
            Description = s.Description,
            Link = link,
            Rarity = s.Rarity.ItemRarity,
            RequiresAttunement = s.Customization,
            Source = Source.MagicItems,
        };
}