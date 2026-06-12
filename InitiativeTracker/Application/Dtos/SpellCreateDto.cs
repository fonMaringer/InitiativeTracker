using InitiativeTracker.Domain;

namespace InitiativeTracker.Application.Dtos;

public record SpellCreateDto(
    string Name,
    bool VerbalComponent,
    bool SomaticComponent,
    bool MaterialComponent,
    SpellClass Class,
    string Description);
