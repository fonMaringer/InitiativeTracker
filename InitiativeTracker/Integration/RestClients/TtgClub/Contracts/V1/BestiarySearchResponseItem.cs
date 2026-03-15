using System.Text.Json.Serialization;

namespace InitiativeTracker.Integration.RestClients.TtgClub.Contracts.V1;

public class BestiarySearchResponseItem
{
    public Name Name { get; set; }
    public string Type { get; set; }
    public string ChallengeRating { get; set; }
    public string Url { get; set; }
    public Source Source { get; set; }
    public Group Group { get; set; }
    
    [JsonIgnore]
    public int AddCount { get; set; } = 1;
}