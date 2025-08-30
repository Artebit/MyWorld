namespace MyWorld.Application.DTOs.Responses;

public record ReminderDto(
    Guid Id,
    Guid UserId,
    Guid? RelatedAppointmentId,
    string Message,
    DateTime RemindAt,
    bool IsSent
);
