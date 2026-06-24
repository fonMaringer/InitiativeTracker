using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record MiniatureUpdateDto(
    string? Name,
    CreatureSize? Size,
    int? PrintedCount,
    string? Link,
    double CropX,
    double CropY,
    double CropWidth,
    double CropHeight,
    double NaturalWidth,
    double NaturalHeight);

