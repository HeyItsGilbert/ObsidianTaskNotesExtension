// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class WebhookConfig
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("url")]
  public string Url { get; set; } = string.Empty;

  [JsonPropertyName("events")]
  public string[] Events { get; set; } = [];

  [JsonPropertyName("active")]
  public bool Active { get; set; }

  [JsonPropertyName("transformFile")]
  public string? TransformFile { get; set; }

  [JsonPropertyName("corsHeaders")]
  public bool? CorsHeaders { get; set; }
}

public class WebhookDelivery
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("webhookId")]
  public string? WebhookId { get; set; }

  [JsonPropertyName("event")]
  public string? Event { get; set; }

  [JsonPropertyName("status")]
  public string? Status { get; set; }

  [JsonPropertyName("statusCode")]
  public int? StatusCode { get; set; }

  [JsonPropertyName("payload")]
  public string? Payload { get; set; }

  [JsonPropertyName("createdAt")]
  public string? CreatedAt { get; set; }
}

public class WebhookResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public WebhookConfig? Data { get; set; }
}

public class WebhookListResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public List<WebhookConfig>? Data { get; set; }
}

public class WebhookDeliveryListResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("data")]
  public List<WebhookDelivery>? Data { get; set; }
}
