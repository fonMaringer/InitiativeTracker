using System.Text.Json.Serialization;

namespace InitiativeTracker.Domain;

public class InitiativeListItem
{
    public int Initiative { get; set; }
    
    public int Dexterity { get; init; } = 10;
    
    [JsonIgnore]
    public int DexModifier => (int)Math.Ceiling((Dexterity - 10) / 2.0);
    
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
    public bool IsDead => CurrentHp <= 0;

    public void RollInitiative() => Initiative = new Random().Next(1, 20) + DexModifier;
    
    public void Reset()
    {
        CurrentAc = Ac;
        CurrentHp = Hp;
    }

    public void AddHp() => CurrentHp += ChangeHpValue;
    
    public void RemoveHp() => CurrentHp -= ChangeHpValue;
}