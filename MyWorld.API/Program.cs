// MyWorld.API/Program.cs  — .NET 8 Minimal API
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MyWorld.Ifrastructure.Data;
using MyWorld.Ifrastructure.Repositories;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Application.Interfaces;
using MyWorld.Application.Services;
using MyWorld.Application.DTOs.Requests;
using System.Linq;
using System.Security.Cryptography;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Swagger + CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Док (если у тебя не задан — можно опустить)
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyWorld.API", Version = "v1" });

    //  объявляем, что у нас есть ключ в заголовке X-UserId
    c.AddSecurityDefinition("X-UserId", new OpenApiSecurityScheme
    {
        Name = "X-UserId",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Demo user id (GUID) passed via header"
    });

    //  требуем эту схему для всех операций (чтобы кнопка Authorize работала глобально)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-UserId"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ---- Connection string resolve (3 источника: appsettings -> env -> дефолт) ----
var conn = builder.Configuration.GetConnectionString("Defau" +
    "lt");
if (string.IsNullOrWhiteSpace(conn))
    conn = Environment.GetEnvironmentVariable("MYWORLD_CONN");
if (string.IsNullOrWhiteSpace(conn))
    conn = "Server=(localdb)\\MSSQLLocalDB;Database=MyWorldDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

// Небольшая диагностика — выведем куда коннектимся (без пароля)
Console.WriteLine($"[DB] Using connection: {conn}");

// DbContext
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();
app.Use(async (ctx, next) =>
{
    // Берём userId из заголовка, если задан
    if (ctx.Request.Headers.TryGetValue("X-UserId", out var raw)
        && Guid.TryParse(raw, out var uid))
    {
        ctx.Items["UserId"] = uid;
    }
    await next();
});

// Хелпер: получить userId из контекста
Guid? GetUserId(HttpContext ctx)
    => ctx.Items.TryGetValue("UserId", out var val) && val is Guid g ? g : null;


app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// --------- ЭНДПОИНТЫ ДЛЯ ДЕМО ---------
var api = app.MapGroup("/api");

// Auth
var auth = api.MapGroup("/auth");

auth.MapPost("/register", async (RegisterUserRequest req, IAuthService svc) =>
{
    try
    {
        var user = await svc.RegisterAsync(req);
        return Results.Created($"/api/users/{user.Id}", user);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
});

auth.MapPost("/login", async (LoginUserRequest req, IAuthService svc) =>
{
    try
    {
        var user = await svc.LoginAsync(req);
        if (user is null) return Results.Unauthorized();
        return Results.Ok(user);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

// Dimensions (простые CRUD через UoW)
api.MapGet("/dimensions", (IUnitOfWork uow) => Results.Ok(uow.Dimensions.GetAll()));

api.MapPost("/dimensions", async (MyWorld.Domain.Models.Dimension d, IUnitOfWork uow) =>
{
    if (d.Id == Guid.Empty) d.Id = Guid.NewGuid();
    uow.Dimensions.Add(d);
    await uow.CommitAsync();
    return Results.Created($"/api/dimensions/{d.Id}", d);
});

api.MapPut("/dimensions/{id:guid}", async (Guid id, MyWorld.Domain.Models.Dimension d, IUnitOfWork uow) =>
{
    if (id != d.Id) return Results.BadRequest("Ids mismatch");
    uow.Dimensions.Update(d);
    await uow.CommitAsync();
    return Results.NoContent();
});

api.MapDelete("/dimensions/{id:guid}", async (Guid id, IUnitOfWork uow) =>
{
    var e = uow.Dimensions.GetById(id);
    if (e is null) return Results.NotFound();
    uow.Dimensions.Remove(e);
    await uow.CommitAsync();
    return Results.NoContent();
});

// Questions (через Application-сервис)
api.MapGet("/questions", async (IQuestionService svc) =>
{
    var list = await svc.GetAllAsync();
    return Results.Ok(list);
});

api.MapGet("/questions/{id:guid}", async (Guid id, IQuestionService svc) =>
{
    var q = await svc.GetByIdAsync(id);
    return q is null ? Results.NotFound() : Results.Ok(q);
});

api.MapPost("/questions", async (CreateQuestionRequest req, IQuestionService svc) =>
{
    var created = await svc.CreateAsync(req);
    return Results.Created($"/api/questions/{created.Id}", created);
});

api.MapPut("/questions/{id:guid}", async (Guid id, MyWorld.Application.DTOs.Requests.UpdateQuestionRequest req, IQuestionService svc) =>
{
    if (id != req.Id) return Results.BadRequest("Ids mismatch");
    await svc.UpdateAsync(req);
    return Results.NoContent();
});

api.MapDelete("/questions/{id:guid}", async (Guid id, IQuestionService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});
// ===== Test Sessions & Responses =====

var sessions = api.MapGroup("/sessions");

// 1) Старт сессии (создаёт TestSession)
sessions.MapPost("/start", async (Guid userId, IUnitOfWork uow) =>
{
    var s = new MyWorld.Domain.Models.TestSession
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        StartedAt = DateTime.UtcNow
    };
    uow.TestSessions.Add(s);
    await uow.CommitAsync();
    return Results.Ok(new { sessionId = s.Id, s.StartedAt });
});

// 2) Сохранить ответ на вопрос (числовой или текстовый)
sessions.MapPost("/{sessionId:guid}/answers", async (Guid sessionId, SubmitAnswerRequest req, IUnitOfWork uow) =>
{
    if (req.Value is null && string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest("Either Value or Text must be provided");

    var resp = new MyWorld.Domain.Models.Response
    {
        Id = Guid.NewGuid(),
        SessionId = sessionId,
        QuestionId = req.QuestionId,
        AnswerValue = req.Value,
        AnswerText = req.Text
    };
    uow.Responses.Add(resp);
    await uow.CommitAsync();
    return Results.Created($"/api/sessions/{sessionId}/answers/{resp.Id}", resp);
});

// 3) Завершить сессию
sessions.MapPost("/{sessionId:guid}/complete", async (Guid sessionId, IUnitOfWork uow) =>
{
    var s = uow.TestSessions.GetById(sessionId);
    if (s is null) return Results.NotFound();
    if (s.CompletedAt is null)
    {
        s.CompletedAt = DateTime.UtcNow;
        uow.TestSessions.Update(s);
        await uow.CommitAsync();
    }
    return Results.Ok(new { s.Id, s.CompletedAt });
});

// 4) Итог: средний балл по каждой сфере (для диаграммы «колеса»)
sessions.MapGet("/{sessionId:guid}/result", (Guid sessionId, IUnitOfWork uow) =>
{
    var responses = uow.Responses.GetAll()
        .Where(r => r.SessionId == sessionId && r.AnswerValue.HasValue)
        .ToList();

    var questions = uow.Questions.GetAll().ToDictionary(q => q.Id, q => q.DimensionId);
    var dims = uow.Dimensions.GetAll().ToDictionary(d => d.Id, d => d.Name);

    var result = responses
        .GroupBy(r => questions.TryGetValue(r.QuestionId, out var did) ? did : Guid.Empty)
        .Where(g => g.Key != Guid.Empty)
        .Select(g => new
        {
            dimensionId = g.Key,
            dimension = dims.TryGetValue(g.Key, out var name) ? name : g.Key.ToString(),
            average = Math.Round(g.Average(x => (double)x.AnswerValue!.Value), 2)
        });

    return Results.Ok(result);
});

// ===== Appointments (встречи) =====
var appointments = api.MapGroup("/appointments");

// GET /api/appointments
appointments.MapGet("/", (HttpContext ctx, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var list = uow.Appointments.GetAll()
        .Where(a => a.UserId == uid)
        .OrderBy(a => a.StartTime)
        .ToList();

    return Results.Ok(list);
});

// GET /api/appointments/{id}
appointments.MapGet("/{id:guid}", (HttpContext ctx, Guid id, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var a = uow.Appointments.GetById(id);
    if (a is null || a.UserId != uid) return Results.NotFound();

    return Results.Ok(a);
});

// POST /api/appointments
appointments.MapPost("/", async (HttpContext ctx, MyWorld.Application.DTOs.Requests.CreateAppointmentRequest req, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx) ?? (req.UserId == Guid.Empty ? null : req.UserId);
    if (uid is null) return Results.BadRequest("UserId is required (header X-UserId or body).");
    if (req.EndTime <= req.StartTime) return Results.BadRequest("EndTime must be after StartTime.");

    var a = new MyWorld.Domain.Models.Appointment
    {
        Id = Guid.NewGuid(),
        UserId = uid.Value,
        Title = req.Title,
        Description = req.Description,
        StartTime = req.StartTime,
        EndTime = req.EndTime,
        Location = req.Location
    };

    uow.Appointments.Add(a);
    await uow.CommitAsync();
    return Results.Created($"/api/appointments/{a.Id}", a);
});

// PUT /api/appointments/{id}
appointments.MapPut("/{id:guid}", async (HttpContext ctx, Guid id, MyWorld.Application.DTOs.Requests.UpdateAppointmentRequest req, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var a = uow.Appointments.GetById(id);
    if (a is null || a.UserId != uid) return Results.NotFound();
    if (req.EndTime <= req.StartTime) return Results.BadRequest("EndTime must be after StartTime.");

    a.Title = req.Title;
    a.Description = req.Description;
    a.StartTime = req.StartTime;
    a.EndTime = req.EndTime;
    a.Location = req.Location;

    uow.Appointments.Update(a);
    await uow.CommitAsync();
    return Results.NoContent();
});

// DELETE /api/appointments/{id}
appointments.MapDelete("/{id:guid}", async (HttpContext ctx, Guid id, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var a = uow.Appointments.GetById(id);
    if (a is null || a.UserId != uid) return Results.NotFound();

    uow.Appointments.Remove(a);
    await uow.CommitAsync();
    return Results.NoContent();
});

// ===== Reminders (напоминания) =====
var reminders = api.MapGroup("/reminders");

// GET /api/reminders?onlyUpcoming=true
reminders.MapGet("/", (HttpContext ctx, bool onlyUpcoming, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var q = uow.Reminders.GetAll().Where(r => r.UserId == uid);
    if (onlyUpcoming)
        q = q.Where(r => r.RemindAt >= DateTime.UtcNow && !r.IsSent);

    return Results.Ok(q.OrderBy(r => r.RemindAt).ToList());
});

// POST /api/reminders
reminders.MapPost("/", async (HttpContext ctx, MyWorld.Application.DTOs.Requests.CreateReminderRequest req, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx) ?? (req.UserId == Guid.Empty ? null : req.UserId);
    if (uid is null) return Results.BadRequest("UserId is required (header X-UserId or body).");

    var r = new MyWorld.Domain.Models.Reminder
    {
        Id = Guid.NewGuid(),
        UserId = uid.Value,
        RelatedAppointmentId = req.RelatedAppointmentId,
        Message = req.Message,
        RemindAt = req.RemindAt,
        IsSent = false
    };

    uow.Reminders.Add(r);
    await uow.CommitAsync();
    return Results.Created($"/api/reminders/{r.Id}", r);
});

// PUT /api/reminders/{id}
reminders.MapPut("/{id:guid}", async (HttpContext ctx, Guid id, MyWorld.Application.DTOs.Requests.UpdateReminderRequest req, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var r = uow.Reminders.GetById(id);
    if (r is null || r.UserId != uid) return Results.NotFound();

    r.Message = req.Message;
    r.RemindAt = req.RemindAt;

    uow.Reminders.Update(r);
    await uow.CommitAsync();
    return Results.NoContent();
});

// DELETE /api/reminders/{id}
reminders.MapDelete("/{id:guid}", async (HttpContext ctx, Guid id, IUnitOfWork uow) =>
{
    var uid = GetUserId(ctx);
    if (uid is null) return Results.Unauthorized();

    var r = uow.Reminders.GetById(id);
    if (r is null || r.UserId != uid) return Results.NotFound();

    uow.Reminders.Remove(r);
    await uow.CommitAsync();
    return Results.NoContent();
});


// ---- авто-миграция и сидинг для демо ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    static string HashSeedPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        var payload = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, payload, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, payload, salt.Length, hash.Length);
        return Convert.ToBase64String(payload);
    }

    var hasChanges = false;

    if (!db.Dimensions.Any())
    {
        var names = new[] { "Здоровье", "Карьера", "Финансы", "Отношения", "Личностный рост", "Досуг", "Дом", "Духовность" };
        var dims = names.Select(n => new MyWorld.Domain.Models.Dimension { Id = Guid.NewGuid(), Name = n }).ToList();
        db.Dimensions.AddRange(dims);
        db.Questions.AddRange(dims.Select((d, i) => new MyWorld.Domain.Models.Question
        {
            Id = Guid.NewGuid(),
            DimensionId = d.Id,
            Text = $"Оцените удовлетворённость сферой «{d.Name}» по шкале 1..10",
            Order = i + 1,
            Type = MyWorld.Domain.Models.QuestionType.Scale
        }));
        hasChanges = true;
    }

    if (!db.Users.Any())
    {
        var demoUser = new MyWorld.Domain.Models.User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "demo@user.dev",
            PasswordHash = HashSeedPassword("12345"),
            FirstName = "Demo",
            LastName = "User",
            RegisteredAt = DateTime.UtcNow,
            LastLoginAt = null
        };
        db.Users.Add(demoUser);
        hasChanges = true;
    }

    if (hasChanges)
    {
        await db.SaveChangesAsync();
    }
}

app.Run();

public record SubmitAnswerRequest(Guid QuestionId, int? Value, string? Text);
