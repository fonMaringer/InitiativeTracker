using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Stands;

public partial class Stands
{
    private Stand? _editStand;
    private StandPreparationList? PreparationListRef;
    private StandCatalog? Catalog;

    private Task OnEditSelected(Stand? stand) => OnEditStandChanged(stand);

    private async Task OnEditStandChanged(Stand? stand)
    {
        _editStand = stand;
        StateHasChanged();
    }

    private async Task InvalidateCatalog()
    {
        if (Catalog is not null)
            await Catalog.LoadAllStands();
    }

    private async Task OnAddForPrint(Stand stand)
    {
        PreparationListRef?.AddItem(stand, 1);
        StateHasChanged();
    }
}
