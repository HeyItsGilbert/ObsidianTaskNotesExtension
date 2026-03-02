// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Models;

public class PomodoroSession
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }  // work, short-break, long-break

    [JsonPropertyName("duration")]
    public int Duration { get; set; }  // seconds

    [JsonPropertyName("startTime")]
    public string? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public string? EndTime { get; set; }

    [JsonPropertyName("task")]
    public TaskItem? Task { get; set; }
}

public class PomodoroStatus
{
    [JsonPropertyName("isRunning")]
    public bool IsRunning { get; set; }

    [JsonPropertyName("timeRemaining")]
    public int TimeRemaining { get; set; }

    [JsonPropertyName("currentSession")]
    public PomodoroSession? CurrentSession { get; set; }

    [JsonPropertyName("nextSessionType")]
    public string? NextSessionType { get; set; }

    [JsonPropertyName("totalPomodoros")]
    public int TotalPomodoros { get; set; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; set; }

    [JsonPropertyName("totalMinutesToday")]
    public int TotalMinutesToday { get; set; }
}

public class PomodoroStats
{
    [JsonPropertyName("totalSessions")]
    public int TotalSessions { get; set; }

    [JsonPropertyName("completedSessions")]
    public int CompletedSessions { get; set; }

    [JsonPropertyName("interruptedSessions")]
    public int InterruptedSessions { get; set; }

    [JsonPropertyName("totalFocusTime")]
    public int TotalFocusTime { get; set; }  // minutes

    [JsonPropertyName("workSessions")]
    public int WorkSessions { get; set; }

    [JsonPropertyName("breakSessions")]
    public int BreakSessions { get; set; }

    [JsonPropertyName("longestStreak")]
    public int LongestStreak { get; set; }

    [JsonPropertyName("averageSessionLength")]
    public double AverageSessionLength { get; set; }
}

public class PomodoroStatusResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PomodoroStatus? Data { get; set; }
}

public class PomodoroSessionsData
{
    [JsonPropertyName("sessions")]
    public List<PomodoroSession>? Sessions { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class PomodoroSessionsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PomodoroSessionsData? Data { get; set; }
}

public class PomodoroStatsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PomodoroStats? Data { get; set; }
}

public class PomodoroActionResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public PomodoroSession? Data { get; set; }
}
