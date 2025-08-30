namespace MyWorld.Application.DTOs.Requests;

public record CreateReminderRequest(
    Guid UserId,
    Guid? RelatedAppointmentId,
    string Message,
    DateTime RemindAt
);
