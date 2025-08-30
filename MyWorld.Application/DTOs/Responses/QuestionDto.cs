using MyWorld.Domain.Models;

namespace MyWorld.Application.DTOs.Responses;

public record QuestionDto(
    Guid Id,
    Guid DimensionId,
    string Text,
    int Order,
    QuestionType Type
);
