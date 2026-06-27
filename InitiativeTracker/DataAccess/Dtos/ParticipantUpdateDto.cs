namespace InitiativeTracker.DataAccess.Dtos;

public record ParticipantUpdateDto(
    int Id,
    string Name,
    int Hp,
    int Ac,
    int Dexterity
);