# Obsidian Task Notes Extension - PSake Build File
# This file defines build, test, and deployment tasks using psake
# spell-checker:ignore LASTEXITCODE csproj msix msixbundle makeappx appxmanifest signtool pfx certutil

# Task configuration
$projectPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension"
$solutionPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension.sln"
$csprojPath = Join-Path $projectPath "ObsidianTaskNotesExtension.csproj"
$installerOutputDir = Join-Path $projectPath "bin\Release\installer"
$appPackagesDir = Join-Path $projectPath "AppPackages"
$buildConfiguration = "Debug"
$runtimes = @("win-x64", "win-arm64")

# Properties to set
Properties {
  $Configuration = "Debug"
  $Platforms = @("x64", "ARM64")
  $SolutionFile = $solutionPath
  $ProjectDir = $projectPath
}

Include ./psakeHelpers.ps1

# Default task
Task default -Depends Build

# Clean
Task Clean -Description "Clean build artifacts" {
  Write-Host "Cleaning solution..." -ForegroundColor Green
    
  # Remove bin, obj, and AppPackages directories
  Get-ChildItem -Path $projectPath -Directory -Filter "bin" -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
  Get-ChildItem -Path $projectPath -Directory -Filter "obj" -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
  if (Test-Path $appPackagesDir) { Remove-Item $appPackagesDir -Recurse -Force -ErrorAction SilentlyContinue }

  # Remove bundle output
  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  if (Test-Path $bundleOutputDir) { Remove-Item $bundleOutputDir -Recurse -Force -ErrorAction SilentlyContinue }
    
  Write-Host "Clean completed." -ForegroundColor Green
}

# Restore NuGet packages
Task Restore -Description "Restore NuGet packages" {
  Write-Host "Restoring NuGet packages..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  & dotnet restore $SolutionFile --verbosity normal
    
  if ($LASTEXITCODE -ne 0) {
    throw "Restore failed with exit code $LASTEXITCODE"
  }
  Pop-Location
    
  Write-Host "Restore completed." -ForegroundColor Green
}

# Build Debug
Task BuildDebug -Depends Restore -Description "Build Debug configuration" {
  $Configuration = "Debug"
  Start-Build -Configuration $Configuration
}

# Build Release
Task BuildRelease -Depends Restore -Description "Build Release configuration" {
  $Configuration = "Release"
  Start-Build -Configuration $Configuration
}

# Build (default to Debug)
Task Build -Depends BuildDebug -Description "Build solution (Debug by default)"


# Publish for specific platform
Task Publish -Depends BuildRelease -Description "Publish Release build" {
  Write-Host "Publishing Release build..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  foreach ($runtime in $runtimes) {
    Write-Host "Publishing for runtime: $runtime" -ForegroundColor Yellow
    & dotnet publish $csprojPath `
      --configuration Release `
      --runtime $runtime `
      --verbosity normal
        
    if ($LASTEXITCODE -ne 0) {
      throw "Publish failed for runtime '$runtime' with exit code $LASTEXITCODE"
    }
  }
  Pop-Location
    
  Write-Host "Publish completed." -ForegroundColor Green
}

# Build MSIX packages for each platform
Task BuildMsix -Depends Restore -Description "Build MSIX packages for x64 and ARM64" {
  Write-Host "Building MSIX packages..." -ForegroundColor Green

  # Get version and extension name from csproj
  $xml = [xml](Get-Content $csprojPath)
  $script:MsixVersion = $xml.Project.PropertyGroup.AppxPackageVersion | Select-Object -First 1
  $script:ExtensionName = $xml.Project.PropertyGroup.AppxPackageIdentityName | Select-Object -First 1
  Write-Host "Extension: $script:ExtensionName, Version: $script:MsixVersion" -ForegroundColor Yellow

  Push-Location $PSScriptRoot

  # Build x64 MSIX
  Write-Host "`n=== Building x64 MSIX ===" -ForegroundColor Cyan
  & dotnet build $csprojPath `
    --configuration Release `
    /p:GenerateAppxPackageOnBuild=true `
    /p:Platform=x64 `
    /p:AppxPackageDir="$appPackagesDir\x64\"
  if ($LASTEXITCODE -ne 0) {
    throw "MSIX build failed for x64 with exit code $LASTEXITCODE"
  }

  # Build ARM64 MSIX - use separate output dir so it doesn't overwrite x64
  Write-Host "`n=== Building ARM64 MSIX ===" -ForegroundColor Cyan
  & dotnet build $csprojPath `
    --configuration Release `
    /p:GenerateAppxPackageOnBuild=true `
    /p:Platform=ARM64 `
    /p:AppxPackageDir="$appPackagesDir\arm64\"
  if ($LASTEXITCODE -ne 0) {
    throw "MSIX build failed for ARM64 with exit code $LASTEXITCODE"
  }

  Pop-Location

  # Locate the generated .msix files
  $script:MsixX64 = Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix" |
    Where-Object { $_.FullName -match 'x64' -and $_.Name -match 'x64' } |
    Select-Object -First 1
  $script:MsixArm64 = Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix" |
    Where-Object { $_.FullName -match 'arm64' -and $_.Name -match 'arm64' } |
    Select-Object -First 1

  if (-not $script:MsixX64) {
    Write-Warning "x64 MSIX not found. Searching all .msix files under AppPackages..."
    Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix" | ForEach-Object { Write-Host "  Found: $($_.FullName)" }
    throw "Could not locate x64 .msix file"
  }
  if (-not $script:MsixArm64) {
    Write-Warning "ARM64 MSIX not found. Searching all .msix files under AppPackages..."
    Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix" | ForEach-Object { Write-Host "  Found: $($_.FullName)" }
    throw "Could not locate ARM64 .msix file"
  }

  Write-Host "`nx64 MSIX:   $($script:MsixX64.FullName)" -ForegroundColor Green
  Write-Host "ARM64 MSIX: $($script:MsixArm64.FullName)" -ForegroundColor Green
  Write-Host "MSIX build completed." -ForegroundColor Green
}

# Bundle MSIX packages into a single .msixbundle for Store submission
Task BundleMsix -Depends BuildMsix -Description "Create .msixbundle from x64 and ARM64 MSIX packages" {
  Write-Host "Creating MSIX bundle..." -ForegroundColor Green

  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  if (-not (Test-Path $bundleOutputDir)) {
    New-Item -ItemType Directory -Path $bundleOutputDir -Force | Out-Null
  }

  $bundleFileName = "${script:ExtensionName}_${script:MsixVersion}_Bundle.msixbundle"
  $bundlePath = Join-Path $bundleOutputDir $bundleFileName

  # Create bundle_mapping.txt
  $mappingFile = Join-Path $bundleOutputDir "bundle_mapping.txt"
  $mappingContent = @"
[Files]
"$($script:MsixX64.FullName)" "$($script:MsixX64.Name)"
"$($script:MsixArm64.FullName)" "$($script:MsixArm64.Name)"
"@
  $mappingContent | Out-File -FilePath $mappingFile -Encoding UTF8
  Write-Host "Bundle mapping written to: $mappingFile" -ForegroundColor Yellow

  # Find makeappx.exe
  $makeappxPath = $null

  # Check Windows SDK paths
  $sdkPaths = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
    "${env:ProgramFiles}\Windows Kits\10\bin"
  )

  foreach ($sdkBase in $sdkPaths) {
    if (Test-Path $sdkBase) {
      # Find the latest version
      $latestSdk = Get-ChildItem $sdkBase -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object Name -Descending |
        Select-Object -First 1
      if ($latestSdk) {
        $arch = switch ($env:PROCESSOR_ARCHITECTURE) {
          "AMD64" { "x64" }
          "x86" { "x86" }
          "ARM64" { "arm64" }
          default { "x64" }
        }
        $candidate = Join-Path $latestSdk.FullName "$arch\makeappx.exe"
        if (Test-Path $candidate) {
          $makeappxPath = $candidate
          break
        }
      }
    }
  }

  if (-not $makeappxPath) {
    # Fallback: search NuGet packages for makeappx from SDK BuildTools
    $nugetMakeappx = Get-ChildItem "$projectPath" -Recurse -Filter "makeappx.exe" -ErrorAction SilentlyContinue |
      Select-Object -First 1
    if ($nugetMakeappx) {
      $makeappxPath = $nugetMakeappx.FullName
    }
  }

  if (-not $makeappxPath) {
    throw "makeappx.exe not found. Install the Windows SDK or ensure Microsoft.Windows.SDK.BuildTools.MSIX NuGet package is restored."
  }

  Write-Host "Using makeappx: $makeappxPath" -ForegroundColor Yellow

  # Create the bundle
  & $makeappxPath bundle /v /f $mappingFile /p $bundlePath /o
  if ($LASTEXITCODE -ne 0) {
    throw "makeappx bundle failed with exit code $LASTEXITCODE"
  }

  $script:BundlePath = $bundlePath
  $sizeMB = [math]::Round((Get-Item $bundlePath).Length / 1MB, 2)
  Write-Host "`nMSIX bundle created: $bundlePath ($sizeMB MB)" -ForegroundColor Green
}

# Verify MSIX bundle was created
Task VerifyMsix -Description "Verify that the MSIX bundle was created successfully" {
  Write-Host "Verifying MSIX bundle..." -ForegroundColor Green

  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  $bundle = Get-ChildItem "$bundleOutputDir\*.msixbundle" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

  if ($bundle) {
    $sizeMB = [math]::Round($bundle.Length / 1MB, 2)
    Write-Host "Found MSIX bundle: $($bundle.Name) ($sizeMB MB)" -ForegroundColor Green
  } else {
    throw "No .msixbundle file found in $bundleOutputDir"
  }

  # Also verify individual MSIX files exist
  $msixFiles = Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix"
  Write-Host "Individual MSIX packages:" -ForegroundColor Yellow
  foreach ($msix in $msixFiles) {
    $sizeMB = [math]::Round($msix.Length / 1MB, 2)
    Write-Host "  $($msix.Name) ($sizeMB MB)" -ForegroundColor White
  }

  Write-Host "`nMSIX verification completed!" -ForegroundColor Green
}

# Self-sign MSIX bundle for local testing/sideloading (NOT needed for Store submission)
Task SelfSignMsix -Depends BundleMsix -Description "Self-sign MSIX bundle for local dev/testing (not for Store)" {
  Write-Host "Self-signing MSIX packages for local testing..." -ForegroundColor Green
  Write-Host "NOTE: This is for local sideloading only. The Microsoft Store signs packages automatically." -ForegroundColor Yellow

  # Read publisher CN from appxmanifest
  $manifestPath = Join-Path $projectPath "Package.appxmanifest"
  $manifest = [xml](Get-Content $manifestPath)
  $publisherCN = $manifest.Package.Identity.Publisher
  Write-Host "Publisher: $publisherCN" -ForegroundColor Yellow

  # Certificate parameters
  $certName = "ObsidianTaskNotes Dev Certificate"
  $certStorePath = "Cert:\LocalMachine\My"
  $pfxPath = Join-Path $PSScriptRoot "bin\Release\dev-signing.pfx"
  $pfxPassword = ConvertTo-SecureString -String "DevTestOnly" -Force -AsPlainText

  # Check for existing dev cert
  $existingCert = Get-ChildItem $certStorePath |
    Where-Object { $_.Subject -eq $publisherCN -and $_.FriendlyName -eq $certName } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

  if ($existingCert -and $existingCert.NotAfter -gt (Get-Date).AddDays(30)) {
    Write-Host "Using existing dev certificate (expires: $($existingCert.NotAfter))" -ForegroundColor Cyan
    $cert = $existingCert
  } else {
    Write-Host "Creating new self-signed certificate..." -ForegroundColor Cyan
    $cert = New-SelfSignedCertificate `
      -Type Custom `
      -Subject $publisherCN `
      -FriendlyName $certName `
      -KeyUsage DigitalSignature `
      -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3") `
      -CertStoreLocation $certStorePath `
      -NotAfter (Get-Date).AddYears(1)
    Write-Host "Certificate created, thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
  }

  # Export PFX for SignTool
  Export-PfxCertificate -Cert "$certStorePath\$($cert.Thumbprint)" `
    -FilePath $pfxPath -Password $pfxPassword

  Assert (Test-Path $pfxPath) "Failed to export PFX certificate"

  # Find SignTool.exe
  $signToolPath = $null
  $sdkPaths = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
    "${env:ProgramFiles}\Windows Kits\10\bin"
  )
  foreach ($sdkBase in $sdkPaths) {
    if (Test-Path $sdkBase) {
      $latestSdk = Get-ChildItem $sdkBase -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object Name -Descending |
        Select-Object -First 1
      if ($latestSdk) {
        $arch = switch ($env:PROCESSOR_ARCHITECTURE) {
          "AMD64" { "x64" }
          "x86" { "x86" }
          "ARM64" { "arm64" }
          default { "x64" }
        }
        $candidate = Join-Path $latestSdk.FullName "$arch\signtool.exe"
        if (Test-Path $candidate) {
          $signToolPath = $candidate
          break
        }
      }
    }
  }
  if (-not $signToolPath) {
    throw "SignTool.exe not found. Install the Windows SDK."
  }
  Write-Host "Using SignTool: $signToolPath" -ForegroundColor Yellow

  # Sign individual MSIX files
  $msixFiles = Get-ChildItem "$appPackagesDir" -Recurse -Filter "*.msix"
  foreach ($msix in $msixFiles) {
    Write-Host "Signing: $($msix.Name)" -ForegroundColor Cyan
    & $signToolPath sign /fd SHA256 /a /f $pfxPath /p "DevTestOnly" $msix.FullName
    if ($LASTEXITCODE -ne 0) {
      throw "Failed to sign $($msix.Name)"
    }
  }

  # Re-create the bundle from signed packages (bundle must be rebuilt after signing contents)
  Write-Host "`nRe-bundling with signed packages..." -ForegroundColor Cyan
  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  $bundleFileName = "${script:ExtensionName}_${script:MsixVersion}_Bundle.msixbundle"
  $bundlePath = Join-Path $bundleOutputDir $bundleFileName
  $mappingFile = Join-Path $bundleOutputDir "bundle_mapping.txt"

  # Find makeappx.exe (same logic as BundleMsix)
  $makeappxPath = $null
  foreach ($sdkBase in $sdkPaths) {
    if (Test-Path $sdkBase) {
      $latestSdk = Get-ChildItem $sdkBase -Directory |
        Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' } |
        Sort-Object Name -Descending |
        Select-Object -First 1
      if ($latestSdk) {
        $arch = switch ($env:PROCESSOR_ARCHITECTURE) {
          "AMD64" { "x64" }; "x86" { "x86" }; "ARM64" { "arm64" }; default { "x64" }
        }
        $candidate = Join-Path $latestSdk.FullName "$arch\makeappx.exe"
        if (Test-Path $candidate) { $makeappxPath = $candidate; break }
      }
    }
  }
  if (-not $makeappxPath) {
    throw "makeappx.exe not found. Install the Windows SDK."
  }

  # Remove old bundle and rebuild
  if (Test-Path $bundlePath) { Remove-Item $bundlePath -Force }
  & $makeappxPath bundle /v /f $mappingFile /p $bundlePath /o
  if ($LASTEXITCODE -ne 0) {
    throw "makeappx bundle failed with exit code $LASTEXITCODE"
  }

  # Sign the bundle itself
  Write-Host "Signing bundle: $bundleFileName" -ForegroundColor Cyan
  & $signToolPath sign /fd SHA256 /a /f $pfxPath /p "DevTestOnly" $bundlePath
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to sign bundle"
  }

  # Install cert to Trusted People store for sideloading
  $trustedPeopleStore = "Cert:\LocalMachine\TrustedPeople"
  $alreadyInstalled = Get-ChildItem $trustedPeopleStore -ErrorAction SilentlyContinue |
    Where-Object { $_.Thumbprint -eq $cert.Thumbprint }

  if (-not $alreadyInstalled) {
    Write-Host "`nInstalling certificate to Trusted People store (may require admin)..." -ForegroundColor Yellow
    try {
      Import-PfxCertificate -FilePath $pfxPath -Password $pfxPassword `
        -CertStoreLocation $trustedPeopleStore | Out-Null
      Write-Host "Certificate installed to Trusted People store." -ForegroundColor Green
    } catch {
      Write-Warning "Could not install cert to Trusted People store. Run as Administrator or manually import:"
      Write-Warning "  certutil -addstore TrustedPeople `"$pfxPath`""
    }
  } else {
    Write-Host "Certificate already in Trusted People store." -ForegroundColor Cyan
  }

  # Clean up PFX
  Remove-Item $pfxPath -Force -ErrorAction SilentlyContinue

  $sizeMB = [math]::Round((Get-Item $bundlePath).Length / 1MB, 2)
  Write-Host "`nSigned MSIX bundle ready for sideloading: $bundlePath ($sizeMB MB)" -ForegroundColor Green
  Write-Host "Install with: Add-AppxPackage -Path `"$bundlePath`"" -ForegroundColor Cyan
}

# Test task (placeholder for future unit tests)
Task Test -Depends Build -Description "Run unit tests" {
  Write-Host "Running tests..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  $testAssemblies = Get-ChildItem -Path $projectPath -Filter "*Tests.csproj" -Recurse
    
  if ($testAssemblies.Count -eq 0) {
    Write-Host "No test projects found." -ForegroundColor Yellow
  } else {
    foreach ($testAssembly in $testAssemblies) {
      Write-Host "Running tests in: $($testAssembly.Name)" -ForegroundColor Yellow
      & dotnet test $testAssembly.FullName --configuration $Configuration --verbosity normal
            
      if ($LASTEXITCODE -ne 0) {
        throw "Tests failed in $($testAssembly.Name) with exit code $LASTEXITCODE"
      }
    }
  }
  Pop-Location
    
  Write-Host "Tests completed." -ForegroundColor Green
}

# Analyze code
Task Analyze -Description "Run code analysis" {
  Write-Host "Running code analysis..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  & dotnet build $SolutionFile `
    --configuration Debug `
    --verbosity normal `
    /p:TreatWarningsAsErrors=true `
    /p:EnforceCodeStyleInBuild=true
    
  if ($LASTEXITCODE -ne 0) {
    throw "Code analysis failed with exit code $LASTEXITCODE"
  }
  Pop-Location
    
  Write-Host "Code analysis completed." -ForegroundColor Green
}

# Full CI pipeline
Task CI -Depends Clean, Restore, BuildDebug, Analyze, Test -Description "Run full CI pipeline" {
  Write-Host "CI pipeline completed successfully!" -ForegroundColor Green
}

# Full build and publish pipeline (MSIX for Microsoft Store)
Task Release -Depends Clean, Restore, BuildDebug, Analyze, Test, BundleMsix, VerifyMsix -Description "Create Release build with MSIX bundle for Store submission" {
  Write-Host "Release pipeline completed successfully!" -ForegroundColor Green
  Write-Host "MSIX bundle available in: bin\Release" -ForegroundColor Cyan
}

# MSIX-only build (skip tests/analysis for quick iteration)
Task ReleaseMsix -Depends Clean, BundleMsix, VerifyMsix -Description "Quick MSIX bundle build (skip tests)" {
  Write-Host "MSIX release build completed!" -ForegroundColor Green

  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  $bundle = Get-ChildItem "$bundleOutputDir\*.msixbundle" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if ($bundle) {
    Write-Host "Ready for Store upload: $($bundle.FullName)" -ForegroundColor Cyan
  }
}

# Format task
Task Format -Description "Format code using dotnet format" {
  Write-Host "Formatting code..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  & dotnet format $SolutionFile --verbosity diagnostic
    
  if ($LASTEXITCODE -ne 0) {
    Write-Host "Format completed with warnings." -ForegroundColor Yellow
  } else {
    Write-Host "Format completed." -ForegroundColor Green
  }
  Pop-Location
}

# Watch task for continuous development
Task Watch -Description "Watch for changes and rebuild (Debug)" {
  Write-Host "Starting watch mode... Press Ctrl+C to stop." -ForegroundColor Green
  Push-Location $PSScriptRoot
  try {
    & dotnet watch --project $csprojPath build --configuration Debug --verbosity normal
  } finally {
    Pop-Location
  }
}

# Build EXE Installer using Inno Setup
Task BuildExeInstaller -Depends Publish -Description "Build EXE installer for WinGet distribution" {
  param(
    [string]$Version
  )

  Write-Host "Building EXE installers..." -ForegroundColor Green
  
  # Get version from csproj if not provided
  if (-not $Version) {
    $xml = [xml](Get-Content $csprojPath)
    $Version = $xml.Project.PropertyGroup.AppxPackageVersion | Select-Object -First 1
    if (-not $Version) {
      $Version = "0.0.1.0"
      Write-Host "No version found in csproj, using default: $Version" -ForegroundColor Yellow
    }
  }
  Write-Host "Version: $Version" -ForegroundColor Yellow

  $setupTemplate = Join-Path $projectPath "setup-template.iss"
  $installerOutputDir = Join-Path $projectPath "bin\Release\installer"

  # Verify Inno Setup is installed
  $InnoSetupPath = "${env:ProgramFiles(x86)}\Inno Setup 6\iscc.exe"
  if (-not (Test-Path $InnoSetupPath)) {
    $InnoSetupPath = "${env:ProgramFiles}\Inno Setup 6\iscc.exe"
  }
  if (-not (Test-Path $InnoSetupPath)) {
    throw "Inno Setup 6 not found. Install from https://jrsoftware.org/isdl.php"
  }

  # Verify setup template exists
  if (-not (Test-Path $setupTemplate)) {
    throw "Setup template not found at: $setupTemplate"
  }

  # Create installer output directory
  if (-not (Test-Path $installerOutputDir)) {
    New-Item -ItemType Directory -Path $installerOutputDir -Force | Out-Null
  }

  $platforms = @("x64", "arm64")
  
  foreach ($platform in $platforms) {
    Write-Host "`n=== Building $platform installer ===" -ForegroundColor Cyan
    
    $publishDir = Join-Path $projectPath "bin\Release\net9.0-windows10.0.26100.0\win-$platform\publish"
    
    # Verify publish directory exists
    if (-not (Test-Path $publishDir)) {
      Write-Warning "Publish directory not found for $platform at: $publishDir"
      Write-Warning "Run 'Publish' task first"
      continue
    }

    # Read and customize setup script
    $setupScript = Get-Content $setupTemplate -Raw
    
    # Update version
    $setupScript = $setupScript -replace '#define AppVersion ".*"', "#define AppVersion `"$Version`""
    
    # Update output filename to include platform
    $setupScript = $setupScript -replace 'OutputBaseFilename=(.*?)\{#AppVersion\}', "OutputBaseFilename=`$1{#AppVersion}-$platform"
    
    # Update source path for the platform
    $setupScript = $setupScript -replace 'Source: "bin\\Release\\net9.0-windows10.0.26100.0\\win-x64\\publish', "Source: `"bin\Release\net9.0-windows10.0.26100.0\win-$platform\publish"
    
    # Add architecture settings
    if ($platform -eq "arm64") {
      $setupScript = $setupScript -replace '(\[Setup\][^\[]*)(MinVersion=)', "`$1ArchitecturesAllowed=arm64`r`nArchitecturesInstallIn64BitMode=arm64`r`n`$2"
    } else {
      $setupScript = $setupScript -replace '(\[Setup\][^\[]*)(MinVersion=)', "`$1ArchitecturesAllowed=x64compatible`r`nArchitecturesInstallIn64BitMode=x64compatible`r`n`$2"
    }

    #Update the License file with a resolved path
    $licensePath = Resolve-Path (Join-Path $projectPath "..\LICENSE.txt")
    $setupScript = $setupScript -replace 'LicenseFile=LICENSE.txt', "LicenseFile=`"$licensePath`""
    
    # Write platform-specific setup script
    $platformSetupPath = Join-Path $projectPath "setup-$platform.iss"
    $setupScript | Out-File -FilePath $platformSetupPath -Encoding UTF8
    
    # Build installer
    Write-Host "Creating $platform installer with Inno Setup..." -ForegroundColor Yellow
    Push-Location $PSScriptRoot
    & $InnoSetupPath $platformSetupPath
    Pop-Location
    
    if ($LASTEXITCODE -eq 0) {
      $installer = Get-ChildItem "$installerOutputDir\*-$platform.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
      if ($installer) {
        $sizeMB = [math]::Round($installer.Length / 1MB, 2)
        Write-Host "Created $platform installer: $($installer.Name) ($sizeMB MB)" -ForegroundColor Green
      }
    } else {
      Write-Warning "Inno Setup failed for $platform with exit code: $LASTEXITCODE"
    }
    Pop-Location
    
    # Clean up platform-specific setup script
    Remove-Item $platformSetupPath -ErrorAction SilentlyContinue
  }
  
  Write-Host "`nEXE installer build completed!" -ForegroundColor Green
  Write-Host "Installers available in: $installerOutputDir" -ForegroundColor Cyan
}

# Full release with EXE installers for GitHub/WinGet
Task ReleaseExe -Depends Clean, Restore, BuildDebug, Analyze, Test, BuildRelease, Publish, BuildExeInstaller -Description "Create Release build with EXE installers" {
  Write-Host "Release with EXE installers completed successfully!" -ForegroundColor Green
  
  $installerDir = Join-Path $projectPath "ObsidianTaskNotesExtension\bin\Release\installer"
  if (Test-Path $installerDir) {
    Write-Host "Installers available in: $installerDir" -ForegroundColor Cyan
    Get-ChildItem $installerDir -Filter "*.exe" | ForEach-Object {
      Write-Host "  - $($_.Name)" -ForegroundColor White
    }
  }
}

# Verify installers were created
Task VerifyInstallers -Description "Verify that EXE installers were created successfully" {
  Write-Host "Verifying installers..." -ForegroundColor Green
  
  $x64Installer = Get-ChildItem "$installerOutputDir\*-x64.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
  $arm64Installer = Get-ChildItem "$installerOutputDir\*-arm64.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
  
  $foundAny = $false
  
  if ($x64Installer) {
    $sizeMB = [math]::Round($x64Installer.Length / 1MB, 2)
    Write-Host "✓ Found x64 installer: $($x64Installer.Name) ($sizeMB MB)" -ForegroundColor Green
    $foundAny = $true
  } else {
    Write-Warning "✗ x64 installer not found"
  }
  
  if ($arm64Installer) {
    $sizeMB = [math]::Round($arm64Installer.Length / 1MB, 2)
    Write-Host "✓ Found ARM64 installer: $($arm64Installer.Name) ($sizeMB MB)" -ForegroundColor Green
    $foundAny = $true
  } else {
    Write-Warning "✗ ARM64 installer not found"
  }
  
  if (-not $foundAny) {
    throw "No installers were created! Check the BuildExeInstaller task output for errors."
  }
  
  Write-Host "`nInstaller verification completed!" -ForegroundColor Green
}

# CI/CD task - full pipeline with MSIX bundle and EXE installers
Task CICD -Depends Clean, Restore, BuildDebug, Analyze, Test, BundleMsix, VerifyMsix, Publish, BuildExeInstaller, VerifyInstallers -Description "Full CI/CD pipeline: MSIX bundle + EXE installers" {
  Write-Host "`n========================================" -ForegroundColor Cyan
  Write-Host "CI/CD Pipeline completed successfully!" -ForegroundColor Green
  Write-Host "========================================" -ForegroundColor Cyan

  $bundleOutputDir = Join-Path $PSScriptRoot "bin\Release"
  $bundle = Get-ChildItem "$bundleOutputDir\*.msixbundle" -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
  if ($bundle) {
    Write-Host "`nMSIX bundle (for Store): $($bundle.FullName)" -ForegroundColor Yellow
  }
  Write-Host "EXE installers (for GitHub/WinGet): $installerOutputDir" -ForegroundColor Yellow
}
