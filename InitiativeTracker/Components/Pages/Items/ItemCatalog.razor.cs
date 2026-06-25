using System.Text.RegularExpressions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Items;

public partial class ItemCatalog
{
    private List<ItemEntity> _items = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Inject] private IItemService ItemService { get; set; } = default!;
    [Parameter] public EventCallback<ItemEntity?> OnEditSelected { get; set; }
    [Parameter] public EventCallback OnDataChanged { get; set; }
    [Parameter] public EventCallback<ItemEntity> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllItems();

    public async Task RefreshAsync()
    {
        _searchQuery = string.Empty;
        await LoadAllItems();
    }

    private async Task LoadAllItems()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allItems = await ItemService.SearchAsync(string.Empty);
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
            var results = await ItemService.SearchAsync(_searchQuery);
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

    private async Task SelectForEdit(ItemEntity? item) => await OnEditSelected.InvokeAsync(item);

    private async Task OnAddToPrint(ItemEntity? item) => await OnAddForPrint.InvokeAsync(item!);

    private async Task OnDelete(ItemEntity item)
    {
        try
        {
            await ItemService.DeleteAsync(item.Id);
            _items.Remove(item);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete item: {ex.Message}");
        }
    }

    private string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        var stripped = Regex.Replace(text, "<.*?>", string.Empty);
        return stripped.Substring(0, maxLength) + "...";
    }
}
