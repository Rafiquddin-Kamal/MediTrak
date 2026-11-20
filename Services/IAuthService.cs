using MediTrack.Web.Models;

namespace MediTrack.Web.Services;

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    LockedOut,
    Inactive
}

public record LoginResult(LoginStatus Status, User? User = null, DateTime? LockoutEndUtc = null);

public interface IAuthService
{
    Task<LoginResult> ValidateCredentialsAsync(string email, string password);
    Task<User> CreateUserAsync(string name, string email, string password, Role role);
    Task<bool> EmailExistsAsync(string email);
}
