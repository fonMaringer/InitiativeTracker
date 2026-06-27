using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.DataAccess.Dtos;

public record MiniatureCreateDto(
    string Name,
    CreatureSize Size,
    byte[] ImageData,
    byte[] CroppedImageData,
    int PrintedCount,
    string? Link,
    double CropX,
    double CropY,
    double CropWidth,
    double CropHeight);
