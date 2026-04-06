# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## [Unreleased]

## [0.5.0] - 2026-04-06

### Added

- Daily Tasks view showing tasks scheduled or due today,
  with support for the new Dock feature (#11)
- Version display on the Settings page for easier
  troubleshooting
- Debug build indicator: distinct icon and "(Debug)" version
  suffix in debug builds

### Fixed

- Create Task crash: plugin crashed after successfully
  creating a task due to unhandled async exception and
  invalid GoBack navigation from a top-level form page;
  now shows a "Task created!" toast and dismisses cleanly
- Edit Task: added defensive error handling to prevent
  potential unobserved async exceptions

### Changed

- Assembly version now derived from AppxPackageVersion in
  the csproj for consistent runtime version reporting

## [0.4.1] - 2026-03-09

### Changed

- Package display name updated to
  "Task Notes Command Palette for Obsidian" per Microsoft Store
  requirements (affects Command Palette UI, manifest, and installer)
- `build.ps1` enhanced with tab completion for psake tasks and
  PSScriptAnalyzer suppression attributes for false positives

### Removed

- WinGet manifest update workflow (`winget.yml`)
- Old `UpdateChangelog.prompt.md` prompt (renamed to
  `UpdateChangelogAndVersion.prompt.md`)

## [0.4.0] 2026-03-02

### Added

- WinGet manifest update workflow for automated manifest updates on
  release publish with dynamic version and tag information retrieval
- NLP parsing and task creation methods in TaskNotesApiClient
- NLP and Webhook model support (NlpParseResponse, WebhookConfig)
- New API endpoints for managing webhooks and calendar events

### Changed

- Updated models to comply with OpenAPI JSON specification:
  Pomodoro, TimeTracking, Stats, and TaskItem models restructured
- Publisher name updated to "Hey! It's Gilbert!" in package manifest
- Enhanced JSON serialization context to support new models

## [0.3.0] 2026-02-27

### Added

- New application asset images for branding and Store submission
  (SmallTile, SplashScreen, Square150x150Logo, Wide310x150Logo,
  Screenshot, StoreLogo-1080)

### Changed

- Improved psakefile argument handling and output formatting for
  better build script maintainability
- Enhanced build script for asset preparation and MSIX packaging

## [0.2.1] 2026-02-27

### Fixed

- Publisher identity in MSIX package configuration and manifest

## [0.2.0] 2026-01-30

### Added

- Comprehensive unit test suite with 76 tests covering models,
  serialization, and API client functionality
- Test infrastructure with xUnit, FluentAssertions, and mocked
  HTTP message handler for isolated testing
- Missing properties to TaskItem model: contexts, details, and
  timeEstimate fields per OpenAPI specification

### Fixed

- API client URL encoding consistency: UpdateTaskAsync now
  properly encodes task IDs like other endpoints
- OpenAPI specification compliance for TaskItem model
- Test project now properly targets Windows platform

### Changed

- Added analyzer suppression for test method naming conventions
- Updated Directory.Packages.props with test package versions

## [0.1.0] 2026-01-30

### Added

- Time tracking integration for task management
- Pomodoro timer feature with customizable intervals
- Task statistics page
- Tag helpers for better task organization
- GitHub Actions workflow for building EXE installers
- Complete task CRUD operations (Create, Read, Update, Delete)
- Initial project setup with build scripts and documentation
- Search and filter capabilities for tasks

### Changed

- Code formatting improvements for consistency

## [0.0.1] Initial Release
