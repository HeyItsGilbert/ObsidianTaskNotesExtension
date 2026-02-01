// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Helpers;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

internal sealed partial class StatsPage : DynamicListPage
{
    private readonly TaskNotesApiClient _apiClient;
    private TaskStats? _taskStats;
    private TimeStats? _timeStats;
    private string? _errorMessage;

    public StatsPage(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        Icon = new IconInfo("\uE9D9"); // Chart icon
        Title = "Task Statistics";
        Name = "Statistics";

        FetchStatsAsync();
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        if (_errorMessage != null)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Error",
                Subtitle = _errorMessage,
                Icon = new IconInfo("\uE783"),
                Tags = [new Tag("Error") { Background = ColorHelpers.FromRgb(220, 53, 69), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
            return items.ToArray();
        }

        if (_taskStats != null)
        {
            // Total Tasks - highlight badge
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📋 Total Tasks",
                Subtitle = "All tasks in your vault",
                Icon = new IconInfo("\uE8EF"), // List icon
                Tags = [new Tag($"{_taskStats.Total}")
                {
                    Background = ColorHelpers.FromRgb(0, 123, 255),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.Total} total tasks"
                }]
            });

            // Active Tasks - green badge
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "🎯 Active Tasks",
                Subtitle = "Tasks in progress",
                Icon = new IconInfo("\uE73A"),
                Tags = [new Tag($"{_taskStats.Active}")
                {
                    Background = ColorHelpers.FromRgb(40, 167, 69),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.Active} active tasks"
                }]
            });

            // Completed Tasks - teal badge
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "✅ Completed Tasks",
                Subtitle = "Tasks you've finished",
                Icon = new IconInfo("\uE73E"),
                Tags = [new Tag($"{_taskStats.Completed}")
                {
                    Icon = new IconInfo("\uE73E"),
                    Background = ColorHelpers.FromRgb(32, 201, 151),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.Completed} completed tasks"
                }]
            });

            // Overdue Tasks - red badge (warning)
            var overdueColor = _taskStats.Overdue > 0
                ? ColorHelpers.FromRgb(220, 53, 69)  // Red for warning
                : ColorHelpers.FromRgb(108, 117, 125); // Gray if none
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "⚠️ Overdue Tasks",
                Subtitle = _taskStats.Overdue > 0 ? "Needs attention!" : "You're on track!",
                Icon = new IconInfo("\uE7BA"),
                Tags = [new Tag($"{_taskStats.Overdue}")
                {
                    Icon = _taskStats.Overdue > 0 ? new IconInfo("\uE7BA") : new IconInfo("\uE7E6"), // Warning/alert icon
                    Background = overdueColor,
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.Overdue} overdue tasks"
                }]
            });

            // Archived Tasks - gray badge
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📦 Archived Tasks",
                Subtitle = "Tasks moved to archive",
                Icon = new IconInfo("\uE7B8"),
                Tags = [new Tag($"{_taskStats.Archived}")
                {
                    Background = ColorHelpers.FromRgb(108, 117, 125),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.Archived} archived tasks"
                }]
            });

            // Tasks with Time Tracking - purple badge
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "⏱️ Tasks with Time Tracking",
                Subtitle = "Tasks you've tracked time on",
                Icon = new IconInfo("\uE823"),
                Tags = [new Tag($"{_taskStats.WithTimeTracking}")
                {
                    Icon = new IconInfo("\uE823"),
                    Background = ColorHelpers.FromRgb(102, 16, 242),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255),
                    ToolTip = $"{_taskStats.WithTimeTracking} tasks with time tracking"
                }]
            });
        }

        if (_timeStats != null)
        {
            var minutes = _timeStats.TotalMinutes;
            var timeTag = TagHelpers.CreateTimeTag(minutes, "Total");

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "🕐 Total Time Tracked",
                Subtitle = "Across all tasks",
                Icon = new IconInfo("\uE916"),
                Tags = [timeTag]
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Loading...",
                Icon = new IconInfo("\uE72C"),
                Tags = [new Tag("Loading") { Background = ColorHelpers.FromRgb(108, 117, 125), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
        }

        // Add refresh command
        items.Add(new ListItem(new Commands.RefreshListCommand(Refresh))
        {
            Title = "🔄 Refresh",
            Subtitle = "Reload statistics",
            Icon = new IconInfo("\uE72C")
        });

        return items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // No search filtering needed for stats page
    }

    private void Refresh()
    {
        IsLoading = true;
        RaiseItemsChanged();
        FetchStatsAsync();
    }

    private async void FetchStatsAsync()
    {
        _errorMessage = null;

        try
        {
            _taskStats = await _apiClient.GetStatsAsync();
            _timeStats = await _apiClient.GetTimeStatsAsync();

            if (_taskStats == null)
            {
                _errorMessage = "Failed to load statistics";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StatsPage] FetchStats - Exception: {ex.Message}");
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            RaiseItemsChanged();
        }
    }
}
