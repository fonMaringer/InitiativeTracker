using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.DataAccess.Dtos;

public record MiniatureCatalogDto(
    int Id,
    string Name,
    CreatureSize Size,
    byte[] CroppedImageData,
    int PrintedCount,
    string? Link);
