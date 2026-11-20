using MediTrack.Web.Models;
using MediTrack.Web.Repositories;
using Microsoft.AspNetCore.Identity;

namespace MediTrack.Web.Services;

/// <summary>
/// Login business rules. Kept out of the controller so it can be unit-tested:
///  - email is case-insensitive
///  - 5 failed attempts lock the account for 15 minutes
///  - inactive accounts cannot sign in
/// </summary>
public class AuthService : IAuthService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(IUserRepository users, IPasswordHasher<User> hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public async Task<LoginResult> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _users.GetByEmailAsync(NormalizeEmail(email));
        if (user is null)
            return new LoginResult(LoginStatus.InvalidCredentials);

        var now = DateTime.UtcNow;

        if (!user.IsActive)
            return new LoginResult(LoginStatus.Inactive);

        if (user.IsLockedOut(now))
            return new LoginResult(LoginStatus.LockedOut, LockoutEndUtc: user.LockoutEnd);

        var verification = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedAttempts)
            {
                user.LockoutEnd = now.Add(LockoutDuration);
                user.FailedLoginCount = 0;
            }
            await _users.UpdateAsync(user);

            return user.IsLockedOut(now)
                ? new LoginResult(LoginStatus.LockedOut, LockoutEndUtc: user.LockoutEnd)
                : new LoginResult(LoginStatus.InvalidCredentials);
        }

        // Upgrade the hash if the hashing algorithm settings changed.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = _hasher.HashPassword(user, password);

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = now;
        await _users.UpdateAsync(user);

        return new LoginResult(LoginStatus.Success, user);
    }

    public async Task<User> CreateUserAsync(string name, string email, string password, Role role)
    {
        var user = new User
        {
            Name = name.Trim(),
            Email = NormalizeEmail(email),
            Role = role
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        await _users.CreateAsync(user);
        return user;
    }

    public async Task<bool> EmailExistsAsync(string email) =>
        await _users.GetByEmailAsync(NormalizeEmail(email)) is not null;
}
