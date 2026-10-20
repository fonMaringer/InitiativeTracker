using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace InitiativeTracker.Components.Pages.Encounters;

public partial class InitiativeTracker(
    IEncounterRepository encounterRepository,
    IEncounterParticipantsRepository encounterParticipantsRepository
) : ComponentBase
{
    private string _createName = string.Empty;
    private int _createHp;
    private int _createAc;

    internal Encounter? CurrentEncounter { get; private set; }
    
    internal List<EncounterParticipant> Participants { get; private set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await SelectEncounterAsync(CurrentEncounter);
    }

    internal async Task SelectEncounterAsync(Encounter? encounter)
    {
        CurrentEncounter = encounter;
        await UpdateParticipantsAsync();
        StateHasChanged();
    }

    private async Task UpdateParticipantsAsync()
    {
        if (CurrentEncounter is null)
            Participants = [];
        else
            Participants = await encounterParticipantsRepository.GetAllEncounterParticipantsAsync(CurrentEncounter.Id);
    }

    private async Task SaveParticipantAsync()
    {
        if (CurrentEncounter is null)
            return;

        var index = 0;
        foreach (var item in Participants)
        {
            item.Order = index++;
        }
        
        await encounterParticipantsRepository.SetEncounterParticipantsAsync(CurrentEncounter.Id, Participants);
        await UpdateParticipantsAsync();
        
        StateHasChanged();
    }

    private async Task UpdateEncounterAsync(int round, int? currentActiveParticipantId)
    {
        if (CurrentEncounter is null)
            return;

        CurrentEncounter.CurrentRound = round;
        CurrentEncounter.CurrentActiveParticipantId = currentActiveParticipantId;

        await encounterRepository.UpdateEncounterAsync(
            new(
                CurrentEncounter.Id,
                null,
                CurrentRound: round,
                CurrentActiveParticipantId: currentActiveParticipantId,
                SkipDead: null
            )
        );
    }

    internal async Task SaveSkipDead()
    {
        if (CurrentEncounter is null)
            return;

        await encounterRepository.UpdateEncounterAsync(
            new(
                CurrentEncounter.Id,
                null,
                null,
                null,
                CurrentEncounter.SkipDead
            )
        );
    }

    internal async Task AddToEncounter(EncounterParticipant item)
    {
        if (CurrentEncounter is null)
            return;

        item.Encounter = CurrentEncounter;
        item.EncounterId = CurrentEncounter.Id;

        Participants.Add(item);
        await SaveParticipantAsync();
    }

    internal async Task AddManual()
    {
        if (CurrentEncounter is null)
            return;

        var item = new EncounterParticipant
        {
            Encounter = CurrentEncounter,
            EncounterId = CurrentEncounter.Id,

            Name = _createName,
            HitsAverage = _createHp,
            ArmorClass = _createAc,
            Source = Source.Manual,
        };
        item.Initialize(HitsMode.Average);

        _createName = string.Empty;
        _createHp = 0;
        _createAc = 0;
        
        Participants.Add(item);
        await SaveParticipantAsync();
    }

    internal async Task Remove(int participantId)
    {
        if (CurrentEncounter is null)
            return;
        
        var currentItem = Participants.FirstOrDefault(p => p.Id == participantId);
        if (currentItem is null)
            return;

        if (CurrentEncounter.CurrentActiveParticipantId == participantId)
        {
            var (currentRound, currentActiveParticipantId) = CalculateNext();
            await UpdateEncounterAsync(currentRound, currentActiveParticipantId);
        }
        
        Participants.Remove(currentItem);
        await SaveParticipantAsync();
    }

    internal async Task Move(int participantId, bool isUp)
    {
        if (CurrentEncounter is null)
            return;

        var currentItem = Participants.FirstOrDefault(p => p.Id == participantId);
        if (currentItem is null)
            return;

        var oldIndex = Participants.IndexOf(currentItem);
        if (oldIndex <= -1)
            return;

        var newIndex = isUp ? oldIndex - 1 : oldIndex + 1;
        if (newIndex < 0)
            newIndex = 0;

        if (newIndex >= Participants.Count)
            return;

        Participants.RemoveAt(oldIndex);
        Participants.Insert(newIndex, currentItem);
        await SaveParticipantAsync();
    }

    #region Controls

    private (int CurrentRound, int? CurrentActiveParticipantId) CalculateNext()
    {
        if (CurrentEncounter is null)
            throw new InvalidOperationException();

        if (Participants.Count == 0)
            return (CurrentEncounter.CurrentRound, null);

        var currentRound = CurrentEncounter.CurrentRound;
        var currentActiveParticipantId = CurrentEncounter.CurrentActiveParticipantId;

        if (currentActiveParticipantId is null)
            return (currentRound, PickFirstParticipant()?.Id);

        var currentIndex = Participants.FindIndex(p => p.Id == currentActiveParticipantId);
        var skipDead = CurrentEncounter.SkipDead;

        for (var step = 1; step <= Participants.Count; step++)
        {
            var wrapped = currentIndex + step >= Participants.Count;
            var candidate = Participants[(currentIndex + step) % Participants.Count];

            if (!skipDead || candidate.State is not HealthState.Dead)
                return (currentRound + (wrapped ? 1 : 0), candidate.Id);
        }

        var nextIndex = currentIndex + 1;
        if (nextIndex >= Participants.Count)
        {
            currentRound++;
            nextIndex = 0;
        }

        return (currentRound, Participants[nextIndex].Id);
    }

    private EncounterParticipant? PickFirstParticipant()
    {
        if (CurrentEncounter?.SkipDead is true)
        {
            var living = Participants.FirstOrDefault(p => p.State is not HealthState.Dead);
            if (living is not null)
                return living;
        }

        return Participants.FirstOrDefault();
    }

    internal async Task Next()
    {
        if (CurrentEncounter is null)
            return;

        var (currentRound, currentActiveParticipantId) = CalculateNext();

        await UpdateEncounterAsync(currentRound, currentActiveParticipantId);
        await SaveParticipantAsync();
    }

    internal async Task SortByInitiative()
    {
        if (CurrentEncounter is null)
            return;

        Participants = Participants.OrderByDescending(i => i.Initiative).ToList();
        await SaveParticipantAsync();
    }

    internal async Task Restart()
    {
        if (CurrentEncounter is null)
            return;

        await UpdateEncounterAsync(1, Participants.FirstOrDefault()?.Id);

        foreach (var item in Participants)
        {
            item.Reset();
        }

        await SaveParticipantAsync();
    }

    internal async Task Clear()
    {
        if (CurrentEncounter is null)
            return;

        Participants = [];
        await UpdateEncounterAsync(1, null);
        await SaveParticipantAsync();
    }

    #endregion
}