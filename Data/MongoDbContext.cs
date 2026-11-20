using MediTrack.Web.Models;
using MediTrack.Web.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MediTrack.Web.Data;

/// <summary>
/// Single access point to the MongoDB database. Each model gets a typed collection here.
/// Add new collections (Appointments, Slots, Prescriptions...) as the project grows.
/// </summary>
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.ConnectionString))
            throw new InvalidOperationException("MongoDb:ConnectionString is missing.");

        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>(_settings.UsersCollectionName);
}
