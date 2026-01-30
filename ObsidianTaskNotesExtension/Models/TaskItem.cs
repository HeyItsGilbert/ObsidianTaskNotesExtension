// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class ApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public ApiData? Data { get; set; }
}

public class ApiData
{
    [JsonPropertyName("tasks")]
    public List<TaskItem>? Tasks { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("filtered")]
    public int Filtered { get; set; }
}

public class TaskItem
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("due")]
    public string? DueString { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("projects")]
    public string[]? Projects { get; set; }

    [JsonPropertyName("scheduled")]
    public string? Scheduled { get; set; }

    [JsonPropertyName("dateCreated")]
    public string? DateCreated { get; set; }

    [JsonPropertyName("dateModified")]
    public string? DateModified { get; set; }

    // Computed properties
    [JsonIgnore]
    public string Id => Path;

    [JsonIgnore]
    public DateTime? Due => DateTime.TryParse(DueString, out var d) ? d : null;

    [JsonIgnore]
    public bool Completed => Status.Equals("done", StringComparison.OrdinalIgnoreCase)
                          || Status.Equals("completed", StringComparison.OrdinalIgnoreCase);

    [JsonIgnore]
    public bool Archived => Status.Equals("archived", StringComparison.OrdinalIgnoreCase);

    public bool IsOverdue => Due.HasValue && Due.Value.Date < DateTime.Today && !Completed;

    public bool IsDueToday => Due.HasValue && Due.Value.Date == DateTime.Today;

    public bool IsDueTomorrow => Due.HasValue && Due.Value.Date == DateTime.Today.AddDays(1);

    [JsonIgnore]
    public DateTime? Modified => DateTime.TryParse(DateModified, out var d) ? d : null;

    [JsonIgnore]
    public bool CompletedToday => Completed && Modified.HasValue && Modified.Value.Date == DateTime.Today;
}

public class SingleTaskResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public TaskItem? Data { get; set; }
}

public class CreateTaskRequest
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("due")]
    public string? Due { get; set; }

    [JsonPropertyName("scheduled")]
    public string? Scheduled { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("projects")]
    public string[]? Projects { get; set; }

    [JsonPropertyName("contexts")]
    public string[]? Contexts { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("timeEstimate")]
    public string? TimeEstimate { get; set; }
}

public class UpdateTaskRequest
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("due")]
    public string? Due { get; set; }

    [JsonPropertyName("scheduled")]
    public string? Scheduled { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("projects")]
    public string[]? Projects { get; set; }

    [JsonPropertyName("contexts")]
    public string[]? Contexts { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("timeEstimate")]
    public string? TimeEstimate { get; set; }
}

public class GenericApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}
