# AI Coding Agent Instructions for ObsidianTaskNotesExtension

## Project Overview

**ObsidianTaskNotesExtension** is a Windows PowerToys Command Palette extension that integrates the TaskNotes HTTP API to manage tasks and notes from an Obsidian vault. It enables users to view, create, edit, complete, and archive tasks directly from the PowerToys Command Palette UI.

**Key Technologies:** C#, .NET 9 (net9.0-windows10.0.26100.0), Windows App SDK, WinRT, MSIX packaging

---

## Architecture & Data Flow

### Layered Architecture

1. **Entry Point:** [Program.cs](Program.cs) — COM server registration for PowerToys extension lifecycle
2. **Commands Provider:** [ObsidianTaskNotesExtensionCommandsProvider.cs](ObsidianTaskNotesExtensionCommandsProvider.cs) — Singleton that:
   - Initializes `SettingsManager` and `TaskNotesApiClient` as shared services
   - Creates all UI pages and wraps them as `CommandItem` instances
   - Returns command array to PowerToys on demand
3. **UI Pages:** [Pages/](Pages/) — Inherit from `DynamicListPage` or `ContentPage` (Command Palette framework)
4. **API Client:** [Services/TaskNotesApiClient.cs](Services/TaskNotesApiClient.cs) — HTTP wrapper around TaskNotes API
5. **Data Models:** [Models/](Models/) — JSON-serializable DTOs with `[JsonPropertyName]` attributes

### Critical Data Flow

- **Task Fetch:** Page → `TaskNotesApiClient.GetTasksAsync()` → HTTP GET to TaskNotes API → Response deserialized to `List<TaskItem>`
- **Task Update:** Command (e.g., toggle status) → `TaskNotesApiClient` method → HTTP request → Callback invokes `_refreshCallback` to reload page items
- **Settings Persistence:** `SettingsManager` (LocalSettings-based) ↔ Connection credentials, Pomodoro defaults

---

## Key Patterns & Conventions

### 1. Page Implementation Pattern
All dynamic list pages inherit from `DynamicListPage` and override `GetItems()`. Pages:
- Store filtered/sorted data in private lists (e.g., `_tasks`)
- Call `FetchTasksAsync()` in constructor to preload data
- Handle error display as list items (e.g., connection errors)
- Support search/filter via `_searchText` member
- Example: [Pages/ObsidianTaskNotesExtensionPage.cs](Pages/ObsidianTaskNotesExtensionPage.cs#L65-L95) filters tasks by title match

### 2. Command Pattern
Commands are nested classes within pages or standalone in [Commands/](Commands/):
- Inherit from `InvokableCommand`
- Receive dependencies (TaskItem, ApiClient, refresh callback) via constructor
- Override `Invoke()` to return `CommandResult.KeepOpen()` (UI stays open) or `CommandResult.CloseCommandPalette()`
- For async operations: fire `async Task` in `Invoke()` and call refresh callback on success
- Example: [Commands/ToggleTaskStatusCommand.cs](Commands/ToggleTaskStatusCommand.cs) — async toggle without blocking UI

### 3. API Client Pattern
`TaskNotesApiClient` methods:
- Precede calls with `ConfigureAuthHeader()` to inject Bearer token
- Use `JsonSerializer.Deserialize<T>(body, _jsonOptions)` for case-insensitive JSON parsing
- Include Debug.WriteLine logs for troubleshooting (e.g., response preview, deserialization errors)
- Return `(bool Success, string Message)` or typed objects; callers check `Success` flag
- Example: [Services/TaskNotesApiClient.cs](Services/TaskNotesApiClient.cs#L45-L62) test connection pattern

### 4. Settings Management
`SettingsManager` wraps Windows `ApplicationData.Current.LocalSettings`:
- Properties automatically read/write to LocalSettings.Values dictionary
- Used for API endpoint URL, auth token, and Pomodoro interval defaults
- Accessible from all services/pages via dependency injection
- No async locking; assumes light-weight JSON config

---

## External Dependencies & Integration Points

### TaskNotes HTTP API
- **Source of Truth:** https://tasknotes.dev/HTTP_API/
- **Common Endpoints:**
  - `GET /api/tasks` — Fetch tasks with optional filters (status, priority, tags, search)
  - `POST /api/tasks` — Create task
  - `PUT /api/tasks/{id}` — Update task (status, title, due date, etc.)
  - `DELETE /api/tasks/{id}` — Delete task
- **Auth:** Bearer token in `Authorization` header (from `SettingsManager.AuthToken`)
- **Response Format:** `{ "success": bool, "data": { "tasks": [...], "total": int, "filtered": int } }`

### PowerToys Command Palette Framework
- **Source of Truth:** https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensibility-overview
- **Lifecycle:** Extension started as COM server, `GetItems()` called on user input, commands dispatched via `Invoke()`
- **Pages:** Return `IListItem[]` from `GetItems()`; Command Palette re-renders on each call
- **Icons:** Use Unicode Segoe MDL2 symbols (e.g., `\uE73A` for checkbox) or `IconHelpers.FromRelativePath()` for PNG assets

### WinRT & COM Interop
- Extension implements `IExtension` interface (COM contract)
- Uses `Shmuelie.WinRTServer` to host COM server in-process
- MSIX packaging provides Windows installation/update mechanism

---

## Common Development Tasks

### Adding a New Task-Related Command
1. Create class in [Commands/](Commands/) inheriting `InvokableCommand`
2. Inject `TaskItem`, `TaskNotesApiClient`, and `Action refreshCallback`
3. In `Invoke()`, call API client method asynchronously and invoke callback on success
4. Register command in page's `GetItems()` method (e.g., `new CommandItem(new MyCommand(...))`)
5. Add corresponding API client method if calling new TaskNotes endpoint

### Adding a New Page
1. Create class in [Pages/](Pages/) inheriting `DynamicListPage` or `ContentPage`
2. Inject `TaskNotesApiClient` in constructor
3. Override `GetItems()` to return list items with associated commands
4. Wrap in `CommandItem` and register in [ObsidianTaskNotesExtensionCommandsProvider.cs](ObsidianTaskNotesExtensionCommandsProvider.cs#L26-L40)
5. Add title/subtitle labels to CommandItem

### Debugging API Issues
- Check `Debug.WriteLine()` output in Visual Studio Debug console
- Verify `SettingsManager.BaseUrl` and `AuthToken` are correct
- Test endpoint with curl/Postman against your TaskNotes server
- API client logs full response body on JSON deserialization failure

### Building & Publishing
- **Debug Build:** `dotnet build` (targets x64 and ARM64)
- **Release Build:** `dotnet publish -c Release` with PublishTrimmed=true
- **MSIX Package:** Visual Studio "Package and Publish" menu (enabled by `EnableMsixTooling=true` in .csproj)

---

## Code Quality & Constraints

- **Null Safety:** Project uses `<Nullable>enable</Nullable>`; avoid null reference exceptions with `?.` operator
- **Trim-Safe:** Release builds trim unused code; ensure public APIs that might be reflected have `[DynamicallyAccessedMembers]` attributes
- **AOT Warnings:** Treated as errors in Release; verify generic serialization remains AOT-compatible
- **Single Instance:** Extension is instantiated once by COM server; maintain stateless services or use thread-safe collections
- **Async Best Practices:** Avoid `.Wait()` or `.Result`; use `await` and `Task`-returning methods for responsive UI

---

## File Structure Quick Reference

| Path | Purpose |
|------|---------|
| `Program.cs` | COM server entry point |
| `ObsidianTaskNotesExtensionCommandsProvider.cs` | Command provider; creates & registers all pages |
| `Services/TaskNotesApiClient.cs` | HTTP wrapper around TaskNotes API |
| `Services/SettingsManager.cs` | Stores config (API URL, auth token) |
| `Pages/` | UI pages (list views, detail views, create/edit forms) |
| `Commands/` | Reusable command implementations |
| `Models/` | JSON DTOs (TaskItem, ApiResponse, etc.) |

---

## References

- **TaskNotes API Docs:** https://tasknotes.dev/HTTP_API/
- **PowerToys Command Palette Extensibility:** https://learn.microsoft.com/en-us/windows/powertoys/command-palette/extensibility-overview
- **Existing Guidance:** [CLAUDE.md](../CLAUDE.md), [AGENTS.md](../AGENTS.md)
