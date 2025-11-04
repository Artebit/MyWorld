using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWorld.Data;
using MyWorld.Models;

namespace MyWorld.Pages
{
    public class TestModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _um;

        public TestModel(AppDbContext db, UserManager<IdentityUser> um)
        {
            _db = db;
            _um = um;
        }

        public Question? Question { get; set; }
        public int TotalQuestions { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty]
        public Answer UserAnswer { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Сколько всего вопросов
            TotalQuestions = await _db.Questions.CountAsync();

            // Если номер вне диапазона — на страницу результатов
            if (PageNumber < 1 || PageNumber > TotalQuestions)
                return RedirectToPage("/TestResult");

            // Загружаем нужный вопрос
            Question = await _db.Questions.FindAsync(PageNumber);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Повторно узнаём количество, чтобы знать, куда дальше
            TotalQuestions = await _db.Questions.CountAsync();

            // Получаем залогиненного пользователя
            var user = await _um.GetUserAsync(User);
            if (user == null)
                return Challenge(); // или RedirectToPage("/Identity/Account/Login");

            // Сохраняем ответ
            UserAnswer.UserId = user.Id;
            UserAnswer.QuestionId = PageNumber;
            await _db.Answers.AddAsync(UserAnswer);
            await _db.SaveChangesAsync();

            // Если есть следующий вопрос — переходим на него, иначе — к результатам
            if (PageNumber < TotalQuestions)
                return RedirectToPage("/Test", new { pageNumber = PageNumber + 1 });
            else
                return RedirectToPage("/TestResult");
        }
    }
}
