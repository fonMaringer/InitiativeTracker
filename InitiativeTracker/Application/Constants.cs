using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Application;

public static class Constants
{
    public const int BaseWidth = 25;
    public const int BaseHeight = 32;
    
    public static readonly Dictionary<CreatureSize, (int WidthMm, int HeightMm)> SizeDimensions = new()
    {
        [CreatureSize.Tiny]       = (BaseWidth / 2, BaseHeight / 2),
        [CreatureSize.Small]      = (BaseWidth, BaseHeight),
        [CreatureSize.Medium]     = (BaseWidth, BaseHeight),
        [CreatureSize.Large]      = (BaseWidth * 2, BaseHeight * 2),
        [CreatureSize.Huge]       = (BaseWidth * 3, BaseHeight * 3),
        [CreatureSize.Gargantuan]  = (BaseWidth * 4, BaseHeight * 4),
    };

}