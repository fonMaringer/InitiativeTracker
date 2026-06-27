using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace InitiativeTracker.Components.Pages.Encounters;

public partial class EncounterCatalog(
    IEncounterRepository encounterRepository,
    IJSRuntime jsRuntime
    ) : ComponentBase
{
    private bool _isLoading;

    private List<Encounter> _encounters = [];
    
    private string _newEncounterName = string.Empty;
    private bool _isRenaming;
    private int _renamingId;
    private string _renameValue = string.Empty;
    
    internal Encounter? SelectedEncounter { get; private set; }
    
    [Parameter]
    public EventCallback<Encounter?> OnSelectEncounter { get; set; }

    public async Task SelectEncounter(int? encounterId)
    {
        if (encounterId is null)
            SelectedEncounter = _encounters.FirstOrDefault();
        else
            SelectedEncounter = await encounterRepository.GetEncounterByIdAsync(encounterId.Value);
        
        await OnSelectEncounter.InvokeAsync(SelectedEncounter);
        StateHasChanged();
    }

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
            _encounters = await encounterRepository.GetAllEncountersAsync();
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

    private async Task AddEncounter()
    {
        if (string.IsNullOrWhiteSpace(_newEncounterName))
            return;
        
        SelectedEncounter = await encounterRepository.CreateEncounterAsync(new (_newEncounterName.Trim()));
        _newEncounterName = string.Empty;
        _encounters.Add(SelectedEncounter);
        await OnSelectEncounter.InvokeAsync(SelectedEncounter);
        StateHasChanged();
    }

    private async Task AddEncounterWithKey(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter")
            await AddEncounter();
    }

    private async Task DeleteEncounter(int encounterId, string name)
    {
        var confirmed = await jsRuntime.InvokeAsync<bool>("confirm", $"Delete \"{name}\" and all its entries?");
        if (confirmed)
        {
            await encounterRepository.DeleteEncounterAsync(encounterId);
            
            var encounter = _encounters.FirstOrDefault(e => e.Id == encounterId);
            var index = 0;
            if (encounter is not null)
            {
                index = _encounters.IndexOf(encounter);
                _encounters.Remove(encounter);
            }
            var newIndex = Math.Max(index - 1, 0);
            if (_encounters.Any())
            {
                SelectedEncounter = _encounters[newIndex];
                await OnSelectEncounter.InvokeAsync(SelectedEncounter);
            }
            else
            {
                SelectedEncounter = null;
                await OnSelectEncounter.InvokeAsync(null);
            }
            StateHasChanged();
        }
    }

    private void StartRename(int encounterId, string currentName)
    {
        _isRenaming = true;
        _renamingId = encounterId;
        _renameValue = currentName;
        StateHasChanged();
    }

    private async Task CommitRename()
    {
        if (string.IsNullOrWhiteSpace(_renameValue))
        {
            _isRenaming = false;
            return;
        }

        await encounterRepository.UpdateEncounterAsync(
            new(
                _renamingId,
                _renameValue,
                null,
                null
            )
        );
        _isRenaming = false;
        await LoadAll();
        await OnSelectEncounter.InvokeAsync(SelectedEncounter);
        StateHasChanged();
    }

    private void CancelRename()
    {
        _isRenaming = false;
        StateHasChanged();
    }

    private async Task HandleRenameKey(KeyboardEventArgs e)
    {
        switch (e.Code)
        {
            case "Enter":
                await CommitRename();
                break;
            case "Escape":
                CancelRename();
                break;
        }
    }
}