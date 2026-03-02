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
                        "timeEstimate": 120
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
        task.TimeEstimate.Should().Be(120);
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
    public void PomodoroStatusResponse_DeserializesActiveSession()
    {
        var json = """
        {
            "success": true,
            "data": {
                "isRunning": true,
                "timeRemaining": 1200,
                "currentSession": {
                    "id": "session-1",
                    "type": "work",
                    "duration": 1500,
                    "startTime": "2025-01-15T10:00:00Z",
                    "task": {
                        "path": "TaskNotes/Tasks/Focus.md",
                        "title": "Focus Task",
                        "status": "todo"
                    }
                },
                "currentStreak": 3,
                "totalPomodoros": 5,
                "totalMinutesToday": 125
            }
        }
        """;

        var response = JsonSerializer.Deserialize<PomodoroStatusResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.IsRunning.Should().BeTrue();
        response.Data.TimeRemaining.Should().Be(1200);

        response.Data.CurrentSession.Should().NotBeNull();
        response.Data.CurrentSession!.Type.Should().Be("work");
        response.Data.CurrentSession.Duration.Should().Be(1500);
        response.Data.CurrentSession.StartTime.Should().Be("2025-01-15T10:00:00Z");

        response.Data.CurrentSession.Task.Should().NotBeNull();
        response.Data.CurrentSession.Task!.Path.Should().Be("TaskNotes/Tasks/Focus.md");
        response.Data.CurrentSession.Task.Title.Should().Be("Focus Task");

        response.Data.CurrentStreak.Should().Be(3);
        response.Data.TotalPomodoros.Should().Be(5);
        response.Data.TotalMinutesToday.Should().Be(125);
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
                        "id": "s1",
                        "type": "work",
                        "duration": 1500,
                        "startTime": "2025-01-15T09:00:00Z",
                        "endTime": "2025-01-15T09:25:00Z",
                        "task": {
                            "path": "TaskNotes/Tasks/Task1.md",
                            "title": "Task 1",
                            "status": "todo"
                        }
                    },
                    {
                        "id": "s2",
                        "type": "work",
                        "duration": 1500,
                        "startTime": "2025-01-15T10:00:00Z",
                        "endTime": "2025-01-15T10:25:00Z"
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
        session1.Type.Should().Be("work");
        session1.Duration.Should().Be(1500);

        var session2 = response.Data.Sessions[1];
        session2.Task.Should().BeNull();
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

    [Fact]
    public void NlpParseResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "success": true,
            "data": {
                "title": "Prepare report",
                "due": "2025-01-20",
                "priority": "1-urgent",
                "tags": ["work", "report"],
                "timeEstimate": 90,
                "details": "Need financial data"
            }
        }
        """;

        var response = JsonSerializer.Deserialize<NlpParseResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Title.Should().Be("Prepare report");
        response.Data.Due.Should().Be("2025-01-20");
        response.Data.Priority.Should().Be("1-urgent");
        response.Data.Tags.Should().BeEquivalentTo(["work", "report"]);
        response.Data.TimeEstimate.Should().Be(90);
        response.Data.Details.Should().Be("Need financial data");
    }

    [Fact]
    public void WebhookListResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "success": true,
            "data": [
                {
                    "id": "wh-1",
                    "url": "https://example.com/hook",
                    "events": ["task.created", "task.completed"],
                    "active": true,
                    "corsHeaders": false
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<WebhookListResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data![0].Id.Should().Be("wh-1");
        response.Data[0].Url.Should().Be("https://example.com/hook");
        response.Data[0].Events.Should().BeEquivalentTo(["task.created", "task.completed"]);
        response.Data[0].Active.Should().BeTrue();
        response.Data[0].CorsHeaders.Should().BeFalse();
    }

    [Fact]
    public void WebhookDeliveryListResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "success": true,
            "data": [
                {
                    "id": "del-1",
                    "webhookId": "wh-1",
                    "event": "task.created",
                    "status": "success",
                    "statusCode": 200,
                    "createdAt": "2025-01-15T10:05:00Z"
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<WebhookDeliveryListResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data![0].Event.Should().Be("task.created");
        response.Data[0].Status.Should().Be("success");
        response.Data[0].StatusCode.Should().Be(200);
    }

    [Fact]
    public void CalendarListResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "success": true,
            "data": [
                {"id": "cal-1", "name": "Work", "type": "google", "color": "#4285F4", "enabled": true},
                {"id": "cal-2", "name": "Personal", "type": "local", "enabled": false}
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<CalendarListResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Data![0].Name.Should().Be("Work");
        response.Data[0].Type.Should().Be("google");
        response.Data[0].Color.Should().Be("#4285F4");
        response.Data[0].Enabled.Should().BeTrue();
        response.Data[1].Enabled.Should().BeFalse();
    }

    [Fact]
    public void CalendarEventsResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "success": true,
            "data": [
                {
                    "id": "evt-1",
                    "title": "Team Meeting",
                    "start": "2025-01-15T09:00:00Z",
                    "end": "2025-01-15T10:00:00Z",
                    "allDay": false,
                    "calendarId": "cal-1",
                    "calendarName": "Work",
                    "description": "Weekly sync",
                    "location": "Conference Room A"
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<CalendarEventsResponse>(json, JsonOptions);

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(1);
        response.Data![0].Title.Should().Be("Team Meeting");
        response.Data[0].AllDay.Should().BeFalse();
        response.Data[0].CalendarId.Should().Be("cal-1");
        response.Data[0].Description.Should().Be("Weekly sync");
        response.Data[0].Location.Should().Be("Conference Room A");
    }
}
