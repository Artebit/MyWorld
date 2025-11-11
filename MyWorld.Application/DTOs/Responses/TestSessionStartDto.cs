namespace MyWorld.Application.DTOs.Responses;

public record TestSessionStartDto(
    Guid SessionId,
    DateTime StartedAt
);
