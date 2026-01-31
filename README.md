# Obsidian TaskNotes Extension

A [PowerToys Run Command Palette](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensibility-overview) extension that integrates with the [TaskNotes](https://tasknotes.dev/) Obsidian plugin to manage tasks directly from your desktop.

## Features

- **View Tasks** — Browse active, completed, and archived tasks with priority and due date badges
- **Quick Actions** — Complete, archive, or delete tasks without leaving the command palette
- **Time Tracking** — Start/stop time tracking sessions on tasks
- **Pomodoro Timer** — Run focused work sessions with built-in pomodoro support
- **Statistics** — View task completion stats and time tracking summaries
- **Rich UI** — Colored tags for priority, due dates, projects, and custom labels

## Prerequisites

- Windows 10/11 with [PowerToys](https://github.com/microsoft/PowerToys) installed
- [Obsidian](https://obsidian.md/) with the [TaskNotes plugin](https://tasknotes.dev/) configured
- TaskNotes HTTP API enabled in your vault
- .NET 9 SDK (for building from source)

## Installation

### From Release

> [!NOTE] EXE & Untrusted Publishers
> MSIX and MS Store release are on the roadmap. Currently the exe is not signed by me.

1. Download the latest `.exe` package from [Releases](../../releases)
2. Double-click to install
3. Open PowerToys Command Palette and search for "Obsidian"

### From Source

```powershell
# Clone the repository
git clone https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension.git
cd ObsidianTaskNotesExtension

# Build and package
./build.ps1 Release
```

## Configuration

After installation, open the extension settings in PowerToys to configure:

- **API URL** — TaskNotes HTTP API endpoint (default: `http://localhost:27124`)
- **Auth Token** — Bearer token for API authentication (if enabled)
- **Icon Settings** — Customize task icons based on status, priority, project, context, or tags

See the [Icon Customization Guide](docs/ICON_CUSTOMIZATION.md) for detailed instructions on customizing task icons.

## Usage

1. Open PowerToys Command Palette (`Alt + Space` by default)
2. Type `Obsidian` or `Tasks` to see available commands
3. Use the following entry points:
   - **Tasks** — View and manage active tasks
   - **All Tasks** — View all tasks including completed/archived
   - **Pomodoro** — Start focus sessions
   - **Time Tracking** — View active timers and summaries
   - **Statistics** — Task and time tracking stats

## Building

This project uses [psake](https://github.com/psake/psake) for build automation via `build.ps1`.

### Quick Start

```powershell
# First time setup - install build dependencies
./build.ps1 -Bootstrap

# Build (Debug)
./build.ps1

# Build Release
./build.ps1 BuildRelease

# Full release pipeline (clean, build, test, package)
./build.ps1 Release
```

### Available Tasks

| Task | Description |
|------|-------------|
| `Build` | Build Debug configuration (default) |
| `BuildRelease` | Build Release configuration |
| `Clean` | Remove all build artifacts |
| `Restore` | Restore NuGet packages |
| `Publish` | Publish for x64 and ARM64 |
| `PackageMsix` | Create MSIX installer |
| `Test` | Run unit tests |
| `Analyze` | Run code analysis |
| `Format` | Format code with `dotnet format` |
| `Watch` | Rebuild on file changes |
| `CI` | Full CI pipeline |
| `Release` | Complete release pipeline |

```powershell
# List all available tasks
./build.ps1 -Help
```

### Development Workflow

```powershell
# Watch mode - auto-rebuild on changes
./build.ps1 Watch

# Run CI checks before committing
./build.ps1 CI
```

## Project Structure

```md
ObsidianTaskNotesExtension/
├── Commands/       # Command implementations (toggle, archive, delete, etc.)
├── Helpers/        # Tag and UI helper utilities
├── Models/         # Data models for TaskNotes API
├── Pages/          # Command Palette page implementations
├── Services/       # API client and settings manager
└── Assets/         # Icons and resources
```

## Resources

- [TaskNotes HTTP API Documentation](https://tasknotes.dev/HTTP_API/)
- [PowerToys Command Palette Extensibility](https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensibility-overview)
- [Segoe MDL2 Assets Icons](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)
- [psake Build Tool](https://github.com/psake/psake)

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgments

- Icon by [Freepik](https://www.flaticon.com/free-icon/check-list_7246748) on Flaticon
