// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Tests.Models;

public class RequestModelSerializationTests
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
  };

  [Fact]
  public void CreateTaskRequest_SerializesCorrectly()
  {
    var request = new CreateTaskRequest
    {
      Title = "Test Task",
      Priority = "2-high",
      Status = "todo",
      Due = "2025-01-15",
      Tags = ["work", "urgent"],
      Projects = ["ProjectA"],
      Contexts = ["@office"],
      Details = "Task details here",
      TimeEstimate = "2h"
    };

    var json = JsonSerializer.Serialize(request, JsonOptions);
    var parsed = JsonDocument.Parse(json);
    var root = parsed.RootElement;

    root.GetProperty("title").GetString().Should().Be("Test Task");
    root.GetProperty("priority").GetString().Should().Be("2-high");
    root.GetProperty("status").GetString().Should().Be("todo");
    root.GetProperty("due").GetString().Should().Be("2025-01-15");
    root.GetProperty("tags").GetArrayLength().Should().Be(2);
    root.GetProperty("projects").GetArrayLength().Should().Be(1);
    root.GetProperty("contexts").GetArrayLength().Should().Be(1);
    root.GetProperty("details").GetString().Should().Be("Task details here");
    root.GetProperty("timeEstimate").GetString().Should().Be("2h");
  }

  [Fact]
  public void CreateTaskRequest_OmitsNullProperties()
  {
    var request = new CreateTaskRequest
    {
      Title = "Minimal Task"
    };

    var json = JsonSerializer.Serialize(request, JsonOptions);
    var parsed = JsonDocument.Parse(json);
    var root = parsed.RootElement;

    root.GetProperty("title").GetString().Should().Be("Minimal Task");
    root.TryGetProperty("priority", out _).Should().BeFalse();
    root.TryGetProperty("tags", out _).Should().BeFalse();
    root.TryGetProperty("due", out _).Should().BeFalse();
  }

  [Fact]
  public void UpdateTaskRequest_SerializesOnlyProvidedFields()
  {
    var request = new UpdateTaskRequest
    {
      Title = "Updated Title",
      Status = "done"
    };

    var json = JsonSerializer.Serialize(request, JsonOptions);
    var parsed = JsonDocument.Parse(json);
    var root = parsed.RootElement;

    root.GetProperty("title").GetString().Should().Be("Updated Title");
    root.GetProperty("status").GetString().Should().Be("done");
    root.TryGetProperty("priority", out _).Should().BeFalse();
    root.TryGetProperty("due", out _).Should().BeFalse();
  }

  [Fact]
  public void UpdateTaskRequest_SerializesAllFields()
  {
    var request = new UpdateTaskRequest
    {
      Title = "Full Update",
      Priority = "1-urgent",
      Status = "done",
      Due = "2025-02-01",
      Scheduled = "2025-01-20",
      Tags = ["important"],
      Projects = ["MainProject"],
      Contexts = ["@home"],
      Details = "Updated details",
      TimeEstimate = "4h"
    };

    var json = JsonSerializer.Serialize(request, JsonOptions);
    var parsed = JsonDocument.Parse(json);
    var root = parsed.RootElement;

    root.GetProperty("title").GetString().Should().Be("Full Update");
    root.GetProperty("priority").GetString().Should().Be("1-urgent");
    root.GetProperty("status").GetString().Should().Be("done");
    root.GetProperty("due").GetString().Should().Be("2025-02-01");
    root.GetProperty("scheduled").GetString().Should().Be("2025-01-20");
    root.GetProperty("tags").GetArrayLength().Should().Be(1);
    root.GetProperty("projects").GetArrayLength().Should().Be(1);
    root.GetProperty("contexts").GetArrayLength().Should().Be(1);
    root.GetProperty("details").GetString().Should().Be("Updated details");
    root.GetProperty("timeEstimate").GetString().Should().Be("4h");
  }
}
