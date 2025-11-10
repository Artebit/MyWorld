namespace MyWorld.Application.DTOs.Requests;

public record LoginUserRequest(
    string Email,
    string Password
);
