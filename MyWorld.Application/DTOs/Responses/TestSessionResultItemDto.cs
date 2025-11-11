namespace MyWorld.Application.DTOs.Responses;

public record TestSessionResultItemDto(
    Guid DimensionId,
    string Dimension,
    double Average
);
