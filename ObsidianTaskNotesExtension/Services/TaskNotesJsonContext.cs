// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

// Request types for serialization (replacing anonymous objects)
public class DescriptionRequest
{
  [JsonPropertyName("description")]
  public string Description { get; set; } = string.Empty;
}

public class DateRequest
{
  [JsonPropertyName("date")]
  public string Date { get; set; } = string.Empty;
}

public class TaskIdRequest
{
  [JsonPropertyName("taskId")]
  public string TaskId { get; set; } = string.Empty;
}

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ApiResponse))]
[JsonSerializable(typeof(ApiData))]
[JsonSerializable(typeof(TaskItem))]
[JsonSerializable(typeof(List<TaskItem>))]
[JsonSerializable(typeof(SingleTaskResponse))]
[JsonSerializable(typeof(CreateTaskRequest))]
[JsonSerializable(typeof(UpdateTaskRequest))]
[JsonSerializable(typeof(GenericApiResponse))]
[JsonSerializable(typeof(TaskQueryFilter))]
[JsonSerializable(typeof(FilterOptions))]
[JsonSerializable(typeof(FilterOptionsResponse))]
[JsonSerializable(typeof(TaskStats))]
[JsonSerializable(typeof(TaskStatsResponse))]
[JsonSerializable(typeof(TimeStats))]
[JsonSerializable(typeof(TimeStatsResponse))]
[JsonSerializable(typeof(TimeEntry))]
[JsonSerializable(typeof(TaskTimeData))]
[JsonSerializable(typeof(TaskTimeResponse))]
[JsonSerializable(typeof(TimeSummaryInfo))]
[JsonSerializable(typeof(ActiveSession))]
[JsonSerializable(typeof(List<ActiveSession>))]
[JsonSerializable(typeof(ActiveSessionsData))]
[JsonSerializable(typeof(ActiveSessionsResponse))]
[JsonSerializable(typeof(TimeSummary))]
[JsonSerializable(typeof(TimeSummaryResponse))]
[JsonSerializable(typeof(TimeSummaryDateRange))]
[JsonSerializable(typeof(TimeSummaryStats))]
[JsonSerializable(typeof(TimeSummaryTaskEntry))]
[JsonSerializable(typeof(TimeSummaryProjectEntry))]
[JsonSerializable(typeof(TimeSummaryTagEntry))]
[JsonSerializable(typeof(PomodoroSession))]
[JsonSerializable(typeof(List<PomodoroSession>))]
[JsonSerializable(typeof(PomodoroSessionsData))]
[JsonSerializable(typeof(PomodoroStatus))]
[JsonSerializable(typeof(PomodoroStats))]
[JsonSerializable(typeof(PomodoroStatusResponse))]
[JsonSerializable(typeof(PomodoroSessionsResponse))]
[JsonSerializable(typeof(PomodoroStatsResponse))]
[JsonSerializable(typeof(PomodoroActionResponse))]
[JsonSerializable(typeof(DescriptionRequest))]
[JsonSerializable(typeof(DateRequest))]
[JsonSerializable(typeof(TaskIdRequest))]
[JsonSerializable(typeof(ExtensionSettings))]
internal sealed partial class TaskNotesJsonContext : JsonSerializerContext
{
}
