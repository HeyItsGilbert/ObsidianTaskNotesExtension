# Helper function for building
function Start-Build {
  param(
    [string]$Configuration = "Debug",
    [string]$Platform = "x64"
  )
    
  Write-Host "Building solution with configuration: $Configuration, platform: $Platform" -ForegroundColor Green
    
  Push-Location $PSScriptRoot
  & dotnet build $SolutionFile `
    --configuration $Configuration `
    --verbosity normal `
    /p:Platform=$Platform
    
  if ($LASTEXITCODE -ne 0) {
    throw "Build failed with exit code $LASTEXITCODE"
  }
  Pop-Location
    
  Write-Host "Build completed." -ForegroundColor Green
}