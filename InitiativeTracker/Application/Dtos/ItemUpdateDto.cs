using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record ItemUpdateDto(
    string? Name,
    string? Type,
    ItemRarity? Rarity,
    bool? RequiresAttunement,
    string? Description,
    int? PrintedCount,
    string? Link);
