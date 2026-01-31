// Copyright (c) Gilbert Sanchez. All rights reserved.
// Licensed under the MIT License. See LICENSE file for details.

using System;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

/// <summary>
/// Service that resolves task icons based on user-configured mappings.
/// Uses the configured primary icon source, falling back to status if no match.
/// </summary>
public class IconMappingService
{
  private IconMappingConfig _config;

  /// <summary>
  /// Initializes a new instance with the specified configuration.
  /// </summary>
  public IconMappingService(IconMappingConfig config)
  {
    _config = config ?? throw new ArgumentNullException(nameof(config));
  }

  /// <summary>
  /// Gets or sets the current icon mapping configuration.
  /// </summary>
  public IconMappingConfig Config
  {
    get => _config;
    set => _config = value ?? throw new ArgumentNullException(nameof(value));
  }

  /// <summary>
  /// Resolves the appropriate icon for a task based on the configured primary source.
  /// Falls back to Status if the primary source has no match.
  /// </summary>
  /// <param name="task">The task to resolve an icon for.</param>
  /// <returns>An <see cref="IconInfo"/> for the task.</returns>
  public IconInfo ResolveIcon(TaskItem task)
  {
    ArgumentNullException.ThrowIfNull(task);

    // Try the primary source first
    var iconCode = ResolveFromSource(_config.PrimaryIconSource, task);

    // If primary source didn't match and it's not already Status, fall back to Status
    if (iconCode == null && _config.PrimaryIconSource != IconPriority.Status)
    {
      iconCode = ResolveStatusIcon(task);
    }

    // Return the resolved icon or the default
    return new IconInfo(iconCode ?? _config.DefaultIcon);
  }

  /// <summary>
  /// Resolves an icon from the specified source.
  /// </summary>
  private string? ResolveFromSource(IconPriority source, TaskItem task)
  {
    return source switch
    {
      IconPriority.Status => ResolveStatusIcon(task),
      IconPriority.Priority => ResolvePriorityIcon(task),
      IconPriority.Project => ResolveProjectIcon(task),
      IconPriority.Context => ResolveContextIcon(task),
      IconPriority.Tag => ResolveTagIcon(task),
      _ => null,
    };
  }

  /// <summary>
  /// Resolves an icon based on task status.
  /// Handles special computed states like overdue, archived, completed.
  /// </summary>
  private string? ResolveStatusIcon(TaskItem task)
  {
    // Check computed states first (most specific)
    if (task.IsOverdue && _config.StatusIcons.TryGetValue("overdue", out var overdueIcon))
    {
      return overdueIcon;
    }

    if (task.Archived && _config.StatusIcons.TryGetValue("archived", out var archivedIcon))
    {
      return archivedIcon;
    }

    if (task.Completed && _config.StatusIcons.TryGetValue("completed", out var completedIcon))
    {
      return completedIcon;
    }

    // Check raw status value
    if (!string.IsNullOrEmpty(task.Status) &&
        _config.StatusIcons.TryGetValue(task.Status, out var statusIcon))
    {
      return statusIcon;
    }

    return null;
  }

  /// <summary>
  /// Resolves an icon based on task priority.
  /// </summary>
  private string? ResolvePriorityIcon(TaskItem task)
  {
    if (string.IsNullOrEmpty(task.Priority))
    {
      return null;
    }

    if (_config.PriorityIcons.TryGetValue(task.Priority, out var icon))
    {
      return icon;
    }

    return null;
  }

  /// <summary>
  /// Resolves an icon based on task projects.
  /// Returns the first matching project icon found.
  /// </summary>
  private string? ResolveProjectIcon(TaskItem task)
  {
    if (task.Projects == null || task.Projects.Length == 0)
    {
      return null;
    }

    foreach (var project in task.Projects)
    {
      // Normalize: remove + prefix if present
      var normalizedProject = project.TrimStart('+');

      if (_config.ProjectIcons.TryGetValue(normalizedProject, out var icon))
      {
        return icon;
      }
    }

    return null;
  }

  /// <summary>
  /// Resolves an icon based on task contexts.
  /// Returns the first matching context icon found.
  /// </summary>
  private string? ResolveContextIcon(TaskItem task)
  {
    if (task.Contexts == null || task.Contexts.Length == 0)
    {
      return null;
    }

    foreach (var context in task.Contexts)
    {
      // Normalize: remove @ prefix if present
      var normalizedContext = context.TrimStart('@');

      if (_config.ContextIcons.TryGetValue(normalizedContext, out var icon))
      {
        return icon;
      }
    }

    return null;
  }

  /// <summary>
  /// Resolves an icon based on task tags.
  /// Returns the first matching tag icon found.
  /// </summary>
  private string? ResolveTagIcon(TaskItem task)
  {
    if (task.Tags == null || task.Tags.Length == 0)
    {
      return null;
    }

    foreach (var tag in task.Tags)
    {
      // Normalize: remove # prefix if present
      var normalizedTag = tag.TrimStart('#');

      if (_config.TagIcons.TryGetValue(normalizedTag, out var icon))
      {
        return icon;
      }
    }

    return null;
  }

  /// <summary>
  /// Creates a default IconMappingService with standard icon mappings.
  /// </summary>
  public static IconMappingService CreateDefault()
  {
    return new IconMappingService(new IconMappingConfig());
  }
}
