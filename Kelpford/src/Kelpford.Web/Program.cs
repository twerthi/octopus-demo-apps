using Kelpford.Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks();

// The portal calls Kelpford.Api server-side. Octopus sets Api:BaseUrl per environment.
builder.Services.AddHttpClient("kelpford-api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5101");
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();
await app.UseOctopusFeatureFlags("Kelpford Hub");

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHealthChecks("/health");

app.MapGet("/api/info", (IConfiguration config) => new
{
    Web = DeploymentInfo.From(config, "Kelpford", "Kelpford.Web"),
    Banner = config["Portal:Banner"] ?? ""
});

// Octopus Feature Flags. The page asks for these to pick its default theme.
app.MapGet("/api/features", FeatureFlags.Evaluate);

app.MapGet("/api/upstream/info", (IHttpClientFactory factory) =>
    Proxy(factory, "api/info"));

app.MapGet("/api/upstream/announcements", (IHttpClientFactory factory) =>
    Proxy(factory, "api/announcements"));

app.Run();

static async Task<IResult> Proxy(IHttpClientFactory factory, string path)
{
    try
    {
        var response = await factory.CreateClient("kelpford-api").GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();
        return Results.Content(body, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
    {
        return Results.Problem(
            title: "Kelpford.Api is unreachable",
            detail: "Check that the API is deployed and Api:BaseUrl points to it.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}
