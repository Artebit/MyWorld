using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;

namespace MyWorld.Application.Interfaces;

public interface IReminderService
{
    Task<IReadOnlyList<ReminderDto>> GetForUserAsync(Guid userId, bool onlyUpcoming);
    Task<ReminderDto> CreateAsync(Guid userId, CreateReminderRequest request);
    Task<bool> UpdateAsync(Guid userId, UpdateReminderRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid reminderId);
}
