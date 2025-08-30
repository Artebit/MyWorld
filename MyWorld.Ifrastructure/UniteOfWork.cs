using System;
using System.Threading.Tasks;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Domain.Models;
using MyWorld.Ifrastructure.Data;           // ВАЖНО
using MyWorld.Ifrastructure.Repositories;   // этот же неймспейс, если файл рядом

namespace MyWorld.Ifrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;

        public IUserRepository Users { get; }
        public IRepository<Dimension> Dimensions { get; }
        public IRepository<Question> Questions { get; }
        public IRepository<AnswerOption> AnswerOptions { get; }
        public IRepository<TestSession> TestSessions { get; }
        public IRepository<Response> Responses { get; }
        public IRepository<Appointment> Appointments { get; }
        public IRepository<Reminder> Reminders { get; }

        public UnitOfWork(AppDbContext ctx)
        {
            _ctx = ctx;

            Users = new UserRepository(ctx);
            Dimensions = new EfRepository<Dimension>(ctx);
            Questions = new EfRepository<Question>(ctx);
            AnswerOptions = new EfRepository<AnswerOption>(ctx);
            TestSessions = new EfRepository<TestSession>(ctx);
            Responses = new EfRepository<Response>(ctx);
            Appointments = new EfRepository<Appointment>(ctx);
            Reminders = new EfRepository<Reminder>(ctx);
        }

        public Task CommitAsync() => _ctx.SaveChangesAsync();

        public void Dispose() => _ctx.Dispose();
    }
}
