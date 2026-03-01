# watch-docs.ps1
# Watches for .cs and .md file changes and rebuilds DocFX documentation automatically.
# Run from C:\BlackBoxConfigurator (the folder containing docfx.json).
#
# Usage:
#   .\watch-docs.ps1              # build + serve at http://localhost:8080, auto-rebuild on changes
#   .\watch-docs.ps1 -Port 9000   # use a different port

param(
    [int]$Port = 8080,
    [switch]$BuildOnly   # skip the HTTP server, just build once
)

Set-Location $PSScriptRoot

# ── 1. Ensure docfx is available ────────────────────────────────────────────
if (-not (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Write-Host "docfx not found — installing as a global .NET tool..." -ForegroundColor Cyan
    dotnet tool install -g docfx
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install docfx. Run: dotnet tool install -g docfx"
        exit 1
    }
    # Refresh PATH for the current session
    $env:PATH = [System.Environment]::GetEnvironmentVariable("PATH","Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("PATH","User")
}

Write-Host "docfx version: $(docfx --version)" -ForegroundColor Gray

# ── 2. Initial build ─────────────────────────────────────────────────────────
function Invoke-DocFxBuild {
    Write-Host "`n[$(Get-Date -Format 'HH:mm:ss')] Building documentation..." -ForegroundColor Cyan
    docfx build docfx.json 2>&1 | ForEach-Object { Write-Host "  $_" }
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Build complete. Browse: http://localhost:$Port" -ForegroundColor Green
    } else {
        Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Build failed — see output above." -ForegroundColor Red
    }
}

Invoke-DocFxBuild

if ($BuildOnly) { exit 0 }

# ── 3. Start the HTTP server in a background job ─────────────────────────────
$serverJob = Start-Job -ScriptBlock {
    param($port, $root)
    Set-Location $root
    docfx serve _site --port $port
} -ArgumentList $Port, $PSScriptRoot

Write-Host "Serving docs at http://localhost:$Port" -ForegroundColor Yellow
Write-Host "Watching for changes in *.cs and docs\  (Ctrl+C to stop)`n" -ForegroundColor Yellow

# ── 4. FileSystemWatcher — rebuild on any .cs or .md change ─────────────────
$watchPaths = @(
    $PSScriptRoot,
    (Join-Path $PSScriptRoot "docs")
)

$watchers = @()
$rebuildPending = $false
$lastRebuild = [datetime]::MinValue

foreach ($path in $watchPaths) {
    if (-not (Test-Path $path)) { continue }

    $w = New-Object System.IO.FileSystemWatcher
    $w.Path = $path
    $w.Filter = "*.*"
    $w.IncludeSubdirectories = $true
    $w.NotifyFilter = [System.IO.NotifyFilters]::LastWrite -bor [System.IO.NotifyFilters]::FileName

    $action = {
        $name = $Event.SourceEventArgs.Name
        $ext  = [System.IO.Path]::GetExtension($name).ToLower()
        if ($ext -in @('.cs', '.md', '.yml', '.json')) {
            $global:rebuildPending = $true
        }
    }

    Register-ObjectEvent $w Changed -Action $action | Out-Null
    Register-ObjectEvent $w Created -Action $action | Out-Null
    Register-ObjectEvent $w Renamed -Action $action | Out-Null
    $w.EnableRaisingEvents = $true
    $watchers += $w
}

# ── 5. Main loop — coalesces rapid saves into a single rebuild ───────────────
try {
    while ($true) {
        Start-Sleep -Milliseconds 500

        if ($rebuildPending -and ([datetime]::Now - $lastRebuild).TotalSeconds -ge 2) {
            $rebuildPending = $false
            $lastRebuild = [datetime]::Now
            Invoke-DocFxBuild
        }
    }
} finally {
    foreach ($w in $watchers) { $w.Dispose() }
    Stop-Job $serverJob -ErrorAction SilentlyContinue
    Remove-Job $serverJob -ErrorAction SilentlyContinue
    Write-Host "`nDoc watcher stopped." -ForegroundColor Gray
}
