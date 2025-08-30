namespace MyWorld.Application.DTOs.Responses;

public record DimensionDto(
    Guid Id,
    string Name,
    string? Description
);
