using System.Text.RegularExpressions;
using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class SpellCatalog
{
    private List<SpellEntity> _spells = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Inject] ISpellService SpellService { get; set; } = default!;
    [Parameter] public EventCallback<SpellEntity?> OnEditSelected { get; set; }
    [Parameter] public EventCallback OnDataChanged { get; set; }
    [Parameter] public EventCallback<SpellEntity> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllSpells();

    public async Task RefreshAsync()
    {
        _searchQuery = string.Empty;
        await LoadAllSpells();
    }

    private async Task LoadAllSpells()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allSpells = await SpellService.SearchAsync(string.Empty);
            _spells = new List<SpellEntity>(allSpells);
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
            var results = await SpellService.SearchAsync(_searchQuery);
            if (version == _pendingSearchVersion)
                _spells = [..results];
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to search spells: {ex.Message}");
            StateHasChanged();
        }
        finally
        {
            _isLoading = false;
            _isSearching = false;
        }
    }

    private async Task SelectForEdit(SpellEntity? spell) => await OnEditSelected.InvokeAsync(spell);

    private async Task OnAddToPrint(SpellEntity? spell) => await OnAddForPrint.InvokeAsync(spell!);

    private async Task OnDelete(SpellEntity spell)
    {
        try
        {
            await SpellService.DeleteAsync(spell.Id);
            _spells.Remove(spell);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete spell: {ex.Message}");
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
