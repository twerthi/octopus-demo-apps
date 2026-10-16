using Kelpford.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config) =>
    DeploymentInfo.From(config, "Kelpford", "Kelpford.Api"));

app.MapGet("/api/announcements", (IConfiguration config) =>
{
    var office = config["Announcements:Office"] ?? "All offices";
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    return new[]
    {
        new Announcement("Quarterly town hall", $"Join leadership on Thursday at 10:00. Streaming to {office}.", today),
        new Announcement("New expense policy", "Receipts under $25 no longer need manager approval.", today.AddDays(-2)),
        new Announcement("Parking garage resurfacing", "Level 2 is closed through Friday.", today.AddDays(-5)),
    };
});

app.Run();

record Announcement(string Title, string Body, DateOnly Posted);
