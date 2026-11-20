using MediTrack.Web.Models;
using MediTrack.Web.Services;
using MongoDB.Driver;

namespace MediTrack.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, bool isDevelopment)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        // Business rule: one account per email address (emails are stored lower-case).
        // MongoDB has no schema, so the unique index is created from code on every start
        // (creating an index that already exists does nothing).
        var emailIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });
        await context.Users.Indexes.CreateOneAsync(emailIndex);

        if (!isDevelopment) return;

        // Create a demo account for local testing.
        var seed = config.GetSection("SeedUser");
        var email = seed["Email"];
        var password = seed["Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        if (await auth.EmailExistsAsync(email)) return;

        var role = Enum.TryParse<Role>(seed["Role"], ignoreCase: true, out var parsed) ? parsed : Role.Admin;
        await auth.CreateUserAsync(seed["Name"] ?? "Demo User", email, password, role);
    }
}
