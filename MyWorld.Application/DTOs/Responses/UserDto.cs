using System;

namespace MyWorld.Application.DTOs.Responses;

public record UserDto(
    Guid Id,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime RegisteredAt,
    DateTime? LastLoginAt
);
