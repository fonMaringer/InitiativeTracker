using InitiativeTracker.Domain;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class BestiaryDetailsAdapter
{
    public static InitiativeListItem ToInitiativeListItem(this BestiaryDetailsResponse s)
        => new()
        {
            Name = s.Name.Rus,
            Ac = s.ArmorClass,
            Hp = s.Hits.Average,
            Dexterity = s.Ability.Dex,
        };
}