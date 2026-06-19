using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class MagicItemDetailsAdapter
{
    public static ItemEntity ToItemEntity(this MagicItemsDetailsResponse s,
        string link)
        => new()
        {
            Name = s.Name.Rus,
            Description = s.Description,
            Link = link,
            Rarity = s.Rarity.ItemRarity,
            RequiresAttunement = s.Customization,
            Source = Source.MagicItems,
        };
}