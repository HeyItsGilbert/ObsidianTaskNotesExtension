# Publishing Plan for ObsidianTaskNotesExtension

This document outlines the steps required to prepare the extension for publishing to the Microsoft Store and WinGet.

<!-- cspell:ignore Inno Winget commandpalette makeappx msixbundle wingetcreate -->

## Current Status

- ✅ Basic MSIX packaging support exists
- ✅ psake build system in place
- ✅ GitHub Actions workflow created for releases
- ⏳ Partner Center registration pending
- ⏳ Store assets need generation

---

## Phase 1: GitHub Releases (Current Focus)

### Completed Items

- [x] EXE installer task added to psakefile.ps1
- [x] Inno Setup script template created (setup-template.iss)
- [x] GitHub Action workflow for building releases
- [x] Build system calls build.ps1 with bootstrap flag

### Testing Checklist

- [ ] Run `.\build.ps1 -Bootstrap` to install dependencies
- [ ] Run `.\build.ps1 BuildExeInstaller` to build EXE installers
- [ ] Verify installers are created in `ObsidianTaskNotesExtension\bin\Release\installer\`
- [ ] Test GitHub Action workflow manually via workflow_dispatch

---

## Phase 2: Microsoft Store Preparation

### Prerequisites

- [ ] Register as Windows app developer in [Partner Center](https://partner.microsoft.com/dashboard/home)
- [ ] Reserve product name "Obsidian Task Notes"
- [ ] Copy identity values from Partner Center:
  - `Package/Identity/Name`: _________________
  - `Package/Identity/Publisher`: _________________
  - `Package/Properties/PublisherDisplayName`: _________________

### File Updates Required

#### 1. Package.appxmanifest

Update with Partner Center values:

```xml
<Identity
    Name="YOUR_PACKAGE_IDENTITY_NAME_HERE"
    Publisher="YOUR_PACKAGE_IDENTITY_PUBLISHER_HERE"
    Version="0.0.1.0" />

<Properties>
    <DisplayName>Obsidian Task Notes</DisplayName>
    <PublisherDisplayName>YOUR_PUBLISHER_DISPLAY_NAME_HERE</PublisherDisplayName>
    <Logo>Assets\StoreLogo.png</Logo>
</Properties>
```

#### 2. ObsidianTaskNotesExtension.csproj

Add to `<PropertyGroup>`:

```xml
<AppxPackageIdentityName>YOUR_PACKAGE_IDENTITY_NAME_HERE</AppxPackageIdentityName>
<AppxPackagePublisher>YOUR_PACKAGE_IDENTITY_PUBLISHER_HERE</AppxPackagePublisher>
<AppxPackageVersion>0.0.1.0</AppxPackageVersion>
```

### Asset Requirements

Generate all required icons using Visual Studio's asset generation tool:

| Asset | Size | Status |
|-------|------|--------|
| Square44x44Logo | 44×44 | ✅ Have scale-200 |
| SmallTile | 71×71 | ❌ Missing |
| Square150x150Logo | 150×150 | ✅ Have scale-200 |
| LargeTile | 310×310 | ❌ Missing |
| Wide310x150Logo | 310×150 | ✅ Have scale-200 |
| SplashScreen | 620×300 | ✅ Have scale-200 |
| StoreLogo | 50×50 | ✅ Have |

**Action:** Use Visual Studio > Project > Add New Item > App Icons to generate all required sizes.

### Build Commands for Store

```powershell
# Build MSIX for x64
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\x64\"

# Build MSIX for ARM64
dotnet build --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\ARM64\"

# Create bundle (requires makeappx.exe from Windows SDK)
makeappx bundle /f bundle_mapping.txt /p ObsidianTaskNotesExtension_Bundle.msixbundle
```

---

## Phase 3: WinGet Publishing

### Prerequisites

- [ ] GitHub CLI installed
- [ ] WingetCreate installed (`winget install Microsoft.WingetCreate`)
- [ ] First GitHub Release published with EXE installers

### WinGet Manifest Requirements

Add `windows-commandpalette-extension` tag to locale YAML:

```yaml
Tags:
- windows-commandpalette-extension
```

Add Windows App SDK dependency to installer YAML:

```yaml
Dependencies:
  PackageDependencies:
  - PackageIdentifier: Microsoft.WindowsAppRuntime.1.6
```

### First Submission (Manual)

```powershell
# Interactive submission with both architecture URLs
wingetcreate new "https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/releases/download/v0.0.1.0/ObsidianTaskNotesExtension-Setup-0.0.1.0-x64.exe" "https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/releases/download/v0.0.1.0/ObsidianTaskNotesExtension-Setup-0.0.1.0-arm64.exe"
```

### Automated Updates

After first submission, use the `update-winget.yml` workflow for automatic updates.

---

## Reference Links

- [PowerToys Command Palette Publishing Guide](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/publish-extension)
- [Partner Center](https://partner.microsoft.com/dashboard/home)
- [Visual Studio Asset Generation](https://learn.microsoft.com/en-us/windows/apps/design/style/iconography/visual-studio-asset-generation)
- [WinGet Package Submission](https://github.com/microsoft/winget-pkgs)
- [TaskNotes API Documentation](https://tasknotes.dev/HTTP_API/)

---

## Version History

| Version | Date | Notes |
|---------|------|-------|
| 0.0.1.0 | TBD | Initial release |
