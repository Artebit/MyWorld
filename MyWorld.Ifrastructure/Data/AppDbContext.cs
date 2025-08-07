using Microsoft.EntityFrameworkCore;
using MyWorld.Domain.Models;
using MyWorld.Models;

namespace MyWorld.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
    }
}
