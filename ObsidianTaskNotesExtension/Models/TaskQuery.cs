// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class TaskQueryFilter
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("project")]
    public string? Project { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("overdue")]
    public bool? Overdue { get; set; }

    [JsonPropertyName("completed")]
    public bool? Completed { get; set; }

    [JsonPropertyName("archived")]
    public bool? Archived { get; set; }

    [JsonPropertyName("due_before")]
    public string? DueBefore { get; set; }

    [JsonPropertyName("due_after")]
    public string? DueAfter { get; set; }

    [JsonPropertyName("sort")]
    public string? Sort { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
