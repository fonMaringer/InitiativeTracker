using InitiativeTracker.Domain;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class BestiaryDetailsAdapter
{
    public static InitiativeListItem ToInitiativeListItem(
        this BestiaryDetailsResponse s,
        string link,
        HitsMode mode)
    {
        var res = new InitiativeListItem
        {
            Name = s.Name.Rus,
            ArmorClass = s.ArmorClass,
            HitsAverage = s.Hits.Average,
            HitsFormula = s.Hits.Formula,
            HitsBonus = s.Hits.Bonus,
            Dexterity = s.Ability.Dex,
            Source = Source.Bestiary,
            Link = link,
        };
        res.Initialize(mode);

        return res;
    }
}