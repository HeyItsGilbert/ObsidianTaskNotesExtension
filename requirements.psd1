@{
  PSDependOptions = @{
    Target = 'CurrentUser'
  }
  'psake' = @{
    Version = '4.9.1'
  }
  # Install Inno Setup for building installers
  'InnoSetup' = @{
    DependencyType = 'Chocolatey'
    Version = '6.2.0'
  }
}