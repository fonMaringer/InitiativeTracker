using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class BestiaryDetailsAdapter
{
    public static EncounterParticipant ToEncounterParticipant(
        this BestiaryDetailsResponse s,
        string link,
        HitsMode mode)
    {
        var res = new EncounterParticipant
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

    public static Miniature ToMiniatureEntity(
        this BestiaryDetailsResponse s,
        string link,
        byte[]? imageData)
        => new()
        {
            Name = s.Name.Rus,
            Link = link,
            ImageData = imageData,
            Size = s.Size.CreatureSize,
        };
}