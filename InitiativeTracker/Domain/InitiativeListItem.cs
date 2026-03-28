using System.Text.Json.Serialization;

namespace InitiativeTracker.Domain;

public class InitiativeListItem
{
    public int Initiative { get; set; }
    
    public int Dexterity { get; init; } = 10;
    
    [JsonIgnore]
    public int DexModifier => Half(Dexterity - 10);
    
    public string Name { get; set; }

    public int Hp { get; set; }

    public int CurrentHp { get; set; }

    public int Ac { get; set; }

    public int CurrentAc { get; set; }
    
    public string? Link { get; set; }
    
    public Source Source { get; set; }

    [JsonIgnore]
    public int ChangeHpValue { get; set; }

    [JsonIgnore]
    public HealthState State => CurrentHp switch
    {
        var v when v > Hp * 0.75 => HealthState.Healthy,
        var v when Hp * 0.5 < v && v <= Hp * 0.75 => HealthState.SlightlyWounded,
        var v when Hp * 0.25 < v && v <= Hp * 0.5 => HealthState.Wounded,
        var v when 0 < v && v <= Hp * 0.25 => HealthState.SeriouslyWounded,
        <= 0 => HealthState.Dead,
    };

    public void RollInitiative() => Initiative = new Random().Next(1, 20) + DexModifier;
    
    public void Reset()
    {
        CurrentAc = Ac;
        CurrentHp = Hp;
    }

    public void AddHp(OperationMode mode) => CurrentHp += mode switch
    {
        OperationMode.Full => ChangeHpValue,
        OperationMode.Half => Half(ChangeHpValue),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public void RemoveHp(OperationMode mode) => CurrentHp -= mode switch
    {
        OperationMode.Full => ChangeHpValue,
        OperationMode.Half => Half(ChangeHpValue),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    private static int Half(int value) => (int)Math.Floor(value / 2.0);
}

public enum OperationMode
{
    Full,
    Half,
}

public enum HealthState
{
    Healthy,
    SlightlyWounded,
    Wounded,
    SeriouslyWounded,
    Dead,
}