<#
PowerShell helper to remove any existing installed package for this project and register the project's AppxManifest
Usage:
  .\fix_store_app.ps1 [-Configuration Debug] [-Platform x64] [-ProjectDir <path>]

This script will:
 - try to locate the project's Package.appxmanifest and built AppxManifest.xml
 - remove any installed package that matches the Identity Name or DisplayName
 - register the found AppxManifest.xml using Add-AppxPackage -Register
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Debug',
    [ValidateSet('x86','x64','ARM64')]
    [string]$Platform = 'x64',
    [string]$ProjectDir = (Get-Location).Path
)

function Write-Info($msg) { Write-Host "[info] $msg" -ForegroundColor Cyan }
function Write-Warn($msg) { Write-Host "[warn] $msg" -ForegroundColor Yellow }
function Write-Err($msg) { Write-Host "[error] $msg" -ForegroundColor Red }

Push-Location $ProjectDir
try {
    Write-Info "ProjectDir = $ProjectDir"

    # Read Package.appxmanifest in project root if present
    $projectManifestPath = Join-Path $ProjectDir 'Package.appxmanifest'
    $identityName = $null
    if (Test-Path $projectManifestPath) {
        try {
            [xml]$pm = Get-Content $projectManifestPath -ErrorAction Stop
            $identity = $pm.Package.Identity
            if ($identity) { $identityName = $identity.Name }
            Write-Info "Found project manifest: $projectManifestPath" 
        } catch {
            Write-Warn "Failed to parse project manifest: $_"
        }
    } else {
        Write-Warn "No Package.appxmanifest found in project root."
    }

    # Search for built AppxManifest.xml under bin and obj
    $searchPaths = @(
        Join-Path $ProjectDir 'bin', $Platform, $Configuration,
        Join-Path $ProjectDir 'bin', $Platform, $Configuration, '*', '*',
        Join-Path $ProjectDir 'obj', $Platform, $Configuration,
        Join-Path $ProjectDir 'obj'
    )

    $candidates = @()
    foreach ($p in $searchPaths) {
        if (Test-Path $p) {
            $found = Get-ChildItem -Path $p -Recurse -Filter 'AppxManifest.xml' -ErrorAction SilentlyContinue | Select-Object -First 10
            if ($found) { $candidates += $found }
        }
    }

    if (-not $candidates -or $candidates.Count -eq 0) {
        Write-Warn "No built AppxManifest.xml found under bin/obj. Falling back to project Package.appxmanifest."
        if (Test-Path $projectManifestPath) { $candidates = @(Get-Item $projectManifestPath) }
    }

    if (-not $candidates -or $candidates.Count -eq 0) {
        Write-Err "No manifest found to register. Build the project first (Debug/$Platform) and re-run this script."
        exit 1
    }

    # Pick the most likely candidate: prefer AppxManifest.xml under a bin or obj path
    $manifest = $candidates | Where-Object { $_.FullName -match 'AppxManifest.xml' } | Select-Object -First 1
    if (-not $manifest) { $manifest = $candidates[0] }
    $manifestPath = $manifest.FullName
    Write-Info "Using manifest: $manifestPath"

    # If we have identity name from project manifest, try to remove installed package by identity
    if ($identityName) {
        Write-Info "Looking for installed packages with Name = $identityName"
        $pkg = Get-AppxPackage -Name $identityName -ErrorAction SilentlyContinue
        if ($pkg) {
            Write-Info "Removing package: $($pkg.PackageFullName)"
            Remove-AppxPackage -Package $pkg.PackageFullName -ErrorAction Continue
        } else {
            Write-Warn "No package found with Name = $identityName"
        }
    } else {
        # Try fallback by DisplayName from manifest
        try {
            [xml]$x = Get-Content $manifestPath -ErrorAction Stop
            $displayName = $x.Package.Properties.DisplayName
            if ($displayName) {
                Write-Info "Looking for installed packages with DisplayName containing: $displayName"
                $candidatesPkg = Get-AppxPackage | Where-Object { $_.Name -like "*$displayName*" -or $_.DisplayName -like "*$displayName*" }
                foreach ($p in $candidatesPkg) {
                    Write-Info "Removing package: $($p.PackageFullName)"
                    Remove-AppxPackage -Package $p.PackageFullName -ErrorAction Continue
                }
            }
        } catch {
            Write-Warn "Failed to read DisplayName from manifest: $_"
        }
    }

    Write-Info "Registering manifest: $manifestPath"
    try {
        Add-AppxPackage -Register -DisableDevelopmentMode -Path $manifestPath -ForceApplicationShutdown -ErrorAction Stop
        Write-Info "Registration succeeded. Try launching the app from Start or debug from Visual Studio."
    } catch {
        Write-Err "Add-AppxPackage failed: $_"
        exit 1
    }
} finally {
    Pop-Location
}
