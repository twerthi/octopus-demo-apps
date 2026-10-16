<#
.SYNOPSIS
    Publishes every demo app and packages it as an Octopus-ready zip.
.EXAMPLE
    ./build.ps1 -Version 1.2.0
#>
param(
    [string] $Version = "1.0.0",
    [string] $Output = "artifacts"
)

$ErrorActionPreference = "Stop"

$projects = @(
    "Kelpford/src/Kelpford.Api/Kelpford.Api.csproj",
    "Kelpford/src/Kelpford.Web/Kelpford.Web.csproj",
    "ShoalbridgeFinancial/src/Shoalbridge.Payments.Api/Shoalbridge.Payments.Api.csproj",
    "ShoalbridgeFinancial/src/Shoalbridge.Portal/Shoalbridge.Portal.csproj",
    "Coralvane/src/Coralvane.App/Coralvane.App.csproj"
)

$artifacts = Join-Path $PSScriptRoot $Output
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

foreach ($project in $projects) {
    $id = [IO.Path]::GetFileNameWithoutExtension($project)
    $publishDir = Join-Path $PSScriptRoot "publish/$id"
    if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

    Write-Host "Publishing $id $Version" -ForegroundColor Cyan
    dotnet publish (Join-Path $PSScriptRoot $project) -c Release -o $publishDir "/p:Version=$Version"
    if ($LASTEXITCODE -ne 0) { throw "Publish failed for $id" }

    $zip = Join-Path $artifacts "$id.$Version.zip"
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zip
    Write-Host "Packaged $zip" -ForegroundColor Green
}
