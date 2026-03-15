using InitiativeTracker.Domain;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class BestiaryDetailsAdapter
{
    public static InitiativeListItem ToInitiativeListItem(this BestiaryDetailsResponse s, string link)
    {
        var res = new InitiativeListItem
        {
            Name = s.Name.Rus,
            Ac = s.ArmorClass,
            Hp = s.Hits.Average,
            Dexterity = s.Ability.Dex,
            Source = Source.Bestiary,
            Link = link,
        };
        res.Reset();

        return res;
    }
}