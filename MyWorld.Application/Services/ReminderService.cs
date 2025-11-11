using System.Linq;
using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IUnitOfWork _uow;

    public ReminderService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public Task<IReadOnlyList<ReminderDto>> GetForUserAsync(Guid userId, bool onlyUpcoming)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var query = _uow.Reminders
            .GetAll()
            .Where(r => r.UserId == userId);

        if (onlyUpcoming)
        {
            query = query.Where(r => r.RemindAt >= DateTime.UtcNow && !r.IsSent);
        }

        var items = query
            .OrderBy(r => r.RemindAt)
            .Select(Map)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<ReminderDto>>(items);
    }

    public async Task<ReminderDto> CreateAsync(Guid userId, CreateReminderRequest request)
    {
        var ownerId = ResolveUserId(userId, request.UserId);

        var entity = new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            RelatedAppointmentId = request.RelatedAppointmentId,
            Message = request.Message,
            RemindAt = request.RemindAt,
            IsSent = false,
        };

        _uow.Reminders.Add(entity);
        await _uow.CommitAsync();

        return Map(entity);
    }

    public async Task<bool> UpdateAsync(Guid userId, UpdateReminderRequest request)
    {
        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException("Reminder identifier is required.", nameof(request));
        }

        var ownerId = ResolveUserId(userId, request.UserId);

        var entity = _uow.Reminders.GetById(request.Id);
        if (entity is null || entity.UserId != ownerId)
        {
            return false;
        }

        entity.RelatedAppointmentId = request.RelatedAppointmentId;
        entity.Message = request.Message;
        entity.RemindAt = request.RemindAt;
        entity.IsSent = request.IsSent;

        _uow.Reminders.Update(entity);
        await _uow.CommitAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid reminderId)
    {
        if (reminderId == Guid.Empty)
        {
            throw new ArgumentException("Reminder identifier is required.", nameof(reminderId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        var entity = _uow.Reminders.GetById(reminderId);
        if (entity is null || entity.UserId != userId)
        {
            return false;
        }

        _uow.Reminders.Remove(entity);
        await _uow.CommitAsync();
        return true;
    }

    private static ReminderDto Map(Reminder entity) => new(
        entity.Id,
        entity.UserId,
        entity.RelatedAppointmentId,
        entity.Message,
        entity.RemindAt,
        entity.IsSent);

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
}
