# Kelpford

Kelpford is a mid-size enterprise and our default fictitious company. Use it whenever a demo doesn't need anything specialized.

## The apps

**Kelpford.Web** is the Kelpford Hub, an internal employee portal. It shows company announcements and what's deployed. It calls Kelpford.Api server-side, so it shows a friendly error if the API isn't deployed or `Api:BaseUrl` is wrong.

**Kelpford.Api** serves the announcements.

## Demo scenario: promoting a release through environments

1. Create a project called **Kelpford Hub** with a lifecycle of Development, Test, Staging, and Production.
2. Add two package steps: deploy **Kelpford.Api** first, then **Kelpford.Web**. Turn on structured configuration variables for `appsettings.json` in both steps.
3. Add a health check step that calls `/health` on each app.
4. Create a release and promote it. The environment badge and the "What's deployed" panel update with each deployment.

To show a misconfiguration, scope `Api:BaseUrl` to the wrong host in Test. The portal reports that the API is unreachable, and fixing the variable and redeploying resolves it.

## Variables

| Variable | Example | Notes |
|---|---|---|
| `Deployment:Environment` | `#{Octopus.Environment.Name}` | Both apps |
| `Deployment:ReleaseNumber` | `#{Octopus.Release.Number}` | Both apps |
| `FeatureFlags:ClientIdentifier` | `#{Octopus.FeatureToggles.ClientIdentifier}` | Web only. Connects the app to Octopus Feature Flags. |
| `FeatureFlags:DarkModeSlug` | `default-dark-mode` | Web only. The slug of the dark mode flag in Octopus. |
| `Api:BaseUrl` | `https://api.test.kelpford.example` | Web only. Scope per environment. |
| `Portal:Banner` | `Systems maintenance Saturday 08:00` | Web only. Leave empty to hide the banner. |
| `Announcements:Office` | `Seattle office` | API only |
