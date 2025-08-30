namespace MyWorld.Application.DTOs.Requests;

public record UpdateDimensionRequest(
    Guid Id,
    string Name,
    string? Description
);
