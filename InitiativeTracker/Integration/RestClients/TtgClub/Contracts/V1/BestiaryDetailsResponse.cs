using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class Size
{
    public string Rus { get; set; }
    public string Eng { get; set; }
    public string Cell { get; set; }

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

public class Hits
{
    public int Average { get; set; }
    public string Formula { get; set; }
    public int Bonus { get; set; }
}

public class Speed
{
    public int Value { get; set; }
    public string? Name { get; set; }
}

public class Ability
{
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Wiz { get; set; }
    public int Cha { get; set; }
}

public class BestiaryDetailsResponse
{
    public long Id { get; set; }
    public Name Name { get; set; }
    public Size Size { get; set; }
    public string ChallengeRating { get; set; }
    public string Url { get; set; }
    public Source Source { get; set; }
    public int Experience { get; set; }
    public string ProficiencyBonus { get; set; }
    public int ArmorClass { get; set; }
    public Hits Hits { get; set; }
    public Speed[] Speed { get; set; }
    public Ability Ability { get; set; }
    public string[] Images { get; set; } = [];
}

