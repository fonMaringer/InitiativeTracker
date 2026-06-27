using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class Miniatures
{
    private int _catalogKey;
    private Miniature? _editMiniature;
    private MiniaturePreparationList? PreparationListRef;

    private Task OnEditSelected(Miniature? miniature) => OnEditMiniatureChanged(miniature);

    private async Task OnEditMiniatureChanged(Miniature? miniature)
    {
        _editMiniature = miniature;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(Miniature miniature)
    {
        PreparationListRef?.AddItem(miniature, 1);
        StateHasChanged();
    }
}
