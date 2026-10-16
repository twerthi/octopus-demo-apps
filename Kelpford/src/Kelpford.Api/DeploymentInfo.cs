using System.Reflection;

namespace Kelpford.Api;

/// <summary>
/// What's running and where. Octopus sets the Deployment:* values at deploy time
/// through structured configuration variables.
/// </summary>
public record DeploymentInfo(
    string Company,
    string Application,
    string Version,
    string Environment,
    string Release,
    string Machine,
    DateTimeOffset StartedAt)
{
    private static readonly DateTimeOffset Started = DateTimeOffset.UtcNow;

    public static DeploymentInfo From(IConfiguration config, string company, string application)
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";
        var plus = version.IndexOf('+');
        if (plus > 0) version = version[..plus];

        return new DeploymentInfo(
            company,
            application,
            version,
            config["Deployment:Environment"] ?? "Local",
            config["Deployment:ReleaseNumber"] ?? "local",
            System.Environment.MachineName,
            Started);
    }
}
