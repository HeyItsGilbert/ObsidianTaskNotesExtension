// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class TimeEntry
{
    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public class TaskTimeData
{
    [JsonPropertyName("summary")]
    public TimeSummaryInfo? Summary { get; set; }

    [JsonPropertyName("activeSession")]
    public ActiveSession? ActiveSession { get; set; }

    [JsonPropertyName("timeEntries")]
    public List<TimeEntry>? TimeEntries { get; set; }
}

public class TimeSummaryInfo
{
    [JsonPropertyName("totalMinutes")]
    public double TotalMinutes { get; set; }

    [JsonPropertyName("entryCount")]
    public int EntryCount { get; set; }
}

public class ActiveSession
{
    [JsonPropertyName("taskId")]
    public string? TaskId { get; set; }

    [JsonPropertyName("taskTitle")]
    public string? TaskTitle { get; set; }

    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }
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
    public double TotalMinutes { get; set; }

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
    public double Minutes { get; set; }
}

public class TimeSummaryProjectEntry
{
    [JsonPropertyName("project")]
    public string? Project { get; set; }

    [JsonPropertyName("minutes")]
    public double Minutes { get; set; }
}

public class TimeSummaryTagEntry
{
    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("minutes")]
    public double Minutes { get; set; }
}

public class ActiveSessionsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<ActiveSession>? Data { get; set; }
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
