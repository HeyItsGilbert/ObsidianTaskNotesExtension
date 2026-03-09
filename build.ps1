# spell-checker:ignore nologo psake
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
  'PSReviewUnusedParameter',
  'Command',
  Justification = 'false positive'
)]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
  'PSReviewUnusedParameter',
  'Parameter',
  Justification = 'false positive'
)]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
  'PSReviewUnusedParameter',
  'CommandAst',
  Justification = 'false positive'
)]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
  'PSReviewUnusedParameter',
  'FakeBoundParams',
  Justification = 'false positive'
)]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
  'PSReviewUnusedParameter',
  'Help',
  Justification = 'false positive'
)]
[CmdletBinding(DefaultParameterSetName = 'Task')]
param(
  # Build task(s) to execute
  [Parameter(ParameterSetName = 'task', Position = 0)]
  [ArgumentCompleter( {
      param($Command, $Parameter, $WordToComplete, $CommandAst, $FakeBoundParams)
      try {
        Get-PSakeScriptTasks -BuildFile './psakefile.ps1' -ErrorAction 'Stop' |
          Where-Object { $_.Name -like "$WordToComplete*" } |
          Select-Object -ExpandProperty 'Name'
      } catch {
        # Silently fail if psake tasks can't be retrieved
        @()
      }
    })]
  [string[]]$Task = 'default',

  # Bootstrap dependencies
  [switch]$Bootstrap,

  # List available build tasks
  [Parameter(ParameterSetName = 'Help')]
  [switch]$Help,

  # Optional properties to pass to psake
  [hashtable]$Properties,

  # Optional parameters to pass to psake
  [hashtable]$Parameters
)

$ErrorActionPreference = 'Stop'

# Bootstrap dependencies
if ($Bootstrap.IsPresent) {
  PackageManagement\Get-PackageProvider -Name Nuget -ForceBootstrap | Out-Null
  Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
  if ((Test-Path -Path ./requirements.psd1)) {
    if (-not (Get-Module -Name PSDepend -ListAvailable)) {
      Install-Module -Name PSDepend -Repository PSGallery -Scope CurrentUser -Force
    }
    Import-Module -Name PSDepend -Verbose:$false
    Invoke-PSDepend -Path './requirements.psd1' -Install -Import -Force -WarningAction SilentlyContinue
  } else {
    Write-Warning 'No [requirements.psd1] found. Skipping build dependency installation.'
  }
}

# Execute psake task(s)
$psakeFile = './psakefile.ps1'
if ($PSCmdlet.ParameterSetName -eq 'Help') {
  Get-PSakeScriptTasks -BuildFile $psakeFile |
    Format-Table -Property Name, Description, Alias, DependsOn
} else {
  Invoke-psake -buildFile $psakeFile -taskList $Task -nologo -properties $Properties -parameters $Parameters
  exit ([int](-not $psake.build_success))
}