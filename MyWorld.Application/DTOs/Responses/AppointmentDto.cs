namespace MyWorld.Application.DTOs.Responses;

public record AppointmentDto(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime? EndTime,
    string? Location
);
