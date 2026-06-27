using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class Size
{
    public string Rus { get; set; } = null!;
    public string Eng { get; set; } = null!;
    public string Cell { get; set; } = null!;

    public CreatureSize CreatureSize => Eng.ToUpper() switch
    {
        "TINY" => CreatureSize.Tiny,
        "SMALL" => CreatureSize.Small,
        "MEDIUM" => CreatureSize.Medium,
        "LARGE" => CreatureSize.Large,
        "HUGE" => CreatureSize.Huge,
        "GARGANTUAN" => CreatureSize.Gargantuan,
        _ => CreatureSize.Unknown,
    };
}