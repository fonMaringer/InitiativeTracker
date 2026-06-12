using InitiativeTracker.Domain;

namespace InitiativeTracker.Application.Dtos;

public record MiniatureUpdateDto(
    string? Name,
    CreatureSize? Size,
    CropRegion? CropRegion);
