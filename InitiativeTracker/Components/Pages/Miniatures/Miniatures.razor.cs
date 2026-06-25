using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class Miniatures
{
    private int _catalogKey;
    private MiniatureEntity? _editMiniature;
    private MiniaturePreparationList? PreparationListRef;

    private Task OnEditSelected(MiniatureEntity? miniature) => OnEditMiniatureChanged(miniature);

    private async Task OnEditMiniatureChanged(MiniatureEntity? miniature)
    {
        _editMiniature = miniature;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(MiniatureEntity miniature)
    {
        PreparationListRef?.AddItem(miniature, 1);
        StateHasChanged();
    }
}
