using InitiativeTracker.Application;
using InitiativeTracker.Application.Dtos;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Adapters;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Components.Pages.Spells;

public partial class AddSpellForm : IDisposable
{
    private string _name = string.Empty;
    private string? _link;
    private string _type = string.Empty;
    private int _level;
    private string _range = string.Empty;
    private string _duration = string.Empty;
    private string _time = string.Empty;
    private bool _concentration;
    private List<string> _classes = [];
    private string _classInput = string.Empty;
    private List<string> _subclasses = [];
    private string _subclassInput = string.Empty;
    private string _upper = string.Empty;
    private bool _verbalComponent;
    private bool _somaticComponent;
    private bool _materialComponentEnabled;
    private string? _materialComponent;
    private string _description = string.Empty;
    private bool _isProcessing;
    private string? _errorMessage;
    private string? _successMessage;
    private int? _printedCount;

    private CancellationTokenSource _searchCts = new();
    private string? _searchPattern;
    private SpellsSearchResponseItem[]? _searchResults;

    private bool _isDisabled => _isProcessing || string.IsNullOrWhiteSpace(_name);

    [Parameter] public EventCallback OnDataChanged { get; set; }
    [Parameter] public SpellEntity? EditSpell { get; set; }
    [Parameter] public EventCallback<SpellEntity?> OnEditSpellChanged { get; set; }

    private SpellEntity? _editSpell;

    protected override void OnParametersSet()
    {
        _editSpell = EditSpell;
        if (_editSpell != null)
        {
            _name = _editSpell.Name;
            _type = _editSpell.Type;
            _level = _editSpell.Level;
            _range = _editSpell.Range;
            _duration = _editSpell.Duration;
            _time = _editSpell.Time;
            _concentration = _editSpell.Concentration;
            _classes = new (_editSpell.Classes);
            _subclasses = new (_editSpell.Subclasses);
            _upper = _editSpell.Upper ?? string.Empty;
            _verbalComponent = _editSpell.VerbalComponent;
            _somaticComponent = _editSpell.SomaticComponent;
            _materialComponentEnabled = !string.IsNullOrEmpty(_editSpell.MaterialComponent);
            _materialComponent = _editSpell.MaterialComponent;
            _description = _editSpell.Description;
            _errorMessage = null;
            _successMessage = null;
            _printedCount = _editSpell.PrintedCount;
            _link = _editSpell.Link;
        }
        else
        {
            ResetForm();
        }
    }

    [Inject] ISpellsClient SpellsClient { get; set; } = default!;
    [Inject] ISpellService SpellService { get; set; } = default!;

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
        {
            await SearchSpells();
        }
    }

    private async Task SearchSpells()
    {
        _searchResults = null;
        StateHasChanged();

        if (string.IsNullOrEmpty(_searchPattern))
            return;

        await _searchCts.CancelAsync();
        _searchCts.Token.Register(StateHasChanged);
        _searchCts.Dispose();
        _searchCts = new();

        try
        {
            _searchResults = await SpellsClient.SearchV1Async(_searchPattern, _searchCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignored due to re-search throttling
        }
    }

    private async Task SaveFromItem(SpellsSearchResponseItem item)
    {
        await SelectSpell(item);
        await OnSave();
    }

    private async Task SelectSpell(SpellsSearchResponseItem item)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
            return;

        var details = await SpellsClient.GetDetailsV1Async(url, _searchCts.Token);
        if (details is null)
            return;

        var link = SpellsClient.BuildDirectLink(details.Url);
        var entity = details.ToSpellEntity(link);

        _name = entity.Name;
        _link = entity.Link;
        _type = entity.Type;
        _level = entity.Level;
        _range = entity.Range;
        _duration = entity.Duration;
        _time = entity.Time;
        _concentration = entity.Concentration;
        _classes = new (entity.Classes);
        _subclasses = new (entity.Subclasses);
        _upper = entity.Upper ?? string.Empty;
        _verbalComponent = entity.VerbalComponent;
        _somaticComponent = entity.SomaticComponent;
        _materialComponentEnabled = !string.IsNullOrEmpty(entity.MaterialComponent);
        _materialComponent = entity.MaterialComponent;
        _description = entity.Description;
    }

    private async Task OnSave()
    {
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            if (_editSpell != null)
            {
                await SpellService.UpdateAsync(_editSpell.Id, new SpellUpdateDto(
                    Name: _name,
                    Type: _type,
                    VerbalComponent: _verbalComponent,
                    SomaticComponent: _somaticComponent,
                    MaterialComponent: _materialComponentEnabled ? _materialComponent : null,
                    Classes: _classes.ToArray(),
                    Subclasses: _subclasses.ToArray(),
                    Description: _description,
                    PrintedCount: _printedCount,
                    Link: _link));

                _successMessage = $"'{_name}' updated successfully.";
                await OnEditSpellChanged.InvokeAsync(null);
                await OnDataChanged.InvokeAsync();
            }
            else
            {
                await SpellService.AddAsync(new SpellCreateDto(
                    Name: _name,
                    Type: _type,
                    VerbalComponent: _verbalComponent,
                    SomaticComponent: _somaticComponent,
                    MaterialComponent: _materialComponentEnabled ? _materialComponent : null,
                    Classes: _classes.ToArray(),
                    Subclasses: _subclasses.ToArray(),
                    Description: _description,
                    PrintedCount: _printedCount ?? 0,
                    Link: _link,
                    Range: _range,
                    Duration: _duration,
                    Time: _time,
                    Level: _level,
                    Upper: string.IsNullOrWhiteSpace(_upper) ? null : _upper,
                    Concentration: _concentration,
                    Source: Source.Manual));

                _successMessage = $"'{_name}' added successfully.";
                ResetForm();
                await OnDataChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save spell: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task OnCancel()
    {
        await OnEditSpellChanged.InvokeAsync(null);
    }

    private void ResetForm()
    {
        _name = string.Empty;
        _type = string.Empty;
        _level = 0;
        _range = string.Empty;
        _duration = string.Empty;
        _time = string.Empty;
        _concentration = false;
        _classes = [];
        _classInput = string.Empty;
        _subclasses = [];
        _subclassInput = string.Empty;
        _upper = string.Empty;
        _verbalComponent = false;
        _somaticComponent = false;
        _materialComponentEnabled = false;
        _materialComponent = null;
        _description = string.Empty;
        _printedCount = null;
        _link = null;
    }

    private void AddClass()
    {
        if (string.IsNullOrWhiteSpace(_classInput))
            return;

        var cls = _classInput.Trim();
        if (!_classes.Contains(cls, StringComparer.OrdinalIgnoreCase))
        {
            _classes.Add(cls);
        }
        _classInput = string.Empty;
        StateHasChanged();
    }

    private void RemoveClass(string cls)
    {
        _classes.Remove(cls);
        StateHasChanged();
    }

    private async Task OnKeyUpAddClass(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
        {
            AddClass();
        }
    }

    private void AddSubclass()
    {
        if (string.IsNullOrWhiteSpace(_subclassInput))
            return;

        var subcls = _subclassInput.Trim();
        if (!_subclasses.Contains(subcls, StringComparer.OrdinalIgnoreCase))
        {
            _subclasses.Add(subcls);
        }
        _subclassInput = string.Empty;
        StateHasChanged();
    }

    private void RemoveSubclass(string subcls)
    {
        _subclasses.Remove(subcls);
        StateHasChanged();
    }

    private async Task OnKeyUpAddSubclass(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
        {
            AddSubclass();
        }
    }

    public void Dispose()
    {
        _searchCts.Cancel();
        _searchCts.Dispose();
    }
}
