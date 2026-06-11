namespace InitiativeTracker.Domain.Entities;

public class MiniatureEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public CreatureSize Size { get; set; }
    public double CroppedRegionX { get; set; }
    public double CroppedRegionY { get; set; }
    public double CroppedRegionWidth { get; set; }
    public double CroppedRegionHeight { get; set; }
}
