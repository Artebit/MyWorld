using MyWorld.Domain.Models;

namespace MyWorld.Application.DTOs.Requests;

public record UpdateQuestionRequest(
    Guid Id,
    Guid DimensionId,
    string Text,
    int Order,
    QuestionType Type
);
