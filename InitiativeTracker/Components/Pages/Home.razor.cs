using InitiativeTracker.Application;
using InitiativeTracker.Domain.Entities;
using InitiativeTracker.Integration.RestClients.TtgClub;
using InitiativeTracker.Integration.RestClients.TtgClub.Adapters;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Source = InitiativeTracker.Domain.Enums.Source;

namespace InitiativeTracker.Components.Pages;

public partial class Home : IDisposable
{
    private CancellationTokenSource _searchCts = new();
    private BestiarySearchResponseItem[]? _searchResults;
    private string? _searchPattern;

    private string _newEncounterName = string.Empty;
    private bool _isRenaming = false;
    private int _renamingId = 0;
    private string _renameValue = string.Empty;

    private GlobalParticipantDto[] _libraryParticipants = [];

    private string _newLibraryName = string.Empty;
    private int _newLibraryDex = 0;
    private int _newLibraryHp = 1;
    private int _newLibraryAc = 0;

    private string _createName = string.Empty;
    private int _createHp;
    private int _createAc;

    [Inject] private IBestiaryClient BestiaryClient { get; set; } = default!;
    [Inject] private IInitiativeService InitiativeService { get; set; } = default!;
    [Inject] private IParticipantLibraryService LibraryService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        await RefreshLibrary();
    }

    private async Task RefreshLibrary()
    {
        _libraryParticipants = (await LibraryService.GetAllAsync()).ToArray();
    }

    // --- Encounter management (inline, no modals) ---

    private async Task AddEncounter()
    {
        if (string.IsNullOrWhiteSpace(_newEncounterName))
            return;

        var id = await InitiativeService.CreateEncounterAsync(_newEncounterName.Trim());
        InitiativeService.SelectEncounter(id);
        _newEncounterName = string.Empty;
        StateHasChanged();
    }

    private async Task AddEncounterWithKey(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter")
            await AddEncounter();
    }

    private async Task DeleteEncounter(int encounterId, string name)
    {
        var confirmed = await JsRuntime.InvokeAsync<bool>("confirm", $"Delete \"{name}\" and all its entries?");
        if (confirmed)
        {
            await InitiativeService.DeleteEncounterAsync(encounterId);
            StateHasChanged();
        }
    }

    private void StartRename(int encounterId, string currentName)
    {
        _isRenaming = true;
        _renamingId = encounterId;
        _renameValue = currentName;
        StateHasChanged();
    }

    private async Task CommitRename()
    {
        if (string.IsNullOrWhiteSpace(_renameValue))
        {
            _isRenaming = false;
            return;
        }

        await InitiativeService.RenameEncounterAsync(_renamingId, _renameValue.Trim());
        _isRenaming = false;
        StateHasChanged();
    }

    private void CancelRename()
    {
        _isRenaming = false;
    }

    private async Task HandleRenameKey(KeyboardEventArgs e)
    {
        if (e.Code is "Enter")
            await CommitRename();
        else if (e.Code is "Escape")
            CancelRename();
    }

    // --- Bestiary search ---

    private async Task OnKeyUpSearch(KeyboardEventArgs? e)
    {
        if (e?.Code is "Enter" or "NumpadEnter")
            await Search();
    }

    private async Task Search()
    {
        if (string.IsNullOrEmpty(_searchPattern))
        {
            _searchResults = null;
            return;
        }

        await _searchCts.CancelAsync();
        _searchCts.Token.Register(StateHasChanged);
        _searchCts.Dispose();
        _searchCts = new();

        _searchResults = await BestiaryClient.SearchV1Async(_searchPattern, _searchCts.Token);
    }

    private async Task Add(BestiarySearchResponseItem item, HitsMode hitsMode)
    {
        var url = item.Url;
        if (string.IsNullOrEmpty(url))
            return;

        var details = await BestiaryClient.GetDetailsV1Async(url, _searchCts.Token);

        if (details is null)
            return;

        var link = BestiaryClient.BuildDirectLink(details.Url);
        for (var i = 0; i < item.AddCount; i++)
        {
            var itemDetails = details.ToInitiativeListItem(link, hitsMode);
            InitiativeService.Append(itemDetails);
        }

        item.AddCount = 1;
    }

    // --- Manual participant add ---

    private void AddManual()
    {
        var item = new InitiativeListItem
        {
            Name = _createName,
            HitsAverage = _createHp,
            ArmorClass = _createAc,
            Source = Source.Manual,
        };
        item.Initialize(HitsMode.Average);

        InitiativeService.Append(item);

        _createName = string.Empty;
        _createHp = 0;
        _createAc = 0;
    }

    // --- Library inline add ---

    private async Task AddFromLibrary(int id)
    {
        var participant = await LibraryService.GetByIdAsync(id);
        if (participant is null)
            return;

        var item = new InitiativeListItem
        {
            Name = participant.Name,
            Dexterity = participant.Dexterity,
            Source = Source.Manual,
            HitsAverage = participant.Hp,
            ArmorClass = participant.Ac,
        };
        item.Initialize(HitsMode.Average);
        InitiativeService.Append(item);
    }

    private async Task AddLibraryParticipant()
    {
        if (string.IsNullOrWhiteSpace(_newLibraryName))
            return;

        await LibraryService.CreateAsync(new CreateParticipantDto(
            Name: _newLibraryName.Trim(),
            Dexterity: _newLibraryDex,
            Hp: _newLibraryHp,
            Ac: _newLibraryAc
        ));

        _newLibraryName = string.Empty;
        _newLibraryDex = 0;
        _newLibraryHp = 1;
        _newLibraryAc = 0;
        await RefreshLibrary();
    }

    private async Task DeleteLibraryParticipant(int id)
    {
        await LibraryService.DeleteAsync(id);
        await RefreshLibrary();
        StateHasChanged();
    }

    // --- Controls ---

    private async Task SaveEncounters()
    {
        await InitiativeService.SaveAllAsync();
    }

    private async Task ClearAsync()
    {
        await InitiativeService.ClearAsync();
    }

    public void Dispose()
    {
        _searchCts.Cancel();
        _searchCts.Dispose();
        _searchCts = new();
    }

    private static string GetItemStyle(InitiativeListItem item) => item.State switch
    {
        HealthState.Healthy => "healthy-item",
        HealthState.SlightlyWounded => "slightly-wounded-item",
        HealthState.Wounded => "wounded-item",
        HealthState.SeriouslyWounded => "seriously-wounded-item",
        HealthState.Dead => "dead-item",
        _ => throw new ArgumentOutOfRangeException(nameof(item.State), item.State, null)
    };
}
