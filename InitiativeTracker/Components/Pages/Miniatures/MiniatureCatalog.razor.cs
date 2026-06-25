using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class MiniatureCatalog
{
    private List<MiniatureEntity> _miniatures = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Inject] IMiniatureService MiniatureService { get; set; } = default!;
    [Parameter] public EventCallback<MiniatureEntity?> OnEditSelected { get; set; }
    [Parameter] public EventCallback OnDataChanged { get; set; }
    [Parameter] public EventCallback<MiniatureEntity> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllMiniatures();

    public async Task RefreshAsync()
    {
        _searchQuery = string.Empty;
        await LoadAllMiniatures();
    }

    private async Task LoadAllMiniatures()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allItems = await MiniatureService.SearchAsync(string.Empty);
            _miniatures = new List<MiniatureEntity>(allItems);
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
            var results = await MiniatureService.SearchAsync(_searchQuery);
            if (version == _pendingSearchVersion)
                _miniatures = [..results];
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search miniatures: {ex.Message}");
            StateHasChanged();
        }
        finally
        {
            _isLoading = false;
            _isSearching = false;
        }
    }

    private async Task SelectForEdit(MiniatureEntity? miniature) => await OnEditSelected.InvokeAsync(miniature);

    private async Task OnAddToPrint(MiniatureEntity? miniature) => await OnAddForPrint.InvokeAsync(miniature!);

    private async Task OnDelete(MiniatureEntity miniature)
    {
        try
        {
            await MiniatureService.DeleteAsync(miniature.Id);
            _miniatures.Remove(miniature);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete miniature: {ex.Message}");
        }
    }
}
