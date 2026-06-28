namespace InitiativeTracker.DataAccess.Dtos;

public record StandCreateDto(
    byte[] ImageData,
    bool InverseTextColor
);
