using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class SpellCatalog(ISpellRepository spellRepository)
{
    private List<Spell> _spells = [];
    private bool _isLoading;
    private string _searchQuery = string.Empty;

    [Parameter]
    public EventCallback<Spell?> OnEditSelected { get; set; }
    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public EventCallback<Spell> OnAddForPrint { get; set; }

    private volatile bool _isSearching;
    private int _pendingSearchVersion;

    protected override async Task OnInitializedAsync() => await LoadAllSpells();

    private async Task LoadAllSpells()
    {
        _isLoading = true;
        StateHasChanged();
        try
        {
            var allSpells = await spellRepository.SearchAsync(string.Empty);
            _spells = new List<Spell>(allSpells);
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
            var results = await spellRepository.SearchAsync(_searchQuery);
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

    private async Task SelectForEdit(Spell? spell) => await OnEditSelected.InvokeAsync(spell);

    private async Task OnAddToPrint(Spell? spell) => await OnAddForPrint.InvokeAsync(spell!);

    private async Task OnDelete(Spell spell)
    {
        try
        {
            await spellRepository.DeleteAsync(spell.Id);
            _spells.Remove(spell);
            StateHasChanged();
            await OnDataChanged.InvokeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to delete spell: {ex.Message}");
        }
    }
}
