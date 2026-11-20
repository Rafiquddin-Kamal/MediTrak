using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MediTrack.Web.Models;

/// <summary>Domain model stored in the "users" collection.</summary>
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Always stored trimmed and lower-case (see AuthService.NormalizeEmail).</summary>
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed with ASP.NET Core's PasswordHasher (PBKDF2). Never store plain text.</summary>
    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Stored as text ("Admin", "Doctor"...) so documents are readable in Compass/mongosh.</summary>
    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public Role Role { get; set; } = Role.Patient;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("failedLoginCount")]
    public int FailedLoginCount { get; set; }

    /// <summary>UTC time until which sign-in is blocked; null when not locked.</summary>
    [BsonElement("lockoutEnd")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? LockoutEnd { get; set; }

    [BsonElement("lastLoginAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? LastLoginAt { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsLockedOut(DateTime nowUtc) => LockoutEnd.HasValue && LockoutEnd.Value > nowUtc;
}
