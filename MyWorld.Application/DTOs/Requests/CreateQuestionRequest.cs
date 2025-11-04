using MyWorld.Domain.Models;

namespace MyWorld.Application.DTOs.Requests;

public record CreateQuestionRequest(
    Guid DimensionId,
    string Text,
    int Order,
    QuestionType Type
);
