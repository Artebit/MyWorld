namespace MyWorld.Application.DTOs.Requests;

public record UpdateAppointmentRequest(
    Guid Id,
    Guid UserId,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime? EndTime,
    string? Location
);
