using System;
using System.Security.Cryptography;
using System.Text;
using MyWorld.Application.DTOs.Requests;
using MyWorld.Application.DTOs.Responses;
using MyWorld.Application.Interfaces;
using MyWorld.Domain.Interfaces;
using MyWorld.Domain.Models;

namespace MyWorld.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;

    public AuthService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<UserDto> RegisterAsync(RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (_uow.Users.GetByEmail(normalizedEmail) is not null)
            throw new InvalidOperationException("User with this email already exists");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = HashPassword(request.Password),
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            LastName = request.LastName?.Trim() ?? string.Empty,
            RegisteredAt = now,
            LastLoginAt = null,
        };

        _uow.Users.Add(user);
        await _uow.CommitAsync();

        return ToDto(user);
    }

    public async Task<UserDto?> LoginAsync(LoginUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required", nameof(request));

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = _uow.Users.GetByEmail(normalizedEmail);
        if (user is null)
            return null;

        if (!VerifyPassword(request.Password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.CommitAsync();

        return ToDto(user);
    }

    private static UserDto ToDto(User user) => new(
        user.Id,
        user.Email,
        string.IsNullOrWhiteSpace(user.FirstName) ? null : user.FirstName,
        string.IsNullOrWhiteSpace(user.LastName) ? null : user.LastName,
        user.RegisteredAt,
        user.LastLoginAt
    );

    private static string HashPassword(string password)
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

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(storedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var salt = payload.AsSpan(0, 16).ToArray();
        var stored = payload.AsSpan(16).ToArray();
        var computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            stored.Length);

        return CryptographicOperations.FixedTimeEquals(stored, computed);
    }
}
