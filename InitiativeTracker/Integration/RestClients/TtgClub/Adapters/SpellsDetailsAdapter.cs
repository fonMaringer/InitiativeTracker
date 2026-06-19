using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Adapters;

public static class SpellsDetailsAdapter
{
    public static SpellEntity ToSpellEntity(this SpellsDetailsResponse s,
        string link) =>
        new()
        {
            Name = s.Name.Rus,
            Description = s.Description,
            Link = link,
            Range = s.Range,
            Duration = s.Duration,
            Time = s.Time,
            Level = s.Level,
            Upper = s.Upper,
            SomaticComponent = s.Components.S,
            VerbalComponent = s.Components.V,
            MaterialComponent = s.Components.M,
            Type = s.School,
            Concentration = s.Concentration,
            Classes = s.Classes.Select(c => c.Name).ToArray(),
            Subclasses = s.Subclasses.Select(c => c.Name).ToArray(),
            Source = Source.Spells,
        };
}