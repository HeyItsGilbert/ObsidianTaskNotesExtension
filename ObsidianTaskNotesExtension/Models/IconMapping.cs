// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ObsidianTaskNotesExtension.Models;

/// <summary>
/// Defines which task field is used as the primary source for icon selection.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<IconPriority>))]
public enum IconPriority
{
  /// <summary>Use task status (todo, done, archived, etc.).</summary>
  Status,

  /// <summary>Use task priority (urgent, high, medium, etc.).</summary>
  Priority,

  /// <summary>Use task project membership.</summary>
  Project,

  /// <summary>Use task context.</summary>
  Context,

  /// <summary>Use task tags.</summary>
  Tag,
}

/// <summary>
/// Configuration for task icon mappings. Maps status values, priorities, projects,
/// contexts, and tags to MDL2 icon codes.
/// </summary>
public class IconMappingConfig
{
  /// <summary>
  /// The primary field used to determine task icons.
  /// Falls back to Status if the primary source has no match.
  /// </summary>
  [JsonPropertyName("primaryIconSource")]
  public IconPriority PrimaryIconSource { get; set; } = IconPriority.Status;

  /// <summary>
  /// Maps task status values to MDL2 icon codes.
  /// Keys: "todo", "done", "completed", "archived", "in-progress", "overdue".
  /// </summary>
  [JsonPropertyName("statusIcons")]
  public Dictionary<string, string> StatusIcons { get; set; } = new(StringComparer.OrdinalIgnoreCase)
  {
    ["todo"] = "\uE73A",        // Checkbox
    ["done"] = "\uE73E",        // Checkmark
    ["completed"] = "\uE73E",   // Checkmark
    ["archived"] = "\uE7B8",    // Archive
    ["in-progress"] = "\uE916", // Play
    ["overdue"] = "\uE7BA",     // Warning
  };

  /// <summary>
  /// Maps priority values to MDL2 icon codes.
  /// </summary>
  [JsonPropertyName("priorityIcons")]
  public Dictionary<string, string> PriorityIcons { get; set; } = new(StringComparer.OrdinalIgnoreCase)
  {
    ["1-urgent"] = "\uE7C1",    // Important (exclamation)
    ["urgent"] = "\uE7C1",
    ["1"] = "\uE7C1",
    ["2-high"] = "\uE7C1",      // Important (exclamation) - distinct from urgent via color in API
    ["high"] = "\uE7C1",
    ["2"] = "\uE7C1",
    ["3-medium"] = "\uE8CB",    // Flag
    ["medium"] = "\uE8CB",
    ["3"] = "\uE8CB",
    ["4-normal"] = "\uE735",    // StarFilled
    ["normal"] = "\uE735",
    ["4"] = "\uE735",
    ["5-low"] = "\uE734",       // Star (outline)
    ["low"] = "\uE734",
    ["5"] = "\uE734",
  };

  /// <summary>
  /// Maps project names (without + prefix) to MDL2 icon codes.
  /// Example: "work" → "\uE821" (Folder).
  /// </summary>
  [JsonPropertyName("projectIcons")]
  public Dictionary<string, string> ProjectIcons { get; set; } = new(StringComparer.OrdinalIgnoreCase)
  {
    // Common project defaults
    ["work"] = "\uE821",        // Folder
    ["home"] = "\uE80F",        // Home
    ["personal"] = "\uE77B",    // Person
  };

  /// <summary>
  /// Maps context names (without @ prefix) to MDL2 icon codes.
  /// Example: "phone" → "\uE717" (Phone).
  /// </summary>
  [JsonPropertyName("contextIcons")]
  public Dictionary<string, string> ContextIcons { get; set; } = new(StringComparer.OrdinalIgnoreCase)
  {
    // Common context defaults
    ["phone"] = "\uE717",       // Phone
    ["email"] = "\uE715",       // Mail
    ["mail"] = "\uE715",        // Mail
    ["office"] = "\uE770",      // Work/Briefcase
    ["home"] = "\uE80F",        // Home
    ["computer"] = "\uE7F4",    // Devices
    ["online"] = "\uE774",      // Globe
  };

  /// <summary>
  /// Maps tag names to MDL2 icon codes.
  /// Example: "urgent" → "\uE7C1" (Important).
  /// </summary>
  [JsonPropertyName("tagIcons")]
  public Dictionary<string, string> TagIcons { get; set; } = new(StringComparer.OrdinalIgnoreCase)
  {
    // Common tag defaults
    ["urgent"] = "\uE7C1",      // Important
    ["bug"] = "\uE783",         // Error
    ["feature"] = "\uE945",     // Lightbulb
    ["idea"] = "\uE945",        // Lightbulb
    ["meeting"] = "\uE716",     // People
    ["waiting"] = "\uE823",     // Clock
  };

  /// <summary>
  /// Fallback icon when no mapping matches.
  /// </summary>
  [JsonPropertyName("defaultIcon")]
  public string DefaultIcon { get; set; } = "\uE73A"; // Checkbox

  /// <summary>
  /// Creates a deep copy of this configuration to avoid mutating shared state.
  /// </summary>
  public IconMappingConfig Clone()
  {
    return new IconMappingConfig
    {
      PrimaryIconSource = PrimaryIconSource,
      StatusIcons = new Dictionary<string, string>(StatusIcons, StringComparer.OrdinalIgnoreCase),
      PriorityIcons = new Dictionary<string, string>(PriorityIcons, StringComparer.OrdinalIgnoreCase),
      ProjectIcons = new Dictionary<string, string>(ProjectIcons, StringComparer.OrdinalIgnoreCase),
      ContextIcons = new Dictionary<string, string>(ContextIcons, StringComparer.OrdinalIgnoreCase),
      TagIcons = new Dictionary<string, string>(TagIcons, StringComparer.OrdinalIgnoreCase),
      DefaultIcon = DefaultIcon,
    };
  }
}

/// <summary>
/// Curated palette of commonly used MDL2 icons for the settings UI.
/// </summary>
public static class IconPalette
{
  /// <summary>
  /// Dictionary of friendly names to MDL2 unicode codes.
  /// </summary>
  public static readonly IReadOnlyDictionary<string, string> Icons = new Dictionary<string, string>
  {
    // Status icons
    ["Checkbox"] = "\uE73A",
    ["Checkmark"] = "\uE73E",
    ["CheckmarkCircle"] = "\uE73D",
    ["Warning"] = "\uE7BA",
    ["Error"] = "\uE783",
    ["Info"] = "\uE946",
    ["Question"] = "\uE897",

    // Priority icons
    ["Important"] = "\uE7C1",
    ["Flag"] = "\uE8CB",
    ["Star"] = "\uE734",
    ["StarFilled"] = "\uE735",
    ["Heart"] = "\uE8F1",

    // Action icons
    ["Play"] = "\uE916",
    ["Pause"] = "\uE769",
    ["Stop"] = "\uE71A",
    ["Sync"] = "\uE72C",
    ["Clock"] = "\uE823",

    // Organization icons
    ["Folder"] = "\uE821",
    ["Tag"] = "\uE8EC",
    ["Category"] = "\uE902",
    ["List"] = "\uE8FD",
    ["Archive"] = "\uE7B8",

    // Communication icons
    ["Phone"] = "\uE717",
    ["Mail"] = "\uE715",
    ["Chat"] = "\uE8F2",
    ["People"] = "\uE716",
    ["Person"] = "\uE77B",

    // Location/context icons
    ["Home"] = "\uE80F",
    ["Work"] = "\uE770",
    ["Globe"] = "\uE774",
    ["Location"] = "\uE81D",
    ["Car"] = "\uE804",

    // Misc icons
    ["Edit"] = "\uE70F",
    ["Delete"] = "\uE74D",
    ["Add"] = "\uE710",
    ["Remove"] = "\uE738",
    ["Settings"] = "\uE713",
    ["Calendar"] = "\uE787",
    ["Document"] = "\uE8A5",
    ["Code"] = "\uE943",
    ["Lightbulb"] = "\uE945",
    ["Rocket"] = "\uE7C8",
  };

  /// <summary>
  /// Gets the friendly name for a given MDL2 code, or null if not in palette.
  /// </summary>
  public static string? GetNameForCode(string code)
  {
    return Icons
      .Where(kvp => kvp.Value == code)
      .Select(kvp => kvp.Key)
      .FirstOrDefault();
  }

  /// <summary>
  /// Validates if a string is a valid MDL2 icon code (single unicode char in PUA range).
  /// </summary>
  public static bool IsValidIconCode(string? code)
  {
    if (string.IsNullOrEmpty(code) || code.Length != 1)
      return false;

    var c = code[0];
    // MDL2 icons are in Private Use Area: E000-F8FF
    return c >= '\uE000' && c <= '\uF8FF';
  }
}
