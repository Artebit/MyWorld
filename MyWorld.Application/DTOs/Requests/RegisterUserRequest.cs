namespace MyWorld.Application.DTOs.Requests;

public record RegisterUserRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName
);
