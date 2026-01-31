# CLAUDE.md

## Project Overview

ObsidianTaskNotesExtension is a Windows PowerToys Command Palette extension that integrates with the TaskNotes API to manage tasks and notes from an Obsidian vault.

## Key References

- **TaskNotes HTTP API**: https://tasknotes.dev/HTTP_API/ — source of truth for all TaskNotes API endpoints, request/response formats, and capabilities.
- **PowerToys Command Palette Extensibility**: https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensibility-overview — source of truth for how the Command Palette extension system works, including extension structure, commands provider, and lifecycle.

## Project Structure

- `ObsidianTaskNotesExtension.sln` — Solution file
- `ObsidianTaskNotesExtension/` — Main project
  - `Commands/` — Command palette command implementations
  - `Models/` — Data models
  - `Services/` — Service layer (API clients, etc.)
  - `Pages/` — UI pages
  - `ObsidianTaskNotesExtensionCommandsProvider.cs` — Main commands provider entry point
  - `Program.cs` — Application entry point
