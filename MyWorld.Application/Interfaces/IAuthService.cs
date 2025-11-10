using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;

namespace MyWorld.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterUserRequest request);
    Task<UserDto?> LoginAsync(LoginUserRequest request);
}
