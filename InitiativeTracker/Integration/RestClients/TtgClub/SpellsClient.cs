using System.Net.Http.Headers;
using System.Text.Json;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.Extensions.Options;

namespace InitiativeTracker.Integration.RestClients.TtgClub;

public interface ISpellsClient
{
    Task<SpellsSearchResponseItem[]> SearchV1Async(string query, CancellationToken ct);

    Task<SpellsDetailsResponse?> GetDetailsV1Async(string url, CancellationToken ct);

    string BuildDirectLink(string url);
}

public class SpellsClient(IOptions<TtgClubClientOptions> options) : ISpellsClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const string MediaType = "application/json";
    private readonly string _apiV1Path = options.Value.ApiV1Path;
    private const int LoadItemsCount = 100;

    private readonly HttpClient _magicItemsV1Client = new()
    {
        BaseAddress = new Uri(options.Value.Host),
        DefaultRequestHeaders =
        {
            { "Accept", MediaType },
        },
    };

    public string BuildDirectLink(string url) => options.Value.Host + url;

    public async Task<SpellsSearchResponseItem[]> SearchV1Async(string query, CancellationToken ct)
    {
        var request = new BestiarySearchRequest
        {
            Page = 0,
            Size = LoadItemsCount,
            Search = new(query, false),
            Order =
            [
                new("level", "asc"),
                new("name", "asc"),
            ],
        };

        var stringRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);

        var content = new StringContent(stringRequest, mediaType: new MediaTypeHeaderValue(MediaType));

        var response = await _magicItemsV1Client.PostAsync(_apiV1Path + "/spells", content, ct);

        var responseItems = await response.Content.ReadFromJsonAsync<SpellsSearchResponseItem[]>(ct);

        return responseItems ?? [];
    }

    public async Task<SpellsDetailsResponse?> GetDetailsV1Async(string url, CancellationToken ct)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiV1Path + url);

        var response = await _magicItemsV1Client.SendAsync(requestMessage, ct);

        var responseItem = await response.Content.ReadFromJsonAsync<SpellsDetailsResponse>(ct);

        return responseItem;
    }
}