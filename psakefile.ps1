# Obsidian Task Notes Extension - PSake Build File
# This file defines build, test, and deployment tasks using psake
# spell-checker:ignore LASTEXITCODE csproj msix

# Task configuration
$projectPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension"
$solutionPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension.sln"
$csprojPath = Join-Path $projectPath "ObsidianTaskNotesExtension.csproj"
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
    
  # Remove bin and obj directories
  Get-ChildItem -Path $projectPath -Directory -Filter "bin" -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
  Get-ChildItem -Path $projectPath -Directory -Filter "obj" -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    
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

# Package MSIX
Task PackageMsix -Depends Publish -Description "Create MSIX package" {
  Write-Host "Creating MSIX package..." -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  foreach ($runtime in $runtimes) {
    Write-Host "Creating MSIX package for runtime: $runtime" -ForegroundColor Yellow
    & dotnet publish $csprojPath `
      --configuration Release `
      --runtime $runtime `
      --output bin/Release/msix/$runtime `
      --verbosity normal
      
    if ($LASTEXITCODE -ne 0) {
      throw "MSIX packaging failed for runtime '$runtime' with exit code $LASTEXITCODE"
    }
  }
  Pop-Location
    
  Write-Host "MSIX package created." -ForegroundColor Green
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

# Full build and publish pipeline
Task Release -Depends Clean, Restore, BuildDebug, Analyze, Test, BuildRelease, Publish, PackageMsix -Description "Create Release build and package" {
  Write-Host "Release pipeline completed successfully!" -ForegroundColor Green
  Write-Host "Outputs available in: bin/Release" -ForegroundColor Cyan
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
    $licensePath = Join-Path $projectPath "LICENSE.txt"
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
  
  $installerDir = Join-Path $projectPath "ObsidianTaskNotesExtension\bin\Release\installer"
  
  $x64Installer = Get-ChildItem "$installerDir\*-x64.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
  $arm64Installer = Get-ChildItem "$installerDir\*-arm64.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
  
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

# CI/CD task - alias for ReleaseExe with installer verification
Task CICD -Depends ReleaseExe, VerifyInstallers -Description "Full CI/CD pipeline: equivalent to ReleaseExe with installer verification" {
  Write-Host "`n========================================" -ForegroundColor Cyan
  Write-Host "CI/CD Pipeline completed successfully!" -ForegroundColor Green
  Write-Host "========================================" -ForegroundColor Cyan
  
  $installerDir = Join-Path $projectPath "ObsidianTaskNotesExtension\bin\Release\installer"
  Write-Host "`nReady for release. Installers at:" -ForegroundColor Yellow
  Write-Host "  $installerDir" -ForegroundColor White
}
