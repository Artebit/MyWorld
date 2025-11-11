using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;

namespace MyWorld.Application.Interfaces;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentDto>> GetForUserAsync(Guid userId);
    Task<AppointmentDto?> GetByIdAsync(Guid userId, Guid appointmentId);
    Task<AppointmentDto> CreateAsync(Guid userId, CreateAppointmentRequest request);
    Task<bool> UpdateAsync(Guid userId, UpdateAppointmentRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid appointmentId);
}
