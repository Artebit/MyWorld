using MyWorld.Domain.Models;
using System.Data;

namespace MyWorld.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        public List<Reminder> Reminders { get; set; } = new List<Reminder>();
        public List<TestSession> TestSessions { get; set; } = new List<TestSession>();
        public List<Question> Questions { get; set; } = [];
    }



}
