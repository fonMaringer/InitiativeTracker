using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.Items;

public partial class Items
{
    private int _catalogKey;
    private ItemEntity? _editItem;
    private ItemPreparationList? PreparationListRef;

    private Task OnEditSelected(ItemEntity? item) => OnEditItemChanged(item);

    private async Task OnEditItemChanged(ItemEntity? item)
    {
        _editItem = item;
        StateHasChanged();
    }

    private void InvalidateCatalog()
    {
        _catalogKey++;
    }

    private async Task OnAddForPrint(ItemEntity item)
    {
        PreparationListRef?.AddItem(item, 1);
        StateHasChanged();
    }
}
