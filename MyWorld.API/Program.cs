using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.Interfaces;
using MyWorld.Application.Services;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Interfaces.Repositories;
using MyWorld.Infrastructure;
using MyWorld.Infrastructure.Data;
using MyWorld.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyWorld.API", Version = "v1" });
    options.AddSecurityDefinition("X-UserId", new OpenApiSecurityScheme
    {
        Name = "X-UserId",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Demo user id (GUID) passed via header",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-UserId",
                },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddCors(policy =>
    policy.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? Environment.GetEnvironmentVariable("MYWORLD_CONN")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=MyWorldDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<ITestSessionService, TestSessionService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-UserId", out var raw) && Guid.TryParse(raw, out var userId))
    {
        context.Items["UserId"] = userId;
    }

    await next();
});

Guid? GetUserId(HttpContext context) =>
    context.Items.TryGetValue("UserId", out var value) && value is Guid guid ? guid : null;

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

var api = app.MapGroup("/api");

var auth = api.MapGroup("/auth");

auth.MapPost("/register", async (RegisterUserRequest request, IAuthService service) =>
{
    try
    {
        var user = await service.RegisterAsync(request);
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

auth.MapPost("/login", async (LoginUserRequest request, IAuthService service) =>
{
    try
    {
        var user = await service.LoginAsync(request);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(user);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

api.MapGet("/dimensions", (IUnitOfWork uow) => Results.Ok(uow.Dimensions.GetAll()));

api.MapPost("/dimensions", async (MyWorld.Domain.Models.Dimension dimension, IUnitOfWork uow) =>
{
    if (dimension.Id == Guid.Empty)
    {
        dimension.Id = Guid.NewGuid();
    }

    uow.Dimensions.Add(dimension);
    await uow.CommitAsync();
    return Results.Created($"/api/dimensions/{dimension.Id}", dimension);
});

api.MapPut("/dimensions/{id:guid}", async (Guid id, MyWorld.Domain.Models.Dimension dimension, IUnitOfWork uow) =>
{
    if (id != dimension.Id)
    {
        return Results.BadRequest(new { message = "Ids mismatch." });
    }

    uow.Dimensions.Update(dimension);
    await uow.CommitAsync();
    return Results.NoContent();
});

api.MapDelete("/dimensions/{id:guid}", async (Guid id, IUnitOfWork uow) =>
{
    var entity = uow.Dimensions.GetById(id);
    if (entity is null)
    {
        return Results.NotFound();
    }

    uow.Dimensions.Remove(entity);
    await uow.CommitAsync();
    return Results.NoContent();
});

api.MapGet("/questions", async (IQuestionService service) =>
{
    var list = await service.GetAllAsync();
    return Results.Ok(list);
});

api.MapGet("/questions/{id:guid}", async (Guid id, IQuestionService service) =>
{
    var question = await service.GetByIdAsync(id);
    return question is null ? Results.NotFound() : Results.Ok(question);
});

api.MapPost("/questions", async (CreateQuestionRequest request, IQuestionService service) =>
{
    var created = await service.CreateAsync(request);
    return Results.Created($"/api/questions/{created.Id}", created);
});

api.MapPut("/questions/{id:guid}", async (Guid id, UpdateQuestionRequest request, IQuestionService service) =>
{
    if (id != request.Id)
    {
        return Results.BadRequest(new { message = "Ids mismatch." });
    }

    await service.UpdateAsync(request);
    return Results.NoContent();
});

api.MapDelete("/questions/{id:guid}", async (Guid id, IQuestionService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
});

var sessions = api.MapGroup("/sessions");

sessions.MapPost("/start", async (Guid userId, ITestSessionService service) =>
{
    if (userId == Guid.Empty)
    {
        return Results.BadRequest(new { message = "UserId is required." });
    }

    var start = await service.StartSessionAsync(userId);
    return Results.Ok(start);
});

sessions.MapPost("/{sessionId:guid}/answers", async (Guid sessionId, SubmitAnswerRequest request, ITestSessionService service) =>
{
    try
    {
        var recorded = await service.SubmitAnswerAsync(sessionId, request);
        if (recorded is null)
        {
            return Results.NotFound();
        }

        return Results.Created($"/api/sessions/{sessionId}/answers/{recorded.Id}", recorded);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

sessions.MapPost("/{sessionId:guid}/complete", async (Guid sessionId, ITestSessionService service) =>
{
    try
    {
        var completion = await service.CompleteSessionAsync(sessionId);
        return completion is null ? Results.NotFound() : Results.Ok(completion);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

sessions.MapGet("/{sessionId:guid}/result", async (Guid sessionId, ITestSessionService service) =>
{
    try
    {
        var result = await service.GetResultAsync(sessionId);
        return Results.Ok(result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

var appointments = api.MapGroup("/appointments");

appointments.MapGet("/", async (HttpContext context, IAppointmentService service) =>
{
    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var items = await service.GetForUserAsync(userId.Value);
    return Results.Ok(items);
});

appointments.MapGet("/{id:guid}", async (HttpContext context, Guid id, IAppointmentService service) =>
{
    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    var appointment = await service.GetByIdAsync(userId.Value, id);
    return appointment is null ? Results.NotFound() : Results.Ok(appointment);
});

appointments.MapPost("/", async (HttpContext context, CreateAppointmentRequest request, IAppointmentService service) =>
{
    try
    {
        var userId = GetUserId(context) ?? (request.UserId == Guid.Empty ? null : request.UserId);
        if (userId is null)
        {
            return Results.BadRequest(new { message = "UserId is required (header X-UserId or body)." });
        }

        var created = await service.CreateAsync(userId.Value, request);
        return Results.Created($"/api/appointments/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

appointments.MapPut("/{id:guid}", async (HttpContext context, Guid id, UpdateAppointmentRequest request, IAppointmentService service) =>
{
    if (id != request.Id)
    {
        return Results.BadRequest(new { message = "Ids mismatch." });
    }

    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    try
    {
        var updated = await service.UpdateAsync(userId.Value, request);
        return updated ? Results.NoContent() : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

appointments.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IAppointmentService service) =>
{
    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    try
    {
        var deleted = await service.DeleteAsync(userId.Value, id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

var reminders = api.MapGroup("/reminders");

reminders.MapGet("/", async (HttpContext context, bool onlyUpcoming, IReminderService service) =>
{
    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    try
    {
        var items = await service.GetForUserAsync(userId.Value, onlyUpcoming);
        return Results.Ok(items);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

reminders.MapPost("/", async (HttpContext context, CreateReminderRequest request, IReminderService service) =>
{
    try
    {
        var userId = GetUserId(context) ?? (request.UserId == Guid.Empty ? null : request.UserId);
        if (userId is null)
        {
            return Results.BadRequest(new { message = "UserId is required (header X-UserId or body)." });
        }

        var created = await service.CreateAsync(userId.Value, request);
        return Results.Created($"/api/reminders/{created.Id}", created);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

reminders.MapPut("/{id:guid}", async (HttpContext context, Guid id, UpdateReminderRequest request, IReminderService service) =>
{
    if (id != request.Id)
    {
        return Results.BadRequest(new { message = "Ids mismatch." });
    }

    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    try
    {
        var updated = await service.UpdateAsync(userId.Value, request);
        return updated ? Results.NoContent() : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

reminders.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IReminderService service) =>
{
    var userId = GetUserId(context);
    if (userId is null)
    {
        return Results.Unauthorized();
    }

    try
    {
        var deleted = await service.DeleteAsync(userId.Value, id);
        return deleted ? Results.NoContent() : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var dimensionSeeded = false;

    if (!db.Dimensions.Any())
    {
        var names = new[] { "Здоровье", "Карьера", "Финансы", "Отношения", "Личностный рост", "Досуг", "Дом", "Духовность" };
        var dimensions = names.Select(name => new MyWorld.Domain.Models.Dimension
        {
            Id = Guid.NewGuid(),
            Name = name,
        }).ToList();

        db.Dimensions.AddRange(dimensions);

        db.Questions.AddRange(dimensions.Select((dimension, index) => new MyWorld.Domain.Models.Question
        {
            Id = Guid.NewGuid(),
            DimensionId = dimension.Id,
            Text = $"Оцените удовлетворённость сферой «{dimension.Name}» по шкале 1..10",
            Order = index + 1,
            Type = MyWorld.Domain.Models.QuestionType.Scale,
        }));

        dimensionSeeded = true;
    }

    if (dimensionSeeded)
    {
        await db.SaveChangesAsync();
    }

    if (!db.Users.Any())
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.RegisterAsync(new RegisterUserRequest("demo@user.dev", "12345", "Demo", "User"));
    }
}

app.Run();
