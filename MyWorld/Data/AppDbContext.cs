using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWorld.Models;

namespace MyWorld.Data
{
    // Наследуемся от IdentityDbContext<IdentityUser>,
    // чтобы у нас сразу шёл контекст для пользователей
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Вот это — не объявление класса, а конструктор:
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Эти два свойства говорят EF Core:
        // «У меня есть сущность Question, сохраняй её в таблицу Questions»
        public DbSet<Question> Questions { get; set; }

        // И так же д   ля Answer → таблица Answers
        public DbSet<Answer> Answers { get; set; }

        public DbSet<ExerciseSession> ExerciseSessions { get; set; }

    }
}
