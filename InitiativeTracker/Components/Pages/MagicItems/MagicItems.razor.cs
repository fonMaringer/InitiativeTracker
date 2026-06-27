using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.MagicItems;

public partial class MagicItems
{
    private int _catalogKey;
    private MagicItem? _editItem;
    private MagicItemPreparationList? PreparationListRef;

    private Task OnEditSelected(MagicItem? item) => OnEditItemChanged(item);

    private async Task OnEditItemChanged(MagicItem? item)
    {
        _editItem = item;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(MagicItem item)
    {
        PreparationListRef?.AddItem(item, 1);
        StateHasChanged();
    }
}
