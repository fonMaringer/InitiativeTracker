using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class Miniatures
{
    private Miniature? _editMiniature;
    private MiniaturePreparationList? PreparationListRef;
    private MiniatureCatalog? Catalog;

    private Task OnEditSelected(Miniature? miniature) => OnEditMiniatureChanged(miniature);

    private async Task OnEditMiniatureChanged(Miniature? miniature)
    {
        _editMiniature = miniature;
        StateHasChanged();
    }

    private async Task InvalidateCatalog()
    {
        if (Catalog is not null)
            await Catalog.OnSearch();
    }

    private async Task OnAddForPrint(MiniatureCatalogDto miniature)
    {
        PreparationListRef?.AddItem(miniature, 1);
        StateHasChanged();
    }
}
