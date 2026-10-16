# Octopus demo apps

Sample applications for Octopus Deploy demos, docs, and training. Each folder belongs to one of our fictitious companies, and each company's apps are built to show off a different set of Octopus features.

| Company | Who they are | Apps | Octopus features it demos |
|---|---|---|---|
| **Kelpford** | A mid-size enterprise. Our default company. | Kelpford.Web, Kelpford.Api | Environment progression, multi-step deployments, environment-scoped variables |
| **Shoalbridge Financial** | A regulated financial institution | Shoalbridge.Portal, Shoalbridge.Payments.Api | Manual interventions, approvals, audit trails, runbooks |
| **Coralvane** | A SaaS vendor with many customers | Coralvane.App | Tenants, tenant tags, tenant variables, release rings |

Coralvane's customers are Octopus tenants: Saltmarsh Logistics, Tidelark Media, Pebblewick Dental, Kittiwell Veterinary, and Wrackmoor Outfitters.

## Prerequisites

- .NET 10 SDK
- Visual Studio 2026, or any editor that works with the .NET 10 SDK

The apps have no database, so they run anywhere .NET runs. The web front ends use two NuGet packages for feature flags, `OpenFeature` and `Octopus.OpenFeature`. The repository's `nuget.config` restores from nuget.org only.

## Folder layout

```
octopus-demo-apps/
├── build.ps1                     Publishes every app and zips it as an Octopus package
├── Kelpford/
│   ├── Kelpford.sln
│   └── src/  Kelpford.Api, Kelpford.Web
├── ShoalbridgeFinancial/
│   ├── ShoalbridgeFinancial.sln
│   └── src/  Shoalbridge.Payments.Api, Shoalbridge.Portal
└── Coralvane/
    ├── Coralvane.sln
    ├── src/  Coralvane.App
    └── tenants/  One JSON file per tenant, with tags and variable values
```

## Run locally

Open a solution in Visual Studio and press F5. The Kelpford and Shoalbridge solutions include a launch profile that starts both projects together. Pick it from the startup project drop-down.

| App | Local URL |
|---|---|
| Kelpford.Web | http://localhost:5100 |
| Kelpford.Api | http://localhost:5101 |
| Shoalbridge.Portal | http://localhost:5200 |
| Shoalbridge.Payments.Api | http://localhost:5201 |
| Coralvane.App (demo tenant) | http://localhost:5300 |
| Coralvane.App (one per tenant) | http://localhost:5301 to 5305 |

## What every app has in common

Every app shows what's deployed: its version, release number, environment, and machine name. That makes it easy to see a deployment land during a demo.

- `GET /health` returns `Healthy`. Use it for health check steps and load balancers.
- `GET /api/info` returns the deployment details.
- The page header shows a color-coded environment badge: blue for Development, amber for Test, purple for Staging, and red for Production.
- Each web front end shows its company logo in the header and a "Powered by Octopus Deploy" footer. Logos live in each app's `wwwroot/images/` folder: `logo.svg` (full logo) and `logo-mark.svg` (icon). Coralvane tenant logos are in `wwwroot/images/tenants/`, named by `Tenant:Slug`.

Each app reads these settings from `appsettings.json`. Set them in Octopus with structured configuration variables:

| Variable | Value |
|---|---|
| `Deployment:Environment` | `#{Octopus.Environment.Name}` |
| `Deployment:ReleaseNumber` | `#{Octopus.Release.Number}` |
| `FeatureFlags:ClientIdentifier` | `#{Octopus.FeatureToggles.ClientIdentifier}` (web front ends only) |

## Dark mode and feature flags

Every web front end has a light and dark mode toggle in its header. The page picks its theme in this order:

1. The viewer's own choice from the toggle, saved in their browser.
2. The `default-dark-mode` [Octopus feature flag](https://octopus.com/docs/feature-flags). When it's on, the page starts in dark mode.
3. The viewer's operating system setting.

To demo it:

1. In the project, go to **Feature Flags** and create a flag with the slug `default-dark-mode`.
2. Set `FeatureFlags:ClientIdentifier` to `#{Octopus.FeatureToggles.ClientIdentifier}` with structured configuration variables, and deploy. Octopus fills in an identifier for the project, environment, and tenant.
3. Turn the flag on for an environment. New visitors see dark mode within about a minute, because the provider caches flag values for that long. You don't need a new release.

Each app evaluates the flag server-side in `FeatureFlags.cs` and serves the result at `GET /api/features`. Without a client identifier, or if the flag service can't be reached, the flag is off.

For Coralvane, the client identifier is per tenant, so you can turn dark mode on for some tenants and not others. Use the flag's tenant targeting, or roll it out by release ring with tenant tags.

## Run in Docker

Every app has a Linux `Dockerfile`, and each company folder has a `docker-compose.yml` that runs its apps on the same ports as the table above. Docker Desktop must be using Linux containers.

```powershell
cd Kelpford
docker compose up --build
```

- Kelpford and Shoalbridge start the web app and its API, already pointed at each other.
- Coralvane starts the demo tenant plus one container per tenant, all from the same `coralvane-app` image. Tenant values come from `tenants/`.
- Containers listen on port 8080 and report their environment as `Docker`. Pass settings as environment variables, for example `Deployment__Environment` or `Api__BaseUrl`.

To build one image, run `docker build` from the company folder so `Directory.Build.props` is included. Pass the version as a build argument:

```powershell
docker build -f src/Kelpford.Web/Dockerfile --build-arg VERSION=1.2.0 -t kelpford-web:1.2.0 .
```

## Package the apps

Run the build script from PowerShell:

```powershell
./build.ps1 -Version 1.0.0
```

It publishes each app and writes one zip per app to `artifacts/`, named the way Octopus expects, for example `Kelpford.Web.1.0.0.zip`. Push them to the built-in feed:

```powershell
octopus package upload --package artifacts/*.zip --space "Default"
```

Each company folder has its own README with a suggested demo scenario.
