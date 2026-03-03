# Build script for ObsidianTaskNotesExtension EXE installers
# Used by GitHub Actions to produce self-contained EXE packages for WinGet distribution.
#
# Usage (from the ObsidianTaskNotesExtension/ObsidianTaskNotesExtension directory):
#   .\build-exe.ps1 -Version "0.4.0.0"
#   .\build-exe.ps1 -Version "0.4.0.0" -Platforms @("x64", "arm64")
#
# Prerequisites:
#   - .NET 9 SDK   (https://dotnet.microsoft.com/download/dotnet/9.0)
#   - Inno Setup 6 (https://jrsoftware.org/isdl.php  or  choco install innosetup)
# spell-checker:ignore innosetup iscc winget wingetcreate LASTEXITCODE csproj

param(
    [string]$ExtensionName = "ObsidianTaskNotesExtension",
    [string]$Configuration = "Release",
    [string]$Version,
    [string[]]$Platforms = @("x64", "arm64")
)

$ErrorActionPreference = "Stop"

$ProjectDir      = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile     = Join-Path $ProjectDir "$ExtensionName.csproj"
$SetupTemplate   = Join-Path $ProjectDir "setup-template.iss"
$InstallerOutDir = Join-Path $ProjectDir "bin\$Configuration\installer"
$RepoRoot        = Split-Path -Parent $ProjectDir

# ---------------------------------------------------------------------------
# Resolve version
# ---------------------------------------------------------------------------
if (-not $Version) {
    $xml     = [xml](Get-Content $ProjectFile)
    $Version = $xml.Project.PropertyGroup.AppxPackageVersion | Select-Object -First 1
    if (-not $Version) {
        $Version = "0.0.1.0"
        Write-Warning "No AppxPackageVersion found in csproj; using default: $Version"
    }
}

Write-Host "Building $ExtensionName EXE installer v$Version" -ForegroundColor Green
Write-Host "Platforms : $($Platforms -join ', ')"            -ForegroundColor Yellow
Write-Host "Output    : $InstallerOutDir"                    -ForegroundColor Yellow

# ---------------------------------------------------------------------------
# Validate prerequisites
# ---------------------------------------------------------------------------
$InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"
if (-not (Test-Path $InnoSetupPath)) {
    $InnoSetupPath = "${env:ProgramFiles}\Inno Setup 6\iscc.exe"
}
if (-not (Test-Path $InnoSetupPath)) {
    throw "Inno Setup 6 not found. Install with: choco install innosetup -y`nor download from https://jrsoftware.org/isdl.php"
}

if (-not (Test-Path $SetupTemplate)) {
    throw "Setup template not found at: $SetupTemplate"
}

# ---------------------------------------------------------------------------
# Restore NuGet packages
# ---------------------------------------------------------------------------
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed with exit code $LASTEXITCODE"
}

# ---------------------------------------------------------------------------
# Ensure installer output directory exists
# ---------------------------------------------------------------------------
if (-not (Test-Path $InstallerOutDir)) {
    New-Item -ItemType Directory -Path $InstallerOutDir -Force | Out-Null
}

# ---------------------------------------------------------------------------
# Build each platform
# ---------------------------------------------------------------------------
foreach ($Platform in $Platforms) {
    Write-Host "`n=== Building $Platform ===" -ForegroundColor Cyan

    # Publish self-contained executable
    Write-Host "Publishing $Platform (self-contained)..." -ForegroundColor Yellow
    dotnet publish $ProjectFile `
        --configuration $Configuration `
        --runtime "win-$Platform" `
        --self-contained true `
        --verbosity normal

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $Platform (exit code $LASTEXITCODE)"
    }

    $PublishDir = Join-Path $ProjectDir "bin\$Configuration\net9.0-windows10.0.26100.0\win-$Platform\publish"
    if (-not (Test-Path $PublishDir)) {
        throw "Expected publish output not found: $PublishDir"
    }

    $FileCount = (Get-ChildItem -Path $PublishDir -Recurse -File).Count
    Write-Host "Published $FileCount files to: $PublishDir" -ForegroundColor Green

    # ------------------------------------------------------------------
    # Generate platform-specific Inno Setup script
    # ------------------------------------------------------------------
    $SetupScript = Get-Content $SetupTemplate -Raw

    # Update version number
    $SetupScript = $SetupScript -replace '#define AppVersion ".*"', "#define AppVersion `"$Version`""

    # Add platform suffix to output filename
    $SetupScript = $SetupScript -replace 'OutputBaseFilename=(.*?)\{#AppVersion\}',
        "OutputBaseFilename=`$1{#AppVersion}-$Platform"

    # Point source path at the platform-specific publish folder
    $SetupScript = $SetupScript -replace (
        'Source: "bin\\Release\\net9\.0-windows10\.0\.26100\.0\\win-x64\\publish'),
        "Source: `"bin\$Configuration\net9.0-windows10.0.26100.0\win-$Platform\publish"

    # Architecture constraints
    $Crlf = [char]13 + [char]10
    if ($Platform -eq "arm64") {
        $SetupScript = $SetupScript -replace '(\[Setup\][^\[]*)(MinVersion=)',
            ('$1ArchitecturesAllowed=arm64' + $Crlf + 'ArchitecturesInstallIn64BitMode=arm64' + $Crlf + '$2')
    } else {
        $SetupScript = $SetupScript -replace '(\[Setup\][^\[]*)(MinVersion=)',
            ('$1ArchitecturesAllowed=x64compatible' + $Crlf + 'ArchitecturesInstallIn64BitMode=x64compatible' + $Crlf + '$2')
    }

    # Resolve the LICENSE path to an absolute path for Inno Setup
    $LicensePath  = Resolve-Path (Join-Path $RepoRoot "LICENSE.txt")
    $SetupScript  = $SetupScript -replace 'LicenseFile=LICENSE\.txt', "LicenseFile=`"$LicensePath`""

    $PlatformIss = Join-Path $ProjectDir "setup-$Platform.iss"
    $SetupScript | Out-File -FilePath $PlatformIss -Encoding UTF8

    # ------------------------------------------------------------------
    # Run Inno Setup
    # ------------------------------------------------------------------
    Write-Host "Creating $Platform installer with Inno Setup..." -ForegroundColor Yellow
    & $InnoSetupPath $PlatformIss

    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed for $Platform (exit code $LASTEXITCODE)"
    }

    $Installer = Get-ChildItem "$InstallerOutDir\*-$Platform.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1
    if ($Installer) {
        $SizeMB = [math]::Round($Installer.Length / 1MB, 2)
        Write-Host "Created: $($Installer.Name) ($SizeMB MB)" -ForegroundColor Green
    }

    # Clean up temp Inno Setup script
    Remove-Item $PlatformIss -ErrorAction SilentlyContinue
}

Write-Host "`nBuild complete! Installers in: $InstallerOutDir" -ForegroundColor Green
