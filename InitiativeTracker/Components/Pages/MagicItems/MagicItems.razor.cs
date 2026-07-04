using InitiativeTracker.Domain.Entities;

namespace InitiativeTracker.Components.Pages.MagicItems;

public partial class MagicItems
{
    private MagicItem? _editItem;
    private MagicItemPreparationList? PreparationListRef;
    private MagicItemCatalog? Catalog;

    private Task OnEditSelected(MagicItem? item) => OnEditItemChanged(item);

    private async Task OnEditItemChanged(MagicItem? item)
    {
        _editItem = item;
        StateHasChanged();
    }

    private async Task InvalidateCatalog()
    {
        if (Catalog is not null)
            await Catalog.OnSearch();
    }

    private async Task OnAddForPrint(MagicItem item)
    {
        PreparationListRef?.AddItem(item, 1);
        StateHasChanged();
    }
}
