namespace MyWorld.Application.DTOs.Requests;

public record CreateAppointmentRequest(
    Guid UserId,
    string Title,
    string? Description,
    DateTime StartTime,
    DateTime? EndTime,
    string? Location
);
