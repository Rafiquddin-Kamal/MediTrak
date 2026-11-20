using MediTrack.Web.Data;
using MediTrack.Web.Models;
using MongoDB.Driver;

namespace MediTrack.Web.Repositories;

/// <summary>All MongoDB queries for users live here, so controllers and services never touch the driver.</summary>
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail) =>
        await _users.Find(u => u.Email == normalizedEmail).FirstOrDefaultAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    /// <summary>Throws MongoWriteException (DuplicateKey) if the email already exists.</summary>
    public Task CreateAsync(User user) => _users.InsertOneAsync(user);

    /// <summary>Saves only the fields the login process changes, so other edits are not overwritten.</summary>
    public Task UpdateAsync(User user)
    {
        var update = Builders<User>.Update
            .Set(u => u.PasswordHash, user.PasswordHash)
            .Set(u => u.FailedLoginCount, user.FailedLoginCount)
            .Set(u => u.LockoutEnd, user.LockoutEnd)
            .Set(u => u.LastLoginAt, user.LastLoginAt);

        return _users.UpdateOneAsync(u => u.Id == user.Id, update);
    }
}
