# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

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
