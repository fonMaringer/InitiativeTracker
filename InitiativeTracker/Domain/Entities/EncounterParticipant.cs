using System.Text.RegularExpressions;
using InitiativeTracker.Domain.Enums;

namespace InitiativeTracker.Domain.Entities;

public class EncounterParticipant
{
    public int Id { get; set; }
    
    public Encounter? Encounter { get; set; }
    public int EncounterId { get; set; }
    
    public int Order { get; set; }

    public int Initiative { get; set; }
    
    public int Dexterity { get; set; } = 10;
    
    public int DexModifier => Half(Dexterity - 10);

    public string Name { get; set; } = null!;

    public int HitsAverage { get; set; }
    
    public string? HitsFormula { get; set; }
    
    public int? HitsBonus { get; set; }
    
    public int HitsDefault { get; set; }

    public int HitsCurrent { get; set; }

    public int ArmorClass { get; set; }

    public int ArmorClassCurrent { get; set; }
    
    public string? Link { get; set; }
    
    public Source Source { get; set; }

    public int ChangeHpValue { get; set; }

    public HealthState State => HitsCurrent switch
    {
        var v when v > HitsDefault * 0.75 => HealthState.Healthy,
        var v when HitsDefault * 0.5 < v && v <= HitsDefault * 0.75 => HealthState.SlightlyWounded,
        var v when HitsDefault * 0.25 < v && v <= HitsDefault * 0.5 => HealthState.Wounded,
        var v when 0 < v && v <= HitsDefault * 0.25 => HealthState.SeriouslyWounded,
        _ => HealthState.Dead,
    };

    public void RollInitiative() => Initiative = new Random().Next(1, 20) + DexModifier;
    
    public void Reset()
    {
        ArmorClassCurrent = ArmorClass;
        HitsCurrent = HitsDefault;
    }

    public void Initialize(HitsMode mode)
    {
        switch (mode)
        {
            case HitsMode.Average:
                HitsDefault = HitsAverage;
                break;
            case HitsMode.Random:
                if (HitsFormula is null)
                {
                    HitsDefault = HitsAverage;
                    break;
                }

                var regex = new Regex(@"(\d+)[dк](\d{1,2})");
                var match = regex.Match(HitsFormula);
                if (match.Success)
                {
                    var diceCount = int.Parse(match.Groups[1].Value);
                    var diceType = int.Parse(match.Groups[2].Value);
                    var bonus = HitsBonus ?? 0;
                    var rand = new Random();
                    var calculatedHits = Enumerable.Range(0, diceCount).Select(_ => rand.Next(1, diceType)).Sum() + bonus;
                    HitsDefault = Math.Max(calculatedHits, 1);
                    break;
                }

                HitsDefault = HitsAverage;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        Reset();
    }

    public void AddHp(OperationMode mode) => HitsCurrent += mode switch
    {
        OperationMode.Full => ChangeHpValue,
        OperationMode.Half => Half(ChangeHpValue),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public void RemoveHp(OperationMode mode) => HitsCurrent -= mode switch
    {
        OperationMode.Full => ChangeHpValue,
        OperationMode.Half => Half(ChangeHpValue),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    private static int Half(int value) => (int)Math.Floor(value / 2.0);
}