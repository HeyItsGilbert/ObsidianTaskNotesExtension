// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
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
                Icon = new IconInfo("\uE783")
            });
            return items.ToArray();
        }

        if (_taskStats != null)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Total Tasks",
                Subtitle = _taskStats.Total.ToString(),
                Icon = new IconInfo("\uE8EF") // List icon
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Active Tasks",
                Subtitle = _taskStats.Active.ToString(),
                Icon = new IconInfo("\uE73A")
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Completed Tasks",
                Subtitle = _taskStats.Completed.ToString(),
                Icon = new IconInfo("\uE73E")
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Overdue Tasks",
                Subtitle = _taskStats.Overdue.ToString(),
                Icon = new IconInfo("\uE7BA")
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Archived Tasks",
                Subtitle = _taskStats.Archived.ToString(),
                Icon = new IconInfo("\uE7B8")
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Tasks with Time Tracking",
                Subtitle = _taskStats.WithTimeTracking.ToString(),
                Icon = new IconInfo("\uE823")
            });
        }

        if (_timeStats != null)
        {
            var minutes = _timeStats.TotalMinutes;
            var timeStr = minutes >= 60 ? $"{minutes / 60:F0}h {minutes % 60:F0}m" : $"{minutes:F0}m";
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Total Time Tracked",
                Subtitle = timeStr,
                Icon = new IconInfo("\uE916")
            });
        }

        if (items.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Loading...",
                Icon = new IconInfo("\uE72C")
            });
        }

        return items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // No search filtering needed for stats page
    }

    private async void FetchStatsAsync()
    {
        IsLoading = true;
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
