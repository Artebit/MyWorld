namespace MyWorld.Application.DTOs.Responses;

public record TestSessionCompletionDto(
    Guid SessionId,
    DateTime? CompletedAt
);
