// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

/// <summary>
/// Service that resolves task icons based on user-configured mappings.
/// Uses the configured primary icon source, falling back to status if no match.
/// Reads configuration from SettingsManager on each call to ensure updates are reflected immediately.
/// </summary>
public class IconMappingService
{
  private readonly SettingsManager? _settingsManager;
  private readonly IconMappingConfig? _testConfig;

  /// <summary>
  /// Initializes a new instance with the specified settings manager.
  /// Configuration is read from SettingsManager on each ResolveIcon call.
  /// </summary>
  public IconMappingService(SettingsManager settingsManager)
  {
    _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
    _testConfig = null;
  }

  /// <summary>
  /// Initializes a new instance with the specified configuration (for unit testing).
  /// </summary>
  /// <param name="config">The icon mapping configuration to use.</param>
  internal IconMappingService(IconMappingConfig config)
  {
    _testConfig = config ?? throw new ArgumentNullException(nameof(config));
    _settingsManager = null;
  }

  /// <summary>
  /// Gets the current icon mapping configuration from SettingsManager or test config.
  /// </summary>
  private IconMappingConfig Config => _testConfig ?? _settingsManager!.IconMappings;

  /// <summary>
  /// Resolves the appropriate icon for a task based on the configured primary source.
  /// Falls back to Status if the primary source has no match.
  /// </summary>
  /// <param name="task">The task to resolve an icon for.</param>
  /// <returns>An <see cref="IconInfo"/> for the task.</returns>
  public IconInfo ResolveIcon(TaskItem task)
  {
    ArgumentNullException.ThrowIfNull(task);

    var config = Config;

    // Try the primary source first
    var iconCode = ResolveFromSource(config.PrimaryIconSource, task, config);

    // If primary source didn't match and it's not already Status, fall back to Status
    if (iconCode == null && config.PrimaryIconSource != IconPriority.Status)
    {
      iconCode = ResolveStatusIcon(task, config);
    }

    // Return the resolved icon or the default
    return new IconInfo(iconCode ?? config.DefaultIcon);
  }

  /// <summary>
  /// Resolves an icon from the specified source.
  /// </summary>
  private static string? ResolveFromSource(IconPriority source, TaskItem task, IconMappingConfig config)
  {
    return source switch
    {
      IconPriority.Status => ResolveStatusIcon(task, config),
      IconPriority.Priority => ResolvePriorityIcon(task, config),
      IconPriority.Project => ResolveProjectIcon(task, config),
      IconPriority.Context => ResolveContextIcon(task, config),
      IconPriority.Tag => ResolveTagIcon(task, config),
      _ => null,
    };
  }

  /// <summary>
  /// Resolves an icon based on task status.
  /// Handles special computed states like overdue, archived, completed.
  /// </summary>
  private static string? ResolveStatusIcon(TaskItem task, IconMappingConfig config)
  {
    // Check computed states first (most specific)
    if (task.IsOverdue && config.StatusIcons.TryGetValue("overdue", out var overdueIcon))
    {
      return overdueIcon;
    }

    if (task.Archived && config.StatusIcons.TryGetValue("archived", out var archivedIcon))
    {
      return archivedIcon;
    }

    if (task.Completed && config.StatusIcons.TryGetValue("completed", out var completedIcon))
    {
      return completedIcon;
    }

    // Check raw status value
    if (!string.IsNullOrEmpty(task.Status) &&
        config.StatusIcons.TryGetValue(task.Status, out var statusIcon))
    {
      return statusIcon;
    }

    return null;
  }

  /// <summary>
  /// Resolves an icon based on task priority.
  /// </summary>
  private static string? ResolvePriorityIcon(TaskItem task, IconMappingConfig config)
  {
    if (string.IsNullOrEmpty(task.Priority))
    {
      return null;
    }

    return config.PriorityIcons.TryGetValue(task.Priority, out var icon) ? icon : null;
  }

  /// <summary>
  /// Resolves an icon based on task projects.
  /// Returns the first matching project icon found.
  /// </summary>
  private static string? ResolveProjectIcon(TaskItem task, IconMappingConfig config)
  {
    if (task.Projects == null || task.Projects.Length == 0)
    {
      return null;
    }

    return task.Projects
      .Select(project => project.TrimStart('+'))
      .Where(normalizedProject => config.ProjectIcons.ContainsKey(normalizedProject))
      .Select(normalizedProject => config.ProjectIcons[normalizedProject])
      .FirstOrDefault();
  }

  /// <summary>
  /// Resolves an icon based on task contexts.
  /// Returns the first matching context icon found.
  /// </summary>
  private static string? ResolveContextIcon(TaskItem task, IconMappingConfig config)
  {
    if (task.Contexts == null || task.Contexts.Length == 0)
    {
      return null;
    }

    return task.Contexts
      .Select(context => context.TrimStart('@'))
      .Where(normalizedContext => config.ContextIcons.ContainsKey(normalizedContext))
      .Select(normalizedContext => config.ContextIcons[normalizedContext])
      .FirstOrDefault();
  }

  /// <summary>
  /// Resolves an icon based on task tags.
  /// Returns the first matching tag icon found.
  /// </summary>
  private static string? ResolveTagIcon(TaskItem task, IconMappingConfig config)
  {
    if (task.Tags == null || task.Tags.Length == 0)
    {
      return null;
    }

    return task.Tags
      .Select(tag => tag.TrimStart('#'))
      .Where(normalizedTag => config.TagIcons.ContainsKey(normalizedTag))
      .Select(normalizedTag => config.TagIcons[normalizedTag])
      .FirstOrDefault();
  }
}
