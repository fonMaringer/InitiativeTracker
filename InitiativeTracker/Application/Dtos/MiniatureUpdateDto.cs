using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record MiniatureUpdateDto(
    string? Name,
    CreatureSize? Size,
    CropRegion? CropRegion);
