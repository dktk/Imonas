param(
    [Parameter(Mandatory = $true)]
    [ValidateScript({ Test-Path $_ -PathType Leaf })]
    [string]$ZipPath,

    [Parameter(Mandatory = $true)]
    [string]$RemoteHost,   # renamed from Host

    [Parameter(Mandatory = $true)]
    [string]$User,

    [Parameter(Mandatory = $true)]
    [string]$RemotePath,

    [int]$Port = 22,
    [string]$KeyPath = "",
    [switch]$EnsureRemoteDir
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-Tool([string]$name) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        throw "Required tool not found on PATH: $name"
    }
}

Ensure-Tool "scp"
Ensure-Tool "ssh"

$zipFull = (Resolve-Path $ZipPath).Path
$zipName = [IO.Path]::GetFileName($zipFull)

if (-not $RemotePath.EndsWith("/")) { $RemotePath += "/" }

$sshBaseArgs = @("-p", "$Port")
$scpArgs = @("-P", "$Port")

if ($KeyPath -and $KeyPath.Trim().Length -gt 0) {
    $keyFull = (Resolve-Path $KeyPath).Path
    $sshBaseArgs += @("-i", $keyFull)
    $scpArgs += @("-i", $keyFull)
}

if ($EnsureRemoteDir) {
    Write-Host "==> Ensuring remote directory exists: $RemotePath"
    $mkdirCmd = "mkdir -p `"$RemotePath`""
    & ssh @sshBaseArgs "$User@$RemoteHost" $mkdirCmd
    if ($LASTEXITCODE -ne 0) { throw "Failed to create remote directory." }
}

Write-Host "==> Copying:"
Write-Host "    Local:  $zipFull"
Write-Host "    Remote: $User@${RemoteHost}:$RemotePath"
Write-Host ""

$dest = "${User}@${RemoteHost}:${RemotePath}${zipName}"

try {
    & scp @scpArgs $zipFull $dest
    if ($LASTEXITCODE -ne 0) { throw "SCP failed with exit code $LASTEXITCODE" }

    Write-Host "==> Done. Uploaded $zipName to $User@${RemoteHost}:$RemotePath"
}
finally {
    # Cleanup: delete the zip + delete sibling folder with same name as zip (minus .zip)
    $zipDir = [IO.Path]::GetDirectoryName($zipFull)
    $folderName = [IO.Path]::GetFileNameWithoutExtension($zipFull)
    $folderPath = Join-Path $zipDir $folderName

    Write-Host ""
    Write-Host "==> Cleanup:"

    if (Test-Path $zipFull -PathType Leaf) {
        Write-Host "    Removing zip: $zipFull"
        Remove-Item -LiteralPath $zipFull -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "    Zip not found (already removed?): $zipFull"
    }

    if (Test-Path $folderPath -PathType Container) {
        Write-Host "    Removing folder: $folderPath"
        Remove-Item -LiteralPath $folderPath -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "    Folder not found (already removed?): $folderPath"
    }
}
