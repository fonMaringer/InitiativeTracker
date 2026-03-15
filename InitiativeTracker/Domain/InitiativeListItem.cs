namespace InitiativeTracker.Domain;

public class InitiativeListItem
{
    public int Initiative { get; set; }
    public int Dexterity { get; set; }
    public string Name { get; set; }

    public int Hp
    {
        get;
        set
        {
            field = value;
            CurrentHp = field;
        }
    }

    public int CurrentHp { get; set; }

    public int Ac
    {
        get;
        set
        {
            field = value;
            CurrentAc = field;
        }
    }

    public int CurrentAc { get; set; }
    public string? Link { get; set; }

    public void RollInitiative() => Initiative = new Random().Next(1, 20) + (int)Math.Ceiling((Dexterity - 10) / 2.0);
    
    public void Reset()
    {
        CurrentAc = Ac;
        CurrentHp = Hp;
    }

    public bool IsDead => CurrentHp <= 0;

    public void AddHp(int count) => CurrentHp += count;
    
    public void RemoveHp(int count) => CurrentHp -= count;
}