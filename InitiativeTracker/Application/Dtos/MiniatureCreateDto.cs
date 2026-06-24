using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application.Dtos;

public record MiniatureCreateDto(
    string Name,
    CreatureSize Size,
    byte[] ImageData,
    int PrintedCount,
    string? Link,
    double CropX,
    double CropY,
    double CropWidth,
    double CropHeight,
    double NaturalWidth,
    double NaturalHeight);
