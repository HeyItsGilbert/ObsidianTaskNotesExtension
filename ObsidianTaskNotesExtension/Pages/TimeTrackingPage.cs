// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Commands;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

internal sealed partial class TimeTrackingPage : DynamicListPage
{
    private readonly TaskNotesApiClient _apiClient;
    private List<ActiveSession>? _activeSessions;
    private TimeSummary? _todaySummary;
    private TimeSummary? _weekSummary;
    private string? _errorMessage;

    public TimeTrackingPage(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        Icon = new IconInfo("\uE823"); // Clock icon
        Title = "Time Tracking";
        Name = "Time Tracking";

        FetchDataAsync();
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

        // Active sessions
        if (_activeSessions is { Count: > 0 })
        {
            foreach (var session in _activeSessions)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = $"Active: {session.TaskTitle ?? session.TaskId ?? "Unknown"}",
                    Subtitle = $"Started: {session.Start}{(session.Description != null ? $" - {session.Description}" : "")}",
                    Icon = new IconInfo("\uE916") // Play icon
                });
            }
        }
        else
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No Active Timers",
                Subtitle = "Start time tracking from a task",
                Icon = new IconInfo("\uE823")
            });
        }

        // Today summary
        if (_todaySummary?.Summary != null)
        {
            var minutes = _todaySummary.Summary.TotalMinutes;
            var timeStr = minutes >= 60 ? $"{minutes / 60:F0}h {minutes % 60:F0}m" : $"{minutes:F0}m";
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Today",
                Subtitle = timeStr,
                Icon = new IconInfo("\uE787") // Calendar icon
            });

            if (_todaySummary.TopTasks is { Count: > 0 })
            {
                foreach (var task in _todaySummary.TopTasks)
                {
                    var taskTime = task.Minutes >= 60 ? $"{task.Minutes / 60:F0}h {task.Minutes % 60:F0}m" : $"{task.Minutes:F0}m";
                    items.Add(new ListItem(new NoOpCommand())
                    {
                        Title = $"  {task.Title ?? task.Task ?? "Unknown"}",
                        Subtitle = taskTime,
                        Icon = new IconInfo("\uE73A")
                    });
                }
            }
        }

        // Week summary
        if (_weekSummary?.Summary != null)
        {
            var minutes = _weekSummary.Summary.TotalMinutes;
            var timeStr = minutes >= 60 ? $"{minutes / 60:F0}h {minutes % 60:F0}m" : $"{minutes:F0}m";
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "This Week",
                Subtitle = timeStr,
                Icon = new IconInfo("\uE8A5")
            });
        }

        // Refresh
        items.Add(new ListItem(new RefreshListCommand(Refresh))
        {
            Title = "Refresh",
            Icon = new IconInfo("\uE72C")
        });

        return items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // No search filtering needed for time tracking page
    }

    private void Refresh()
    {
        FetchDataAsync();
    }

    private async void FetchDataAsync()
    {
        IsLoading = true;
        _errorMessage = null;

        try
        {
            _activeSessions = await _apiClient.GetActiveTimeSessionsAsync();
            _todaySummary = await _apiClient.GetTimeSummaryAsync(period: "today");
            _weekSummary = await _apiClient.GetTimeSummaryAsync(period: "week");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TimeTrackingPage] FetchData - Exception: {ex.Message}");
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            RaiseItemsChanged();
        }
    }
}
