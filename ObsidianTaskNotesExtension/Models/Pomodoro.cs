// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class PomodoroSession
{
    [JsonPropertyName("taskId")]
    public string? TaskId { get; set; }

    [JsonPropertyName("taskTitle")]
    public string? TaskTitle { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("timeRemaining")]
    public int? TimeRemaining { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("startedAt")]
    public string? StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public string? CompletedAt { get; set; }
}

public class PomodoroStatus
{
    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("session")]
    public PomodoroSession? Session { get; set; }

    [JsonPropertyName("timeRemaining")]
    public int? TimeRemaining { get; set; }

    [JsonPropertyName("statistics")]
    public PomodoroStats? Statistics { get; set; }
}

public class PomodoroStats
{
    [JsonPropertyName("sessionsCompleted")]
    public int SessionsCompleted { get; set; }

    [JsonPropertyName("totalFocusMinutes")]
    public double TotalFocusMinutes { get; set; }

    [JsonPropertyName("currentStreak")]
    public int CurrentStreak { get; set; }

    [JsonPropertyName("averageSessionMinutes")]
    public double AverageSessionMinutes { get; set; }
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
