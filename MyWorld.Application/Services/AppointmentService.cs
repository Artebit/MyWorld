using System.Linq;
using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _uow;

    public AppointmentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task<IReadOnlyList<AppointmentDto>> GetForUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var items = _uow.Appointments
            .GetAll()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.StartTime)
            .Select(Map)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<AppointmentDto>>(items);
    }

    public Task<AppointmentDto?> GetByIdAsync(Guid userId, Guid appointmentId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment identifier is required.", nameof(appointmentId));
        }

        var entity = _uow.Appointments.GetById(appointmentId);
        if (entity is null || entity.UserId != userId)
        {
            return Task.FromResult<AppointmentDto?>(null);
        }

        return Task.FromResult<AppointmentDto?>(Map(entity));
    }

    public async Task<AppointmentDto> CreateAsync(Guid userId, CreateAppointmentRequest request)
    {
        var ownerId = ResolveUserId(userId, request.UserId);
        ValidateSchedule(request.StartTime, request.EndTime);

        var entity = new Appointment
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Title = request.Title,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location,
        };

        _uow.Appointments.Add(entity);
        await _uow.CommitAsync();
        return Map(entity);
    }

    public async Task<bool> UpdateAsync(Guid userId, UpdateAppointmentRequest request)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Appointment identifier is required.", nameof(request));
        }

        var ownerId = ResolveUserId(userId, request.UserId);
        ValidateSchedule(request.StartTime, request.EndTime);

        var entity = _uow.Appointments.GetById(request.Id);
        if (entity is null || entity.UserId != ownerId)
        {
            return false;
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.Location = request.Location;

        _uow.Appointments.Update(entity);
        await _uow.CommitAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid appointmentId)
    {
        if (appointmentId == Guid.Empty)
        {
            throw new ArgumentException("Appointment identifier is required.", nameof(appointmentId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var entity = _uow.Appointments.GetById(appointmentId);
        if (entity is null || entity.UserId != userId)
        {
            return false;
        }

        _uow.Appointments.Remove(entity);
        await _uow.CommitAsync();
        return true;
    }

    private static AppointmentDto Map(Appointment entity) => new(
        entity.Id,
        entity.UserId,
        entity.Title,
        entity.Description,
        entity.StartTime,
        entity.EndTime,
        entity.Location);

    private static Guid ResolveUserId(Guid headerUserId, Guid requestUserId)
    {
        var effective = headerUserId != Guid.Empty ? headerUserId : requestUserId;
        if (effective == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.");
        }

        if (headerUserId != Guid.Empty && requestUserId != Guid.Empty && headerUserId != requestUserId)
        {
            throw new ArgumentException("Conflicting user identifiers provided.");
        }

        return effective;
    }

    private static void ValidateSchedule(DateTime start, DateTime? end)
    {
        if (end.HasValue && end <= start)
        {
            throw new ArgumentException("End time must be after the start time.");
        }
    }
}
