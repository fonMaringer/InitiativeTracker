using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record ItemCreateDto(
    string Name,
    string? Type,
    ItemRarity Rarity,
    bool RequiresAttunement,
    string Description,
    int PrintedCount,
    string? Link,
    Source Source);
