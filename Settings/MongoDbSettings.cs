namespace MediTrack.Web.Settings;

/// <summary>Bound from the "MongoDb" section of appsettings.json (or user secrets).</summary>
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UsersCollectionName { get; set; } = "users";
}
