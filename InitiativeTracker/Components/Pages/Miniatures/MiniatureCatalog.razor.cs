using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Miniatures;

public partial class MiniatureCatalog(IMiniatureRepository miniatureRepository)
{
    private List<MiniatureCatalogDto> _miniatures = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Parameter]
    public EventCallback<Miniature?> OnEditSelected { get; set; }
    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public EventCallback<MiniatureCatalogDto> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllMiniatures();

    private async Task LoadAllMiniatures()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allItems = await miniatureRepository.SearchAsync(string.Empty);
            _miniatures = new List<MiniatureCatalogDto>(allItems);
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
            await OnSearch();
    }

    public async Task OnSearch()
    {
        if (_isSearching) return;

        var version = Interlocked.Increment(ref _pendingSearchVersion);
        _isSearching = true;

        try
        {
            _isLoading = true;
            StateHasChanged();
            var results = await miniatureRepository.SearchAsync(_searchQuery);
            if (version == _pendingSearchVersion)
                _miniatures = [..results];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search miniatures: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            _isSearching = false;
            StateHasChanged();
        }
    }

    private async Task SelectForEdit(MiniatureCatalogDto? dto)
    {
        if (dto == null)
            return;

        var miniature = await miniatureRepository.GetByIdAsync(dto.Id);
        await OnEditSelected.InvokeAsync(miniature);
    }

    private async Task OnAddToPrint(MiniatureCatalogDto? miniature) => await OnAddForPrint.InvokeAsync(miniature!);

    private async Task OnDelete(MiniatureCatalogDto miniature)
    {
        try
        {
            await miniatureRepository.DeleteAsync(miniature.Id);
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
