using InitiativeTracker.DataAccess.Dtos;
using InitiativeTracker.DataAccess.Repositories;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Domain.Enums;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Adapters;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Components.Pages.MagicItems;

public partial class AddMagicItemForm(
    IMagicItemsClient magicItemsClient,
    IMagicItemRepository magicItemRepository
    ) : IDisposable
{
    private string _name = string.Empty;
    private string? _type;
    private string? _link;
    private ItemRarity _rarity = ItemRarity.Common;
    private bool _requiresAttunement;
    private string _description = string.Empty;
    private bool _isProcessing;
    private string? _errorMessage;
    private string? _successMessage;
    private int? _printedCount = 0;
    private Source _source;

    private readonly ItemRarity[] _rarityOptions = Enum.GetValues<ItemRarity>();
    private bool _isDisabled => _isProcessing || string.IsNullOrWhiteSpace(_name);

    [Parameter]
    public EventCallback OnDataChanged { get; set; }
    [Parameter]
    public MagicItem? EditItem { get; set; }
    [Parameter]
    public EventCallback<MagicItem?> OnEditItemChanged { get; set; }

    private MagicItem? _editItem;
    private CancellationTokenSource _searchCts = new();

    protected override void OnParametersSet()
    {
        _editItem = EditItem;
        if (_editItem != null)
        {
            _name = _editItem.Name;
            _type = _editItem.Type;
            _link = _editItem.Link;
            _rarity = _editItem.Rarity;
            _requiresAttunement = _editItem.RequiresAttunement;
            _description = _editItem.Description;
            _errorMessage = null;
            _successMessage = null;
            _printedCount = _editItem.PrintedCount;
            _source = default;
        }
        else
        {
            ResetForm();
        }
    }

    private string? _searchPattern;
    private MagicItemsSearchResponseItem[]? _searchResults;

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
        {
            await SearchItems();
        }
    }

    public void Dispose()
    {
        _searchCts.Cancel();
        _searchCts.Dispose();
    }

    private async Task SearchItems()
    {
        _searchResults = null;
        StateHasChanged();

        if (string.IsNullOrEmpty(_searchPattern))
        {
            return;
        }

        await _searchCts.CancelAsync();
        _searchCts.Token.Register(StateHasChanged);
        _searchCts.Dispose();
        _searchCts = new();

        try
        {
            _searchResults = await magicItemsClient.SearchV1Async(_searchPattern, _searchCts.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignored due to re-search throttling
        }
    }

    private async Task SaveFromItem(MagicItemsSearchResponseItem item)
    {
        await SelectItem(item);
        await OnSave();
    }

    private async Task SelectItem(MagicItemsSearchResponseItem item)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        var details = await magicItemsClient.GetDetailsV1Async(url, _searchCts.Token);

        if (details is null)
            return;

        var link = magicItemsClient.BuildDirectLink(item.Url);
        var entity = details.ToItemEntity(link);

        _name = entity.Name;
        _type = entity.Type;
        _link = entity.Link;
        _rarity = entity.Rarity;
        _requiresAttunement = entity.RequiresAttunement;
        _description = entity.Description;
        _source = entity.Source;
    }

    private async Task OnSave()
    {
        _isProcessing = true;
        _errorMessage = null;
        _successMessage = null;

        try
        {
            if (_editItem != null)
            {
                await magicItemRepository.UpdateAsync(_editItem.Id, new MagicItemUpdateDto(
                    Name: _name,
                    Type: _type,
                    Rarity: _rarity,
                    RequiresAttunement: _requiresAttunement,
                    Description: _description,
                    PrintedCount: _printedCount,
                    Link: _link));

                _successMessage = $"'{_name}' updated successfully.";
                await OnEditItemChanged.InvokeAsync(null);
                await OnDataChanged.InvokeAsync();
            }
            else
            {
                await magicItemRepository.AddAsync(new MagicItemCreateDto(
                    Name: _name,
                    Type: _type,
                    Rarity: _rarity,
                    RequiresAttunement: _requiresAttunement,
                    Description: _description,
                    PrintedCount: _printedCount ?? 0,
                    Link: _link,
                    Source: _source));

                _successMessage = $"'{_name}' added successfully.";
                ResetForm();
                await OnDataChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save item: {ex.Message}";
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private async Task OnCancel()
    {
        await OnEditItemChanged.InvokeAsync(null);
    }

    private void ResetForm()
    {
        _name = string.Empty;
        _type = null;
        _link = null;
        _printedCount = null;
        _rarity = ItemRarity.Common;
        _requiresAttunement = false;
        _description = string.Empty;
        _source = default;
    }
}
