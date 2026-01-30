// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

public class FilterOptions
{
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    [JsonPropertyName("projects")]
    public List<string>? Projects { get; set; }

    [JsonPropertyName("statuses")]
    public List<string>? Statuses { get; set; }

    [JsonPropertyName("priorities")]
    public List<string>? Priorities { get; set; }
}

public class FilterOptionsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public FilterOptions? Data { get; set; }
}
