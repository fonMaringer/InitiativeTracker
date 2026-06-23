namespace InitiativeTracker.Application.Dtos;

public record SpellUpdateDto(
    string? Name,
    string? Type,
    bool? VerbalComponent,
    bool? SomaticComponent,
    string? MaterialComponent,
    string[]? Classes,
    string[]? Subclasses,
    string? Description,
    int? PrintedCount,
    string? Link);
