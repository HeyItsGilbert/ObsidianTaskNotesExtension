// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class TaskStats
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("active")]
    public int Active { get; set; }

    [JsonPropertyName("overdue")]
    public int Overdue { get; set; }

    [JsonPropertyName("archived")]
    public int Archived { get; set; }

    [JsonPropertyName("withTimeTracking")]
    public int WithTimeTracking { get; set; }
}

public class TimeStats
{
    [JsonPropertyName("totalMinutes")]
    public double TotalMinutes { get; set; }
}

public class TaskStatsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public TaskStats? Data { get; set; }
}

public class TimeStatsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public TimeStats? Data { get; set; }
}
