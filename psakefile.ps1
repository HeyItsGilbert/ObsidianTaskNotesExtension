# Obsidian Task Notes Extension - PSake Build File
# This file defines build, test, and deployment tasks using psake

# Task configuration
$projectPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension"
$solutionPath = Join-Path $PSScriptRoot "ObsidianTaskNotesExtension.sln"
$csprojPath = Join-Path $projectPath "ObsidianTaskNotesExtension" "ObsidianTaskNotesExtension.csproj"
$buildConfiguration = "Debug"
$runtimes = @("win-x64", "win-arm64")

# Properties to set
Properties {
  $Configuration = "Debug"
  $Platforms = @("x64", "ARM64")
  $SolutionFile = $solutionPath
  $ProjectDir = $projectPath
}

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
  Invoke-Build -Configuration $Configuration
}

# Build Release
Task BuildRelease -Depends Restore -Description "Build Release configuration" {
  $Configuration = "Release"
  Invoke-Build -Configuration $Configuration
}

# Build (default to Debug)
Task Build -Depends BuildDebug -Description "Build solution (Debug by default)"

# Helper function for building
function Invoke-Build {
  param(
    [string]$Configuration = "Debug"
  )
    
  Write-Host "Building solution with configuration: $Configuration" -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  & dotnet build $SolutionFile --configuration $Configuration --verbosity normal
    
  if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
  }
  Pop-Location
    
  Write-Host "Build completed." -ForegroundColor Green
}

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
  & dotnet publish $csprojPath `
    --configuration Release `
    --output bin/Release/msix `
    --verbosity normal
    
  if ($LASTEXITCODE -ne 0) {
    throw "MSIX packaging failed with exit code $LASTEXITCODE"
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
Task Release -Depends Clean, Restore, BuildRelease, Analyze, Test, Publish, PackageMsix -Description "Create Release build and package" {
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
