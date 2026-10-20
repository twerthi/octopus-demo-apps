# Shoalbridge Financial

Shoalbridge Financial is a regulated financial institution. Use it for demos about approvals, compliance, audit trails, and controlled change.

## The apps

**Shoalbridge.Portal** is the customer online banking site. Customers see their accounts and recent payments, and can send a payment. When `Portal:MaintenanceMode` is `true`, the portal shows a maintenance page while `/health` and `/api/info` keep responding.

**Shoalbridge.Payments.Api** holds the accounts and processes payments. It rejects any payment that takes an account over `Payments:DailyLimit` for the day. When `Compliance:AuditMode` is on, it writes an audit log entry for each payment and marks it **Audited** in the portal.

Data is held in memory and resets when the API restarts.

## Demo scenario: a controlled production release

1. Create a project called **Shoalbridge Online Banking** with a lifecycle of Development, Test, Staging, and Production.
2. Deploy **Shoalbridge.Payments.Api** first, then **Shoalbridge.Portal**.
3. Add a manual intervention step before the deployment steps, scoped to Production. Assign it to a Change Approvers team.
4. Set `Compliance:AuditMode` to `true` in Production only, and use a lower `Payments:DailyLimit` in Test.
5. Promote a release. In Test, send a payment over the limit to show the variable at work. In Production, show the approval and the Audited tag on new payments.

## Runbooks

Create two runbooks: **Enable maintenance mode** and **Disable maintenance mode**. Each one sets `Portal:MaintenanceMode` in the portal's `appsettings.json` and restarts the app. To test locally, use the "maintenance mode" launch profile.

## Variables

| Variable | Example | Notes |
|---|---|---|
| `Deployment:Environment` | `#{Octopus.Environment.Name}` | Both apps |
| `Deployment:ReleaseNumber` | `#{Octopus.Release.Number}` | Both apps |
| `FeatureFlags:ClientIdentifier` | `#{Octopus.FeatureToggles.ClientIdentifier}` | Portal only. Connects the app to Octopus Feature Flags. |
| `FeatureFlags:DarkModeSlug` | `default-dark-mode` | Portal only. The slug of the dark mode flag in Octopus. |
| `PaymentsApi:BaseUrl` | `https://payments.staging.shoalbridge.example` | Portal only. Scope per environment. |
| `Portal:MaintenanceMode` | `false` | Portal only |
| `Portal:SupportPhone` | `1-800-555-0142` | Portal only |
| `Payments:DailyLimit` | `10000` | API only. Use `500` in Test to demo rejections. |
| `Compliance:AuditMode` | `true` | API only. Scope to Production. |
