# watch-docs.ps1
# Watches for .cs and .md file changes and rebuilds DocFX documentation automatically.
# Run from C:\BlackBoxConfigurator\BlackBoxControl1 (the folder containing docfx.json).
#
# Usage:
#   .\watch-docs.ps1              # build + serve at http://localhost:8080
#   .\watch-docs.ps1 -Port 9000   # use a different port
#   .\watch-docs.ps1 -BuildOnly   # build once, no server or watcher

param(
    [int]$Port = 8080,
    [switch]$BuildOnly
)

Set-Location $PSScriptRoot

# ── 1. Ensure docfx is available ────────────────────────────────────────────
if (-not (Get-Command docfx -ErrorAction SilentlyContinue)) {
    Write-Host "docfx not found. Run: dotnet tool install -g docfx" -ForegroundColor Red
    Write-Host "Then restart this terminal and try again." -ForegroundColor Red
    exit 1
}

Write-Host "docfx $(docfx --version)" -ForegroundColor Gray

# ── 2. Build function ────────────────────────────────────────────────────────
function Invoke-DocFxBuild {
    $ts = Get-Date -Format "HH:mm:ss"
    Write-Host ""
    Write-Host "[$ts] Building documentation..." -ForegroundColor Cyan
    docfx build docfx.json 2>&1 | ForEach-Object { Write-Host "  $_" }
    $ts2 = Get-Date -Format "HH:mm:ss"
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[$ts2] Done. Browse: http://localhost:$Port" -ForegroundColor Green
    } else {
        Write-Host "[$ts2] Build failed." -ForegroundColor Red
    }
}

Invoke-DocFxBuild

if ($BuildOnly) { exit 0 }

# ── 3. HTTP server in a background job ───────────────────────────────────────
$serverJob = Start-Job -ScriptBlock {
    param($p, $r)
    Set-Location $r
    docfx serve _site --port $p
} -ArgumentList $Port, $PSScriptRoot

Write-Host "Serving at http://localhost:$Port" -ForegroundColor Yellow
Write-Host "Watching *.cs, *.md, *.yml for changes. Press Ctrl+C to stop." -ForegroundColor Yellow

# ── 4. FileSystemWatcher ─────────────────────────────────────────────────────
$watchPaths = @($PSScriptRoot, (Join-Path $PSScriptRoot "docs"))

$watchers = @()
$global:rebuildPending = $false
$lastRebuild = [datetime]::MinValue

foreach ($path in $watchPaths) {
    if (-not (Test-Path $path)) { continue }

    $w = New-Object System.IO.FileSystemWatcher
    $w.Path                  = $path
    $w.Filter                = "*.*"
    $w.IncludeSubdirectories = $true
    $w.NotifyFilter          = [System.IO.NotifyFilters]::LastWrite -bor
                               [System.IO.NotifyFilters]::FileName

    $action = {
        $ext = [System.IO.Path]::GetExtension($Event.SourceEventArgs.Name).ToLower()
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

# ── 5. Poll loop — coalesces rapid saves into one rebuild ────────────────────
try {
    while ($true) {
        Start-Sleep -Milliseconds 500
        $age = ([datetime]::Now - $lastRebuild).TotalSeconds
        if ($global:rebuildPending -and $age -ge 2) {
            $global:rebuildPending = $false
            $lastRebuild = [datetime]::Now
            Invoke-DocFxBuild
        }
    }
} finally {
    foreach ($w in $watchers) { $w.Dispose() }
    Stop-Job  $serverJob -ErrorAction SilentlyContinue
    Remove-Job $serverJob -ErrorAction SilentlyContinue
    Write-Host "Doc watcher stopped." -ForegroundColor Gray
}
