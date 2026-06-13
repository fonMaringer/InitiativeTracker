using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record SpellCreateDto(
    string Name,
    string Type,
    bool VerbalComponent,
    bool SomaticComponent,
    bool MaterialComponent,
    SpellClass Class,
    string Description,
    int PrintedCount,
    string? Link);
