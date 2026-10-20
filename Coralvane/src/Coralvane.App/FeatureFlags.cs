using System.Reflection;
using Octopus.OpenFeature.Provider;

namespace Coralvane.App;

/// <summary>
/// Feature flags come from Octopus Feature Flags through OpenFeature. Octopus sets
/// FeatureFlags:ClientIdentifier at deploy time to #{Octopus.FeatureToggles.ClientIdentifier},
/// which tells the flag service which project, environment, and tenant to evaluate for.
/// Without a client identifier, or if the service can't be reached, every flag returns its default.
/// The dark mode flag's slug comes from FeatureFlags:DarkModeSlug, so it can match whatever the
/// flag is called in Octopus.
/// </summary>
public static class FeatureFlags
{
    public const string DefaultDarkModeSlug = "default-dark-mode";

    public static async Task UseOctopusFeatureFlags(this WebApplication app, string product)
    {
        var clientIdentifier = app.Configuration["FeatureFlags:ClientIdentifier"];
        if (string.IsNullOrWhiteSpace(clientIdentifier) || clientIdentifier.StartsWith("#{"))
        {
            app.Logger.LogInformation("FeatureFlags:ClientIdentifier isn't set, so feature flags use their defaults.");
            return;
        }

        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? "0.0.0";

        var configuration = new OctopusFeatureConfiguration(clientIdentifier, new ProductMetadata(product, version))
        {
            LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>()
        };
        await OpenFeature.Api.Instance.SetProviderAsync(new OctopusFeatureProvider(configuration));
    }

    public static async Task<object> Evaluate(IConfiguration config)
    {
        var darkModeSlug = config["FeatureFlags:DarkModeSlug"];
        if (string.IsNullOrWhiteSpace(darkModeSlug)) darkModeSlug = DefaultDarkModeSlug;

        var client = OpenFeature.Api.Instance.GetClient();
        return new
        {
            DefaultDarkMode = await client.GetBooleanValueAsync(darkModeSlug, false)
        };
    }
}
