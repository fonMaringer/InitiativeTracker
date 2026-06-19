using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record SpellCreateDto(
    string Name,
    string Type,
    bool VerbalComponent,
    bool SomaticComponent,
    string? MaterialComponent,
    string Class,
    string Description,
    int PrintedCount,
    string? Link,
    string Range,
    string Duration,
    string Time,
    int Level,
    string? Upper,
    bool Concentration,
    Source Source
);
