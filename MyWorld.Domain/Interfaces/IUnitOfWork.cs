using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;

namespace MyWorld.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRepository<Dimension> Dimensions { get; }
    IRepository<Question> Questions { get; }
    IRepository<AnswerOption> AnswerOptions { get; }
    IRepository<TestSession> TestSessions { get; }
    IRepository<Response> Responses { get; }
    IRepository<Appointment> Appointments { get; }
    IRepository<Reminder> Reminders { get; }
    Task CommitAsync();
}
