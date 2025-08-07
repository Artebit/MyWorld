using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyWorld.Models;

namespace MyWorld.Ifrastructure.Data
{
    public static class DbInitializer
    {
        /// <summary>
        /// Прогоняет миграции и seed-ит вопросы из JSON, если таблица пустая.
        /// </summary>
        public static async Task MigrateAndSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // 1) Применить миграции
            await db.Database.MigrateAsync();

            // 2) Seed вопросов, если их ещё нет
            if (!await db.Questions.AnyAsync())
            {
                var path = Path.Combine(env.ContentRootPath, "Data", "questions.json");
                var json = await File.ReadAllTextAsync(path);
                var questions = JsonSerializer.Deserialize<List<Question>>(json,
                                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                 ?? new List<Question>();

                await db.Questions.AddRangeAsync(questions);
                await db.SaveChangesAsync();
            }
        }
    }
}
