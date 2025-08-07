using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using MyWorld.Data;
using MyWorld.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Настроить EF + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2) Настроить Identity
builder.Services.AddDefaultIdentity<IdentityUser>(opts =>
{
    opts.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>();

// 3) Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".MyWorld.Session";
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// 4) Прогон миграций и seed вопросов
await app.MigrateAndSeedAsync();

// 5) HTTP-конвейер
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

//
// 6) JSON API для автосохранения «черновика» ответа
//

// Получить сохранённый черновик
app.MapGet("/api/answers/{questionId:int}", async (
    int questionId,
    ClaimsPrincipal user,
    UserManager<IdentityUser> um,
    AppDbContext db) =>
{
    var u = await um.GetUserAsync(user);
    if (u is null)
        return Results.Unauthorized();

    var ans = await db.Answers
        .Where(a => a.QuestionId == questionId && a.UserId == u.Id)
        .FirstOrDefaultAsync();

    // Если нет — возвращаем пустой ответ
    return Results.Json(new
    {
        questionId,
        response = ans?.Response ?? ""
    });
});

// Сохранить или обновить черновик
app.MapPost("/api/answers/{questionId:int}", async (
    int questionId,
    HttpRequest req,
    ClaimsPrincipal user,
    UserManager<IdentityUser> um,
    AppDbContext db) =>
{
    var u = await um.GetUserAsync(user);
    if (u is null)
        return Results.Unauthorized();

    // читаем JSON { "response": "текст" }
    using var doc = await JsonDocument.ParseAsync(req.Body);
    var text = doc.RootElement.GetProperty("response").GetString() ?? "";

    // находим или создаём новую запись
    var ans = await db.Answers
         .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.UserId == u.Id);

    if (ans is null)
    {
        ans = new Answer
        {
            QuestionId = questionId,
            UserId = u.Id,
            Response = text,
            LastUpdated = DateTime.UtcNow
        };
        db.Answers.Add(ans);
    }
    else
    {
        ans.Response = text;
        ans.LastUpdated = DateTime.UtcNow;
        db.Answers.Update(ans);
    }

    await db.SaveChangesAsync();
    return Results.Ok();
});

//
// 7) Razor Pages
//
app.MapRazorPages();

app.Run();
