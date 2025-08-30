namespace MyWorld.Application.DTOs.Requests;

public record CreateDimensionRequest(
    string Name,
    string? Description
);
