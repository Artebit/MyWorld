namespace MyWorld.Application.DTOs.Responses;

public record RecordedAnswerDto(
    Guid Id,
    Guid SessionId,
    Guid QuestionId,
    int? Value,
    string? Text
);
