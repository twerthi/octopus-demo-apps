using Coralvane.App;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.Configure<TenantOptions>(builder.Configuration.GetSection("Tenant"));
builder.Services.Configure<FeatureOptions>(builder.Configuration.GetSection("Features"));

var app = builder.Build();
await app.UseOctopusFeatureFlags("Coralvane Workspace");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config) =>
    DeploymentInfo.From(config, "Coralvane", "Coralvane.App"));

// Octopus Feature Flags. The page asks for these to pick its default theme.
app.MapGet("/api/features", FeatureFlags.Evaluate);

// Everything tenant-specific comes from configuration. Octopus supplies it per tenant
// using tenant variables, so one package serves every customer.
app.MapGet("/api/tenant", (Microsoft.Extensions.Options.IOptions<TenantOptions> tenant,
                           Microsoft.Extensions.Options.IOptions<FeatureOptions> features) => new
{
    Tenant = tenant.Value,
    Features = features.Value
});

app.MapGet("/api/dashboard", (Microsoft.Extensions.Options.IOptions<TenantOptions> tenant) =>
{
    // Deterministic per tenant, so each customer shows its own believable numbers.
    var seed = tenant.Value.Slug.Aggregate(17, (hash, c) => unchecked(hash * 31 + c)) & 0x7fffffff;
    var rng = new Random(seed);
    return new
    {
        ActiveUsers = rng.Next(12, 180),
        BookingsThisWeek = rng.Next(40, 900),
        OpenRequests = rng.Next(0, 35),
        Satisfaction = Math.Round(4.1 + rng.NextDouble() * 0.8, 1),
        WeeklyBookings = Enumerable.Range(0, 8).Select(_ => rng.Next(30, 120)).ToArray()
    };
});

app.MapGet("/api/reports", (Microsoft.Extensions.Options.IOptions<FeatureOptions> features) =>
    features.Value.Reporting
        ? Results.Ok(new[]
          {
              new { Name = "Monthly activity summary", Format = "PDF" },
              new { Name = "Customer satisfaction trends", Format = "CSV" },
          })
        : Results.NotFound(new { title = "Reporting isn’t included in this plan." }));

app.Run();

public class TenantOptions
{
    public string Name { get; set; } = "Coralvane Demo";
    public string Slug { get; set; } = "demo";
    public string PrimaryColor { get; set; } = "#e2595b";
    public string Tier { get; set; } = "Standard";
    public string Region { get; set; } = "US-West";
    public string ReleaseRing { get; set; } = "GA";
    public string SupportEmail { get; set; } = "support@coralvane.example";
}

public class FeatureOptions
{
    public bool Reporting { get; set; }
    public bool BetaDashboard { get; set; }
}
