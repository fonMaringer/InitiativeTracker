using InitiativeTracker.Components.Pages.Encounters;
using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages;

public partial class Home
{
    private EncounterCatalog? _encounterCatalogRef;
    private Encounters.InitiativeTracker? _initiativeTrackerRef;
    
    protected override async Task OnInitializedAsync()
    {
        if (_initiativeTrackerRef is not null)
            await _initiativeTrackerRef.SelectEncounterAsync(_encounterCatalogRef?.SelectedEncounter);
        StateHasChanged();
    }

    public async Task OnSelectEncounter(Encounter encounter)
    {
        if (_initiativeTrackerRef is not null)
            await _initiativeTrackerRef.SelectEncounterAsync(encounter);
        StateHasChanged();
    }

    private async Task OnAddToEncounter(EncounterParticipant item)
    {
        if (_initiativeTrackerRef is not null)
            await _initiativeTrackerRef.AddToEncounter(item);
        StateHasChanged();
    }
}
