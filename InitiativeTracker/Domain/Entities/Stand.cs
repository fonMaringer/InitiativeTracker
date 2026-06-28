namespace InitiativeTracker.Domain.Entities;

public class Stand
{
    public int Id { get; set; }
    public byte[]? ImageData { get; set; } = [];
    public bool InverseTextColor { get; set; }
}