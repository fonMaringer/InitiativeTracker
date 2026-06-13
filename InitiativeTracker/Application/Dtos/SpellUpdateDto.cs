using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record SpellUpdateDto(
    string? Name,
    string? Type,
    bool? VerbalComponent,
    bool? SomaticComponent,
    bool? MaterialComponent,
    SpellClass? Class,
    string? Description);
