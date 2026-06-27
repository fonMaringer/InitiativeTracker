using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Domain.Entities;

public class Miniature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? ImageData { get; set; } = [];
    public byte[]? CroppedImageData { get; set; } = [];
    public CreatureSize Size { get; set; }
    public int PrintedCount { get; set; }
    public string? Link { get; set; }

    public double CropX { get; set; }
    public double CropY { get; set; }
    public double CropWidth { get; set; }
    public double CropHeight { get; set; }

    public double NaturalWidth { get; set; }
    public double NaturalHeight { get; set; }
}
