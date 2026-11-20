using MediTrack.Web.Models;

namespace MediTrack.Web.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string normalizedEmail);
    Task<User?> GetByIdAsync(string id);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
}
