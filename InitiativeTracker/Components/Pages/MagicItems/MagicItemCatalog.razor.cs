using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.MagicItems;

public partial class MagicItemCatalog(IMagicItemRepository magicItemRepository)
{
    private List<MagicItem> _items = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Parameter] 
    public EventCallback<MagicItem?> OnEditSelected { get; set; }
    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public EventCallback<MagicItem> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllItems();

    private async Task LoadAllItems()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allItems = await magicItemRepository.SearchAsync(string.Empty);
            _items = allItems.OrderBy(i => i.Name).ToList();
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

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
            await _onSearch();
    }

    private async Task _onSearch()
    {
        if (_isSearching) return;

        var version = Interlocked.Increment(ref _pendingSearchVersion);
        _isSearching = true;

        try
        {
            _isLoading = true;
            StateHasChanged();
            var results = await magicItemRepository.SearchAsync(_searchQuery);
            if (version == _pendingSearchVersion)
                _items = results.OrderBy(i => i.Name).ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search items: {ex.Message}");
            StateHasChanged();
        }
        finally
        {
            _isLoading = false;
            _isSearching = false;
        }
    }

    private async Task SelectForEdit(MagicItem? item) => await OnEditSelected.InvokeAsync(item);

    private async Task OnAddToPrint(MagicItem? item) => await OnAddForPrint.InvokeAsync(item!);

    private async Task OnDelete(MagicItem item)
    {
        try
        {
            await magicItemRepository.DeleteAsync(item.Id);
            _items.Remove(item);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete item: {ex.Message}");
        }
    }
}
