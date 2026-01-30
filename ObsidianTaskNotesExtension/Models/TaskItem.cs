// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class TaskItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("due")]
    public DateTime? Due { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("projects")]
    public string[]? Projects { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    public bool IsOverdue => Due.HasValue && Due.Value.Date < DateTime.Today && !Completed;

    public bool IsDueToday => Due.HasValue && Due.Value.Date == DateTime.Today;

    public bool IsDueTomorrow => Due.HasValue && Due.Value.Date == DateTime.Today.AddDays(1);
}
