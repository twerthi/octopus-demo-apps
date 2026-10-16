# Coralvane

Coralvane sells Coralvane Workspace, a SaaS platform for bookings and customer requests. Use it for multi-tenancy demos.

## The app

**Coralvane.App** is a single package that serves every customer. Everything that differs between customers comes from configuration: the name, brand color, plan, region, release ring, and which features are switched on. In Octopus, those values are tenant variables.

Premium tenants get reporting. Tenants with `Features:BetaDashboard` turned on see an extra Insights card.

## Tenants

The `tenants/` folder has one file per customer, with its tags and variable values. Visual Studio has a launch profile for each tenant, so you can run them side by side.

| Tenant | Tier | Region | Release ring | Local URL |
|---|---|---|---|---|
| Saltmarsh Logistics | Premium | US-West | GA | http://localhost:5301 |
| Tidelark Media | Standard | EU | Beta | http://localhost:5302 |
| Pebblewick Dental | Standard | US-East | GA | http://localhost:5303 |
| Kittiwell Veterinary | Premium | UK | Alpha | http://localhost:5304 |
| Wrackmoor Outfitters | Standard | US-West | GA | http://localhost:5305 |

## Demo scenario: rolling out by release ring

1. Create tenant tag sets called **Tier**, **Region**, and **Release Ring**, using the values above.
2. Create the five tenants and tag them.
3. Create a project called **Coralvane Workspace** that requires tenanted deployments. Add a package step for **Coralvane.App** with structured configuration variables turned on.
4. Add project variable templates for each `Tenant:*` and `Features:*` setting, then fill in each tenant's values from its file in `tenants/`.
5. Create a release and deploy it to the Alpha ring first (Kittiwell Veterinary), then Beta (Tidelark Media), then GA. Each tenant's site shows its own branding with the same version number.

For a bonus, upgrade Pebblewick Dental to Premium by changing its tag and `Features:Reporting`, and redeploy. Reporting appears without a new release.

## Variables

| Variable | Example |
|---|---|
| `Deployment:Environment` | `#{Octopus.Environment.Name}` |
| `Deployment:ReleaseNumber` | `#{Octopus.Release.Number}` |
| `FeatureFlags:ClientIdentifier` | `#{Octopus.FeatureToggles.ClientIdentifier}` |
| `Tenant:Name` | `Pebblewick Dental` |
| `Tenant:Slug` | `pebblewick-dental` |
| `Tenant:PrimaryColor` | `#2a9d8f` |
| `Tenant:Tier` | `Standard` |
| `Tenant:Region` | `US-East` |
| `Tenant:ReleaseRing` | `GA` |
| `Tenant:SupportEmail` | `support@pebblewick-dental.example` |
| `Features:Reporting` | `false` |
| `Features:BetaDashboard` | `false` |
