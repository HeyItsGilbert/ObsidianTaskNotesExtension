# Helper function for building
function Start-Build {
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