// Infrastructure/Repositories/UnitOfWork.cs
using System;
using System.Threading.Tasks;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;
using MyWorld.Infrastructure.Data;

namespace MyWorld.Infrastructure.Repositories
{
    public class UnitOfWork(AppDbContext ctx) : IUnitOfWork
    {

        public IUserRepository Users { get; } = new UserRepository(ctx);
        public IRepository<Dimension> Dimensions { get; } = new EfRepository<Dimension>(ctx);
        public IRepository<Question> Questions { get; } = new EfRepository<Question>(ctx);
        public IRepository<AnswerOption> AnswerOptions { get; } = new EfRepository<AnswerOption>(ctx);
        public IRepository<TestSession> TestSessions { get; } = new EfRepository<TestSession>(ctx);
        public IRepository<Response> Responses { get; } = new EfRepository<Response>(ctx);
        public IRepository<Appointment> Appointments { get; } = new EfRepository<Appointment>(ctx);
        public IRepository<Reminder> Reminders { get; } = new EfRepository<Reminder>(ctx);

        public async Task CommitAsync() =>
            await ctx.SaveChangesAsync();

        public void Dispose() =>
            ctx.Dispose();
    }
}
