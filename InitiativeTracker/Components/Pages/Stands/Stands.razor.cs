using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Stands;

public partial class Stands
{
    private int _catalogKey;
    private Stand? _editStand;
    private StandPreparationList? PreparationListRef;

    private Task OnEditSelected(Stand? stand) => OnEditStandChanged(stand);

    private async Task OnEditStandChanged(Stand? stand)
    {
        _editStand = stand;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(Stand stand)
    {
        PreparationListRef?.AddItem(stand, 1);
        StateHasChanged();
    }
}
