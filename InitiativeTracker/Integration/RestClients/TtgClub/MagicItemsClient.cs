using System.Net.Http.Headers;
using System.Text.Json;
using InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;
using Microsoft.Extensions.Options;

namespace InitiativeTracker.Integration.RestClients.TtgClub;

public interface IMagicItemsClient
{
    Task<MagicItemsSearchResponseItem[]> SearchV1Async(string query, CancellationToken ct);

    Task<MagicItemsDetailsResponse?> GetDetailsV1Async(string url, CancellationToken ct);

    string BuildDirectLink(string url);
}

public class MagicItemsClient(IOptions<TtgClubClientOptions> options) : IMagicItemsClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private const string MediaType = "application/json";
    private readonly string _apiV1Path = options.Value.ApiV1Path;

    private readonly HttpClient _magicItemsV1Client = new()
    {
        BaseAddress = new Uri(options.Value.Host),
        DefaultRequestHeaders =
        {
            { "Accept", MediaType },
        },
    };

    public string BuildDirectLink(string url) => options.Value.Host + url;

    public async Task<MagicItemsSearchResponseItem[]> SearchV1Async(string query, CancellationToken ct)
    {
        var request = new BestiarySearchRequest
        {
            Page = 0,
            Size = 10,
            Search = new(query, false),
            Order =
            [
                new("rarity", "asc"),
                new("name", "asc"),
            ],
        };

        var stringRequest = JsonSerializer.Serialize(request, _jsonSerializerOptions);

        var content = new StringContent(stringRequest, mediaType: new MediaTypeHeaderValue(MediaType));

        var response = await _magicItemsV1Client.PostAsync(_apiV1Path + "/items/magic", content, ct);

        var responseItems = await response.Content.ReadFromJsonAsync<MagicItemsSearchResponseItem[]>(ct);

        return responseItems ?? [];
    }

    public async Task<MagicItemsDetailsResponse?> GetDetailsV1Async(string url, CancellationToken ct)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, _apiV1Path + url);

        var response = await _magicItemsV1Client.SendAsync(requestMessage, ct);

        var responseItem = await response.Content.ReadFromJsonAsync<MagicItemsDetailsResponse>(ct);

        return responseItem;
    }
}