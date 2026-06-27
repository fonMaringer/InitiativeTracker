using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.DataAccess.Dtos;

public record MagicItemUpdateDto(
    string? Name,
    string? Type,
    ItemRarity? Rarity,
    bool? RequiresAttunement,
    string? Description,
    int? PrintedCount,
    string? Link);
