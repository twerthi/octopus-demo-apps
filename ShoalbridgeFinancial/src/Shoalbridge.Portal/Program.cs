using System.Text;
using Shoalbridge.Portal;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();
builder.Services.AddHttpClient("payments", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentsApi:BaseUrl"] ?? "http://localhost:5201");
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();
await app.UseOctopusFeatureFlags("Shoalbridge Online Banking");

// Maintenance mode: a runbook flips Portal:MaintenanceMode to take the portal offline safely.
// Health checks stay up so load balancers and Octopus can still see the app, and the
// maintenance page can still load its logo, theme, and feature flags.
app.Use(async (context, next) =>
{
    var config = context.RequestServices.GetRequiredService<IConfiguration>();
    var path = context.Request.Path;
    if (config.GetValue("Portal:MaintenanceMode", false)
        && !path.StartsWithSegments("/health")
        && !path.StartsWithSegments("/api/info")
        && !path.StartsWithSegments("/api/features")
        && !path.StartsWithSegments("/images")
        && !path.StartsWithSegments("/theme.js"))
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.Headers.RetryAfter = "600";
        if (path.StartsWithSegments("/api"))
        {
            await context.Response.WriteAsJsonAsync(new { title = "Scheduled maintenance" });
            return;
        }
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "maintenance.html"));
        return;
    }
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config) => new
{
    Portal = DeploymentInfo.From(config, "Shoalbridge Financial", "Shoalbridge.Portal"),
    MaintenanceMode = config.GetValue("Portal:MaintenanceMode", false),
    SupportPhone = config["Portal:SupportPhone"] ?? ""
});

// Octopus Feature Flags. The page asks for these to pick its default theme.
app.MapGet("/api/features", FeatureFlags.Evaluate);

app.MapGet("/api/upstream/info", (IHttpClientFactory f) => Forward(f, HttpMethod.Get, "api/info"));
app.MapGet("/api/accounts", (IHttpClientFactory f) => Forward(f, HttpMethod.Get, "api/accounts"));
app.MapGet("/api/payments", (IHttpClientFactory f) => Forward(f, HttpMethod.Get, "api/payments"));
app.MapPost("/api/payments", async (HttpRequest request, IHttpClientFactory f) =>
{
    using var reader = new StreamReader(request.Body);
    return await Forward(f, HttpMethod.Post, "api/payments", await reader.ReadToEndAsync());
});

app.Run();

static async Task<IResult> Forward(IHttpClientFactory factory, HttpMethod method, string path, string? json = null)
{
    try
    {
        using var message = new HttpRequestMessage(method, path);
        if (json is not null) message.Content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await factory.CreateClient("payments").SendAsync(message);
        var body = await response.Content.ReadAsStringAsync();
        var type = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        return Results.Content(body, type, statusCode: (int)response.StatusCode);
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        return Results.Problem(
            title: "Payments service is unreachable",
            detail: "Check that Shoalbridge.Payments.Api is deployed and PaymentsApi:BaseUrl points to it.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
