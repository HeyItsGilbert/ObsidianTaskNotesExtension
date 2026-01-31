// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Tests.Models;

public class ResponseModelDeserializationTests
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
  };

  [Fact]
  public void ApiResponse_DeserializesTaskList()
  {
    var json = """
        {
            "success": true,
            "data": {
                "tasks": [
                    {
                        "path": "TaskNotes/Tasks/Test.md",
                        "title": "Test Task",
                        "status": "todo",
                        "due": "2025-01-15",
                        "priority": "2-high",
                        "tags": ["work", "urgent"],
                        "projects": ["ProjectA"],
                        "contexts": ["@office"],
                        "details": "Some details",
                        "timeEstimate": "2h"
                    }
                ],
                "total": 1,
                "filtered": 1
            }
        }
        """;

    var response = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.Total.Should().Be(1);
    response.Data.Filtered.Should().Be(1);
    response.Data.Tasks.Should().HaveCount(1);

    var task = response.Data.Tasks![0];
    task.Path.Should().Be("TaskNotes/Tasks/Test.md");
    task.Title.Should().Be("Test Task");
    task.Status.Should().Be("todo");
    task.DueString.Should().Be("2025-01-15");
    task.Priority.Should().Be("2-high");
    task.Tags.Should().BeEquivalentTo(["work", "urgent"]);
    task.Projects.Should().BeEquivalentTo(["ProjectA"]);
    task.Contexts.Should().BeEquivalentTo(["@office"]);
    task.Details.Should().Be("Some details");
    task.TimeEstimate.Should().Be("2h");
  }

  [Fact]
  public void SingleTaskResponse_DeserializesCorrectly()
  {
    var json = """
        {
            "success": true,
            "data": {
                "path": "TaskNotes/Tasks/Single.md",
                "title": "Single Task",
                "status": "done",
                "due": null,
                "priority": "4-normal"
            }
        }
        """;

    var response = JsonSerializer.Deserialize<SingleTaskResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.Path.Should().Be("TaskNotes/Tasks/Single.md");
    response.Data.Title.Should().Be("Single Task");
    response.Data.Status.Should().Be("done");
    response.Data.DueString.Should().BeNull();
    response.Data.Priority.Should().Be("4-normal");
  }

  [Fact]
  public void TaskStatsResponse_DeserializesCorrectly()
  {
    var json = """
        {
            "success": true,
            "data": {
                "total": 100,
                "active": 45,
                "completed": 50,
                "overdue": 5,
                "archived": 10,
                "withTimeTracking": 20
            }
        }
        """;

    var response = JsonSerializer.Deserialize<TaskStatsResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.Total.Should().Be(100);
    response.Data.Active.Should().Be(45);
    response.Data.Completed.Should().Be(50);
    response.Data.Overdue.Should().Be(5);
    response.Data.Archived.Should().Be(10);
    response.Data.WithTimeTracking.Should().Be(20);
  }

  [Fact]
  public void TimeStatsResponse_DeserializesCorrectly()
  {
    var json = """
        {
            "success": true,
            "data": {
                "totalMinutes": 480.5
            }
        }
        """;

    var response = JsonSerializer.Deserialize<TimeStatsResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.TotalMinutes.Should().Be(480.5);
  }

  [Fact]
  public void PomodoroStatusResponse_DeserializesActiveSession()
  {
    var json = """
        {
            "success": true,
            "data": {
                "active": true,
                "session": {
                    "taskId": "TaskNotes/Tasks/Focus.md",
                    "taskTitle": "Focus Task",
                    "state": "active",
                    "timeRemaining": 1500,
                    "duration": 1500,
                    "startedAt": "2025-01-15T10:00:00Z"
                },
                "timeRemaining": 1200,
                "statistics": {
                    "sessionsCompleted": 5,
                    "totalFocusMinutes": 125.0,
                    "currentStreak": 3,
                    "averageSessionMinutes": 25.0
                }
            }
        }
        """;

    var response = JsonSerializer.Deserialize<PomodoroStatusResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.Active.Should().BeTrue();
    response.Data.TimeRemaining.Should().Be(1200);

    response.Data.Session.Should().NotBeNull();
    response.Data.Session!.TaskId.Should().Be("TaskNotes/Tasks/Focus.md");
    response.Data.Session.TaskTitle.Should().Be("Focus Task");
    response.Data.Session.State.Should().Be("active");
    response.Data.Session.TimeRemaining.Should().Be(1500);
    response.Data.Session.Duration.Should().Be(1500);

    response.Data.Statistics.Should().NotBeNull();
    response.Data.Statistics!.SessionsCompleted.Should().Be(5);
    response.Data.Statistics.TotalFocusMinutes.Should().Be(125.0);
    response.Data.Statistics.CurrentStreak.Should().Be(3);
    response.Data.Statistics.AverageSessionMinutes.Should().Be(25.0);
  }

  [Fact]
  public void PomodoroSessionsResponse_DeserializesSessionList()
  {
    var json = """
        {
            "success": true,
            "data": {
                "sessions": [
                    {
                        "taskId": "TaskNotes/Tasks/Task1.md",
                        "taskTitle": "Task 1",
                        "state": "completed",
                        "duration": 1500,
                        "startedAt": "2025-01-15T09:00:00Z",
                        "completedAt": "2025-01-15T09:25:00Z"
                    },
                    {
                        "taskId": null,
                        "taskTitle": null,
                        "state": "completed",
                        "duration": 1500,
                        "startedAt": "2025-01-15T10:00:00Z",
                        "completedAt": "2025-01-15T10:25:00Z"
                    }
                ],
                "total": 2
            }
        }
        """;

    var response = JsonSerializer.Deserialize<PomodoroSessionsResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data.Should().NotBeNull();
    response.Data!.Total.Should().Be(2);
    response.Data.Sessions.Should().HaveCount(2);

    var session1 = response.Data.Sessions![0];
    session1.TaskId.Should().Be("TaskNotes/Tasks/Task1.md");
    session1.State.Should().Be("completed");

    var session2 = response.Data.Sessions[1];
    session2.TaskId.Should().BeNull();
  }

  [Fact]
  public void ApiResponse_HandlesEmptyTaskList()
  {
    var json = """
        {
            "success": true,
            "data": {
                "tasks": [],
                "total": 0,
                "filtered": 0
            }
        }
        """;

    var response = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data!.Tasks.Should().BeEmpty();
    response.Data.Total.Should().Be(0);
  }

  [Fact]
  public void ApiResponse_IsCaseInsensitive()
  {
    var json = """
        {
            "Success": true,
            "Data": {
                "Tasks": [
                    {
                        "Path": "test.md",
                        "Title": "Test",
                        "Status": "todo"
                    }
                ],
                "Total": 1,
                "Filtered": 1
            }
        }
        """;

    var response = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Data!.Tasks.Should().HaveCount(1);
  }
}
