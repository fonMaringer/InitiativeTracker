using System.ComponentModel.DataAnnotations;

namespace InitiativeTracker.Integration.RestClients.TtgClub;

public sealed class TtgClubClientOptions
{
    [Required]
    public string Host { get; set; } = null!;
    
    [Required]
    public string ApiV1Path { get; set; } = null!;
}