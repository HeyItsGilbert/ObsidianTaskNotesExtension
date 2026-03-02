// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class TimeEntry
{
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }  // minutes

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}

public class ActiveSessionTask
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("projects")]
    public string[]? Projects { get; set; }
}

public class ActiveSessionInfo
{
    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("elapsedMinutes")]
    public int ElapsedMinutes { get; set; }
}

public class ActiveSession
{
    [JsonPropertyName("task")]
    public ActiveSessionTask? Task { get; set; }

    [JsonPropertyName("session")]
    public ActiveSessionInfo? Session { get; set; }

    [JsonPropertyName("elapsedMinutes")]
    public int ElapsedMinutes { get; set; }
}

public class TaskTimeData
{
    [JsonPropertyName("task")]
    public ActiveSessionTask? Task { get; set; }

    [JsonPropertyName("summary")]
    public TaskTimeSummary? Summary { get; set; }

    [JsonPropertyName("activeSession")]
    public ActiveSessionInfo? ActiveSession { get; set; }

    [JsonPropertyName("timeEntries")]
    public List<TimeEntry>? TimeEntries { get; set; }
}

public class TaskTimeSummary
{
    [JsonPropertyName("totalMinutes")]
    public int TotalMinutes { get; set; }

    [JsonPropertyName("totalHours")]
    public double TotalHours { get; set; }

    [JsonPropertyName("totalSessions")]
    public int TotalSessions { get; set; }

    [JsonPropertyName("completedSessions")]
    public int CompletedSessions { get; set; }

    [JsonPropertyName("activeSessions")]
    public int ActiveSessions { get; set; }

    [JsonPropertyName("averageSessionMinutes")]
    public double AverageSessionMinutes { get; set; }
}

public class TimeSummary
{
    [JsonPropertyName("period")]
    public string? Period { get; set; }

    [JsonPropertyName("dateRange")]
    public TimeSummaryDateRange? DateRange { get; set; }

    [JsonPropertyName("summary")]
    public TimeSummaryStats? Summary { get; set; }

    [JsonPropertyName("topTasks")]
    public List<TimeSummaryTaskEntry>? TopTasks { get; set; }

    [JsonPropertyName("topProjects")]
    public List<TimeSummaryProjectEntry>? TopProjects { get; set; }

    [JsonPropertyName("topTags")]
    public List<TimeSummaryTagEntry>? TopTags { get; set; }
}

public class TimeSummaryDateRange
{
    [JsonPropertyName("from")]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    public string? To { get; set; }
}

public class TimeSummaryStats
{
    [JsonPropertyName("totalMinutes")]
    public int TotalMinutes { get; set; }

    [JsonPropertyName("totalHours")]
    public double TotalHours { get; set; }

    [JsonPropertyName("tasksWithTime")]
    public int TasksWithTime { get; set; }

    [JsonPropertyName("activeTasks")]
    public int ActiveTasks { get; set; }

    [JsonPropertyName("completedTasks")]
    public int CompletedTasks { get; set; }
}

public class TimeSummaryTaskEntry
{
    [JsonPropertyName("task")]
    public string? Task { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }
}

public class TimeSummaryProjectEntry
{
    [JsonPropertyName("project")]
    public string? Project { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }
}

public class TimeSummaryTagEntry
{
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }
}

public class ActiveSessionsData
{
    [JsonPropertyName("activeSessions")]
    public List<ActiveSession>? ActiveSessions { get; set; }

    [JsonPropertyName("totalActiveSessions")]
    public int TotalActiveSessions { get; set; }

    [JsonPropertyName("totalElapsedMinutes")]
    public int TotalElapsedMinutes { get; set; }
}

public class ActiveSessionsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public ActiveSessionsData? Data { get; set; }
}

public class TaskTimeResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public TaskTimeData? Data { get; set; }
}

public class TimeSummaryResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public TimeSummary? Data { get; set; }
}
