using Microsoft.EntityFrameworkCore;
using MyWorld.Domain.Models;

namespace MyWorld.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Dimension> Dimensions => Set<Dimension>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<Response> Responses => Set<Response>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
}
