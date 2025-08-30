// Program.cs — .NET 8 Minimal API (MyWorld)

using Microsoft.EntityFrameworkCore;
using MyWorld.Ifrastructure.Data;
using MyWorld.Ifrastructure.Repositories;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Application.Interfaces;
using MyWorld.Application.Services;
using MyWorld.Application.DTOs.Requests;

var builder = WebApplication.CreateBuilder(args);

// Swagger + CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// DbContext (SQL Server LocalDB). Строка в appsettings.json -> ConnectionStrings:Default
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// DI
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// --------- ЭНДПОИНТЫ ДЛЯ ДЕМО ---------
var api = app.MapGroup("/api");

// Dimensions (простые CRUD через UoW)
api.MapGet("/dimensions", (IUnitOfWork uow) =>
{
    var items = uow.Dimensions.GetAll();
    return Results.Ok(items);
});

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

// ---- авто-миграция и сидинг для демо ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

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
        await db.SaveChangesAsync();
    }
}

app.Run();
