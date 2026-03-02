// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class NlpTextRequest
{
  [JsonPropertyName("text")]
  public string Text { get; set; } = string.Empty;
}

public class NlpParseResult
{
  [JsonPropertyName("title")]
  public string? Title { get; set; }

  [JsonPropertyName("due")]
  public string? Due { get; set; }

  [JsonPropertyName("scheduled")]
  public string? Scheduled { get; set; }

  [JsonPropertyName("priority")]
  public string? Priority { get; set; }

  [JsonPropertyName("tags")]
  public string[]? Tags { get; set; }

  [JsonPropertyName("projects")]
  public string[]? Projects { get; set; }

  [JsonPropertyName("contexts")]
  public string[]? Contexts { get; set; }

  [JsonPropertyName("timeEstimate")]
  public int? TimeEstimate { get; set; }

  [JsonPropertyName("details")]
  public string? Details { get; set; }
}

public class NlpParseResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public NlpParseResult? Data { get; set; }
}
