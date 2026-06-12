using InitiativeTracker.Domain;

namespace InitiativeTracker.Application.Dtos;

public record MiniatureCreateDto(
    string Name,
    CreatureSize Size,
    byte[] ImageData,
    CropRegion? CropRegion);

public record CropRegion(double X, double Y, double Width, double Height);
