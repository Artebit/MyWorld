namespace MyWorld.Domain.Models
{
    public class Reminder
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? RelatedAppointmentId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime RemindAt { get; set; }
        public bool IsSent { get; set; }
    }
}
