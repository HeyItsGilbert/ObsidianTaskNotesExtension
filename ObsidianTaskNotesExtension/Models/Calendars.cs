// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class CalendarInfo
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("type")]
  public string? Type { get; set; }

  [JsonPropertyName("color")]
  public string? Color { get; set; }

  [JsonPropertyName("enabled")]
  public bool Enabled { get; set; }
}

public class CalendarEvent
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("title")]
  public string? Title { get; set; }

  [JsonPropertyName("start")]
  public string? Start { get; set; }

  [JsonPropertyName("end")]
  public string? End { get; set; }

  [JsonPropertyName("allDay")]
  public bool AllDay { get; set; }

  [JsonPropertyName("calendarId")]
  public string? CalendarId { get; set; }

  [JsonPropertyName("calendarName")]
  public string? CalendarName { get; set; }

  [JsonPropertyName("description")]
  public string? Description { get; set; }

  [JsonPropertyName("location")]
  public string? Location { get; set; }
}

public class CalendarListResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public List<CalendarInfo>? Data { get; set; }
}

public class CalendarEventsResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public List<CalendarEvent>? Data { get; set; }
}
