namespace MyWorld.Application.DTOs.Requests;

public record UpdateReminderRequest(
    Guid Id,
    Guid UserId,
    Guid? RelatedAppointmentId,
    string Message,
    DateTime RemindAt,
    bool IsSent
);
