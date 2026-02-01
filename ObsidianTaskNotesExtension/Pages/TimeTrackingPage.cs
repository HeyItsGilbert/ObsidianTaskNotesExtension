// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Commands;
using ObsidianTaskNotesExtension.Helpers;
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
                Icon = new IconInfo("\uE783"),
                Tags = [new Tag("Error") { Background = ColorHelpers.FromRgb(220, 53, 69), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
            return items.ToArray();
        }

        // === ACTIVE SESSIONS ===
        if (_activeSessions is { Count: > 0 })
        {
            foreach (var session in _activeSessions)
            {
                var sessionTags = new List<ITag>
                {
                    TagHelpers.CreateActiveTag("Running")
                };

                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = $"⏱️ {session.TaskTitle ?? session.TaskId ?? "Unknown Task"}",
                    Subtitle = session.Description ?? $"Started: {session.Start}",
                    Icon = new IconInfo("\uE916"), // Play icon
                    Tags = sessionTags.ToArray()
                });
            }
        }
        else
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No Active Timers",
                Subtitle = "Start time tracking from a task",
                Icon = new IconInfo("\uE823"),
                Tags = [new Tag("Idle") { Background = ColorHelpers.FromRgb(108, 117, 125), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
        }

        // === TODAY ===
        if (_todaySummary?.Summary != null)
        {
            var minutes = _todaySummary.Summary.TotalMinutes;

            var todayTags = new List<ITag>
            {
                TagHelpers.CreateTimeTag(minutes)
            };

            if (_todaySummary.Summary.TasksWithTime > 0)
            {
                todayTags.Add(TagHelpers.CreateCountTag(_todaySummary.Summary.TasksWithTime, "tasks"));
            }

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📅 Today's Progress",
                Subtitle = $"{_todaySummary.Summary.TasksWithTime} tasks tracked",
                Icon = new IconInfo("\uE787"), // Calendar icon
                Tags = todayTags.ToArray()
            });

            if (_todaySummary.TopTasks is { Count: > 0 })
            {
                foreach (var task in _todaySummary.TopTasks)
                {
                    var taskTimeTag = TagHelpers.CreateTimeTag(task.Minutes);
                    items.Add(new ListItem(new NoOpCommand())
                    {
                        Title = task.Title ?? task.Task ?? "Unknown",
                        Subtitle = "Time spent today",
                        Icon = new IconInfo("\uE73A"),
                        Tags = [taskTimeTag]
                    });
                }
            }
        }
        else
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📅 No Time Tracked Today",
                Subtitle = "Start a timer to track your work",
                Icon = new IconInfo("\uE787")
            });
        }

        // === THIS WEEK ===
        if (_weekSummary?.Summary != null)
        {
            var minutes = _weekSummary.Summary.TotalMinutes;

            var weekTags = new List<ITag>
            {
                TagHelpers.CreateTimeTag(minutes)
            };

            if (_weekSummary.Summary.TasksWithTime > 0)
            {
                weekTags.Add(TagHelpers.CreateCountTag(_weekSummary.Summary.TasksWithTime, "tasks"));
            }

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📆 This Week",
                Subtitle = $"{_weekSummary.Summary.TasksWithTime} tasks tracked",
                Icon = new IconInfo("\uE8A5"), // Detail view icon
                Tags = weekTags.ToArray()
            });
        }

        // Refresh
        items.Add(new ListItem(new RefreshListCommand(Refresh))
        {
            Title = "🔄 Refresh",
            Subtitle = "Reload time tracking data",
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
        IsLoading = true;
        RaiseItemsChanged();
        FetchDataAsync();
    }

    private async void FetchDataAsync()
    {
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
