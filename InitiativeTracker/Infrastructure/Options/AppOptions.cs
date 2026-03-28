using System.ComponentModel.DataAnnotations;

namespace InitiativeTracker.Infrastructure.Options;

public sealed class AppOptions
{
    [Required]
    public bool OpenBrowserOnStart { get; set; }
    
    [Required]
    public required string BrowserUrl { get; set; }
}