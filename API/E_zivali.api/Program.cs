using Google.Cloud.Firestore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options => { options.AddFixedWindowLimiter("tracker", limiterOptions =>
{ limiterOptions.PermitLimit = 6; limiterOptions.Window = TimeSpan.FromMinutes(1); limiterOptions.QueueLimit = 0; limiterOptions.AutoReplenishment = true; });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; });

var app = builder.Build();
app.UseRateLimiter();

var trackerSecrets = builder.Configuration.GetSection("TrackerSecrets").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();

string potDoKljuca = Path.Combine(AppContext.BaseDirectory, "Secrets", "firebase-service-account.json");

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", potDoKljuca);

FirestoreDb firestore = FirestoreDb.Create("Firebase-project-id");

app.MapGet("/", () => { return "E-zivali GPS API deluje."; });

app.MapPost("/location", async (LokacijaRequest lokacija) =>
{ if (!trackerSecrets.TryGetValue(lokacija.DeviceId, out string? praviSecret)) 

    { return Results.Unauthorized(); }

    if (lokacija.DeviceSecret != praviSecret) 

    { return Results.Unauthorized(); } 

    if (lokacija.Latitude < -90 || lokacija.Latitude > 90 || lokacija.Longitude < -180 || lokacija.Longitude > 180) 

    { return Results.BadRequest("Neveljavne koordinate."); } 

    Dictionary<string, object> podatki = new Dictionary<string, object> 
    { { "deviceId", lokacija.DeviceId }, { "latitude", lokacija.Latitude },
        { "longitude", lokacija.Longitude }, { "timestamp", Timestamp.GetCurrentTimestamp() } };
    await firestore.Collection("locations").AddAsync(podatki); return Results.Ok("Lokacija uspe�no shranjena.");}).RequireRateLimiting("tracker");

app.Run("http://0.0.0.0:5000");

public class LokacijaRequest
{
    public string DeviceId { get; set; } = "";

    public string DeviceSecret { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}