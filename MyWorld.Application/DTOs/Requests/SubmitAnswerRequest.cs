namespace MyWorld.Application.DTOs.Requests;

public record SubmitAnswerRequest(
    Guid QuestionId,
    int? Value,
    string? Text
);
