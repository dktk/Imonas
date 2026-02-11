param(
  [Parameter(Mandatory=$true)]
  [string]$ProjectPath,

  # Runtime identifier for the target platform (default: linux-x64)
  [string]$Runtime = "linux-x64",

  # Optional: publish output root folder (defaults to ./artifacts/publish/<ProjectName>/<timestamp>)
  [string]$OutRoot = (Join-Path (Get-Location) "artifacts\publish"),

  # Optional: if set, will run `dotnet restore` first
  [switch]$Restore
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-Tool([string]$name) {
  if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
    throw "Required tool not found on PATH: $name"
  }
}

Ensure-Tool "dotnet"

$ProjectFull = (Resolve-Path $ProjectPath).Path
$ProjectDir  = Split-Path $ProjectFull -Parent
$ProjectName = [IO.Path]::GetFileNameWithoutExtension($ProjectFull)

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$publishDir = Join-Path $OutRoot "$ProjectName\$timestamp"
$zipPath    = Join-Path $OutRoot "$ProjectName\$timestamp.zip"

New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Write-Host "Project:      $ProjectFull"
Write-Host "Publish dir:  $publishDir"
Write-Host "Zip output:   $zipPath"
Write-Host ""

Push-Location $ProjectDir
try {
  if ($Restore) {
    Write-Host "==> Restoring..."
    dotnet restore $ProjectFull
  }

  Write-Host "==> Publishing (Release, $Runtime)..."
  dotnet publish $ProjectFull -c Release -r $Runtime --self-contained false -o $publishDir

  Write-Host "==> Creating zip..."
  if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
  Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force

  Write-Host ""
  Write-Host "Done."
  Write-Host "Publish folder: $publishDir"
  Write-Host "Zip file:       $zipPath"
}
finally {
  Pop-Location
}
