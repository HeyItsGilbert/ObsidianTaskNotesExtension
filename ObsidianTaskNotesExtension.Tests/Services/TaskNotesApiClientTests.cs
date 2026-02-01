// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Reflection;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;
using ObsidianTaskNotesExtension.Tests.Helpers;

namespace ObsidianTaskNotesExtension.Tests.Services;

public class TaskNotesApiClientTests : IDisposable
{
  private readonly MockHttpMessageHandler _mockHandler;
  private readonly HttpClient _httpClient;
  private readonly TaskNotesApiClient _apiClient;

  public TaskNotesApiClientTests()
  {
    _mockHandler = new MockHttpMessageHandler();
    _httpClient = new HttpClient(_mockHandler);

    // Create a mock SettingsManager by using a real one with defaults
    var settingsManager = new SettingsManager();

    // Create the API client
    _apiClient = new TaskNotesApiClient(settingsManager);

    // Use reflection to replace the internal HttpClient with our mock
    var httpClientField = typeof(TaskNotesApiClient).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
    httpClientField?.SetValue(_apiClient, _httpClient);
  }

  public void Dispose()
  {
    _apiClient.Dispose();
    _httpClient.Dispose();
    GC.SuppressFinalize(this);
  }

  [Fact]
  public async Task TestConnectionAsync_ReturnsSuccess_WhenHealthEndpointReturns200()
  {
    _mockHandler.SetupResponse("/api/health", HttpStatusCode.OK, "{\"status\": \"ok\"}");

    var (success, message) = await _apiClient.TestConnectionAsync();

    success.Should().BeTrue();
    message.Should().Be("Connection successful!");
  }

  [Fact]
  public async Task TestConnectionAsync_ReturnsFailure_WhenHealthEndpointReturns500()
  {
    _mockHandler.SetupResponse("/api/health", HttpStatusCode.InternalServerError, "");

    var (success, message) = await _apiClient.TestConnectionAsync();

    success.Should().BeFalse();
    message.Should().Contain("500");
  }

  [Fact]
  public async Task GetActiveTasksAsync_ReturnsFilteredTasks()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "tasks": [
                    {"path": "task1.md", "title": "Active Task", "status": "todo"},
                    {"path": "task2.md", "title": "Completed Task", "status": "done"},
                    {"path": "task3.md", "title": "Archived Task", "status": "archived"}
                ],
                "total": 3,
                "filtered": 3
            }
        }
        """;
    _mockHandler.SetupResponse("/api/tasks", HttpStatusCode.OK, responseJson);

    var tasks = await _apiClient.GetActiveTasksAsync();

    // Should only include todo task (not completed or archived, unless completed today)
    tasks.Should().HaveCount(1);
    tasks[0].Title.Should().Be("Active Task");
  }

  [Fact]
  public async Task GetAllTasksAsync_ReturnsAllTasks()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "tasks": [
                    {"path": "task1.md", "title": "Task 1", "status": "todo"},
                    {"path": "task2.md", "title": "Task 2", "status": "done"},
                    {"path": "task3.md", "title": "Task 3", "status": "archived"}
                ],
                "total": 3,
                "filtered": 3
            }
        }
        """;
    _mockHandler.SetupResponse("/api/tasks", HttpStatusCode.OK, responseJson);

    var tasks = await _apiClient.GetAllTasksAsync();

    tasks.Should().HaveCount(3);
  }

  [Fact]
  public async Task GetAllTasksAsync_ReturnsEmptyList_WhenApiFails()
  {
    _mockHandler.SetupResponse("/api/tasks", HttpStatusCode.InternalServerError, "");

    var tasks = await _apiClient.GetAllTasksAsync();

    tasks.Should().BeEmpty();
  }

  [Fact]
  public async Task GetTaskAsync_ReturnsTask_WhenFound()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "path": "TaskNotes/Tasks/Test.md",
                "title": "Test Task",
                "status": "todo"
            }
        }
        """;
    _mockHandler.SetupResponse("/api/tasks/", HttpStatusCode.OK, responseJson);

    var task = await _apiClient.GetTaskAsync("TaskNotes/Tasks/Test.md");

    task.Should().NotBeNull();
    task!.Title.Should().Be("Test Task");
  }

  [Fact]
  public async Task GetTaskAsync_ReturnsNull_WhenNotFound()
  {
    _mockHandler.SetupResponse(HttpStatusCode.NotFound, "{\"success\": false, \"error\": \"Not found\"}");

    var task = await _apiClient.GetTaskAsync("nonexistent.md");

    task.Should().BeNull();
  }

  [Fact]
  public async Task CreateTaskAsync_ReturnsCreatedTask()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "path": "TaskNotes/Tasks/New Task.md",
                "title": "New Task",
                "status": "todo"
            }
        }
        """;
    _mockHandler.SetupResponse("/api/tasks", HttpStatusCode.OK, responseJson);

    var request = new CreateTaskRequest { Title = "New Task" };
    var task = await _apiClient.CreateTaskAsync(request);

    task.Should().NotBeNull();
    task!.Title.Should().Be("New Task");
    task.Path.Should().Be("TaskNotes/Tasks/New Task.md");
  }

  [Fact]
  public async Task UpdateTaskAsync_ReturnsUpdatedTask()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "path": "TaskNotes/Tasks/Test.md",
                "title": "Updated Title",
                "status": "done"
            }
        }
        """;
    _mockHandler.SetupResponse("/api/tasks/", HttpStatusCode.OK, responseJson);

    var request = new UpdateTaskRequest { Title = "Updated Title", Status = "done" };
    var task = await _apiClient.UpdateTaskAsync("TaskNotes/Tasks/Test.md", request);

    task.Should().NotBeNull();
    task!.Title.Should().Be("Updated Title");
    task.Status.Should().Be("done");
  }

  [Fact]
  public async Task UpdateTaskAsync_UrlEncodesTaskId()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "path": "TaskNotes/Tasks/My Task.md",
                "title": "My Task",
                "status": "todo"
            }
        }
        """;
    _mockHandler.SetupResponse(HttpStatusCode.OK, responseJson);

    var request = new UpdateTaskRequest { Title = "My Task" };
    await _apiClient.UpdateTaskAsync("TaskNotes/Tasks/My Task.md", request);

    // Verify the URL was properly encoded (slashes are encoded)
    _mockHandler.Requests.Should().HaveCount(1);
    var requestUrl = _mockHandler.Requests[0].RequestUri?.ToString();
    requestUrl.Should().Contain("TaskNotes%2FTasks%2F");
  }

  [Fact]
  public async Task ToggleStatusAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/toggle-status", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.ToggleStatusAsync("test.md");

    result.Should().BeTrue();
  }

  [Fact]
  public async Task ArchiveTaskAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/archive", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.ArchiveTaskAsync("test.md");

    result.Should().BeTrue();
  }

  [Fact]
  public async Task StartTimeTrackingAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/time/start", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.StartTimeTrackingAsync("test.md");

    result.Should().BeTrue();
  }

  [Fact]
  public async Task StopTimeTrackingAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/time/stop", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.StopTimeTrackingAsync("test.md");

    result.Should().BeTrue();
  }

  [Fact]
  public async Task GetStatsAsync_ReturnsStats()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "total": 50,
                "active": 20,
                "completed": 25,
                "overdue": 3,
                "archived": 5,
                "withTimeTracking": 10
            }
        }
        """;
    _mockHandler.SetupResponse("/api/stats", HttpStatusCode.OK, responseJson);

    var stats = await _apiClient.GetStatsAsync();

    stats.Should().NotBeNull();
    stats!.Total.Should().Be(50);
    stats.Active.Should().Be(20);
    stats.Completed.Should().Be(25);
    stats.Overdue.Should().Be(3);
  }

  [Fact]
  public async Task GetTimeStatsAsync_ReturnsTimeStats()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "totalMinutes": 360.5
            }
        }
        """;
    _mockHandler.SetupResponse("/api/time-stats", HttpStatusCode.OK, responseJson);

    var stats = await _apiClient.GetTimeStatsAsync();

    stats.Should().NotBeNull();
    stats!.TotalMinutes.Should().Be(360.5);
  }

  [Fact]
  public async Task GetTimeStatsAsync_IncludesQueryParameters()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "totalMinutes": 120.0
            }
        }
        """;
    _mockHandler.SetupResponse(HttpStatusCode.OK, responseJson);

    await _apiClient.GetTimeStatsAsync(range: "week", start: "2025-01-01", end: "2025-01-07");

    _mockHandler.Requests.Should().HaveCount(1);
    var requestUrl = _mockHandler.Requests[0].RequestUri?.ToString();
    requestUrl.Should().Contain("range=week");
    requestUrl.Should().Contain("start=2025-01-01");
    requestUrl.Should().Contain("end=2025-01-07");
  }

  [Fact]
  public async Task StartPomodoroAsync_ReturnsSession()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "taskId": "test.md",
                "state": "active",
                "timeRemaining": 1500,
                "duration": 1500
            }
        }
        """;
    _mockHandler.SetupResponse("/api/pomodoro/start", HttpStatusCode.OK, responseJson);

    var session = await _apiClient.StartPomodoroAsync("test.md");

    session.Should().NotBeNull();
    session!.State.Should().Be("active");
    session.TimeRemaining.Should().Be(1500);
  }

  [Fact]
  public async Task StopPomodoroAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/api/pomodoro/stop", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.StopPomodoroAsync();

    result.Should().BeTrue();
  }

  [Fact]
  public async Task PausePomodoroAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/api/pomodoro/pause", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.PausePomodoroAsync();

    result.Should().BeTrue();
  }

  [Fact]
  public async Task ResumePomodoroAsync_ReturnsTrue_WhenSuccessful()
  {
    _mockHandler.SetupResponse("/api/pomodoro/resume", HttpStatusCode.OK, "{\"success\": true}");

    var result = await _apiClient.ResumePomodoroAsync();

    result.Should().BeTrue();
  }

  [Fact]
  public async Task GetPomodoroStatusAsync_ReturnsStatus()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "active": true,
                "session": {
                    "taskId": "test.md",
                    "state": "active",
                    "timeRemaining": 1200
                },
                "timeRemaining": 1200
            }
        }
        """;
    _mockHandler.SetupResponse("/api/pomodoro/status", HttpStatusCode.OK, responseJson);

    var status = await _apiClient.GetPomodoroStatusAsync();

    status.Should().NotBeNull();
    status!.Active.Should().BeTrue();
    status.TimeRemaining.Should().Be(1200);
  }

  [Fact]
  public async Task GetPomodoroSessionsAsync_ReturnsSessions()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "sessions": [
                    {"taskId": "task1.md", "state": "completed"},
                    {"taskId": "task2.md", "state": "completed"}
                ],
                "total": 2
            }
        }
        """;
    _mockHandler.SetupResponse("/api/pomodoro/sessions", HttpStatusCode.OK, responseJson);

    var sessions = await _apiClient.GetPomodoroSessionsAsync(limit: 10);

    sessions.Should().HaveCount(2);
  }

  [Fact]
  public async Task GetPomodoroStatsAsync_ReturnsStats()
  {
    var responseJson = """
        {
            "success": true,
            "data": {
                "sessionsCompleted": 10,
                "totalFocusMinutes": 250.0,
                "currentStreak": 5,
                "averageSessionMinutes": 25.0
            }
        }
        """;
    _mockHandler.SetupResponse("/api/pomodoro/stats", HttpStatusCode.OK, responseJson);

    var stats = await _apiClient.GetPomodoroStatsAsync();

    stats.Should().NotBeNull();
    stats!.SessionsCompleted.Should().Be(10);
    stats.TotalFocusMinutes.Should().Be(250.0);
    stats.CurrentStreak.Should().Be(5);
  }

  [Fact]
  public void BuildObsidianUri_CreatesValidUri_WithVaultName()
  {
    // The SettingsManager uses defaults, so vault name will be empty
    // This test verifies the URI building logic
    var uri = _apiClient.BuildObsidianUri("TaskNotes/Tasks/Test.md");

    uri.Should().StartWith("obsidian://open?");
    uri.Should().Contain("file=");
    // HttpUtility.UrlEncode uses lowercase hex characters
    uri.Should().Contain("TaskNotes%2fTasks%2fTest");
    uri.Should().NotContain(".md"); // Extension should be stripped
  }

  [Fact]
  public void BuildObsidianUri_HandlesSpacesInPath()
  {
    var uri = _apiClient.BuildObsidianUri("TaskNotes/Tasks/My Task.md");

    // HttpUtility.UrlEncode encodes spaces as + (form encoding)
    uri.Should().Contain("My+Task");
  }
}