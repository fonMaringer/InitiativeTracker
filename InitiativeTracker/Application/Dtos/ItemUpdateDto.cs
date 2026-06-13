using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record ItemUpdateDto(
    string? Name,
    ItemRarity? Rarity,
    bool? RequiresAttunement,
    string? Description,
    int? PrintedCount);
