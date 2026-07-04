using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace InitiativeTracker.Components.Pages.Encounters;

public partial class ParticipantCatalog(
    IParticipantRepository participantRepository
    ) : ComponentBase
{
    private IReadOnlyCollection<ParticipantCatalogItem> _participants = [];

    private bool _isLoading;
    private int? _selectedParticipantId;
    private string _newLibraryName = string.Empty;
    private int _newLibraryDex = 10;
    private int _newLibraryHp = 1;
    private int _newLibraryAc = 10;

    [Parameter]
    public EventCallback<EncounterParticipant> OnAddToEncounter { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAll();
    }

    private async Task LoadAll()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            _participants = await participantRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load catalog: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task AddToEncounter(int id)
    {
        var participant = await participantRepository.GetByIdAsync(id);
        if (participant is null)
            return;

        var item = new EncounterParticipant
        {
            Name = participant.Name,
            Dexterity = participant.Dexterity,
            Source = Source.Manual,
            HitsAverage = participant.Hits,
            HitsCurrent = participant.Hits,
            ArmorClass = participant.ArmorClass,
            ArmorClassCurrent = participant.ArmorClass,
        };

        await OnAddToEncounter.InvokeAsync(item);
    }

    private async Task UpsertParticipant()
    {
        if (string.IsNullOrWhiteSpace(_newLibraryName))
            return;

        if (_selectedParticipantId is null)
        {
            await participantRepository.CreateAsync(new ParticipantCreateDto(
                Name: _newLibraryName.Trim(),
                Dexterity: _newLibraryDex,
                Hp: _newLibraryHp,
                Ac: _newLibraryAc
            ));
        }
        else
        {
            await participantRepository.UpdateAsync(new ParticipantUpdateDto(
                Id: _selectedParticipantId.Value,
                Name: _newLibraryName.Trim(),
                Hp: _newLibraryHp,
                Ac: _newLibraryAc,
                Dexterity: _newLibraryDex
            ));
        }

        _selectedParticipantId = null;
        _newLibraryName = string.Empty;
        _newLibraryDex = 10;
        _newLibraryHp = 1;
        _newLibraryAc = 10;
        await LoadAll();
    }

    private async Task DeleteLibraryParticipant(int id)
    {
        await participantRepository.DeleteAsync(id);
        await LoadAll();
        StateHasChanged();
    }

    private async Task SelectParticipantForEdit(int id)
    {
        var participant = await participantRepository.GetByIdAsync(id);
        if (participant is null)
            return;

        _selectedParticipantId = id;
        _newLibraryName = participant.Name;
        _newLibraryHp = participant.Hits;
        _newLibraryAc = participant.ArmorClass;
        _newLibraryDex = participant.Dexterity;
        StateHasChanged();
    }
}