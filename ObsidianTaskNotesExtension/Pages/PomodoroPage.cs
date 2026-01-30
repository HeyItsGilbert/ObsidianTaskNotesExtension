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

internal sealed partial class PomodoroPage : DynamicListPage
{
    private readonly TaskNotesApiClient _apiClient;
    private PomodoroStatus? _status;
    private PomodoroStats? _stats;
    private List<PomodoroSession>? _sessions;
    private string? _errorMessage;

    public PomodoroPage(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        Icon = new IconInfo("\uE823"); // Clock icon
        Title = "Pomodoro Timer";
        Name = "Pomodoro";

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

        // Current session status
        if (_status?.Active == true && _status.Session != null)
        {
            var session = _status.Session;
            var remaining = _status.TimeRemaining.HasValue
                ? $"{_status.TimeRemaining.Value / 60}m {_status.TimeRemaining.Value % 60}s remaining"
                : "In progress";

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"Active: {session.TaskTitle ?? "No task"}",
                Subtitle = $"{session.State} - {remaining}",
                Icon = new IconInfo("\uE916")
            });

            // Show pause/resume based on state
            if (session.State?.Equals("paused", StringComparison.OrdinalIgnoreCase) == true)
            {
                items.Add(new ListItem(new ResumePomodoroCommand(_apiClient, Refresh))
                {
                    Title = "Resume Pomodoro",
                    Icon = new IconInfo("\uE768")
                });
            }
            else
            {
                items.Add(new ListItem(new PausePomodoroCommand(_apiClient, Refresh))
                {
                    Title = "Pause Pomodoro",
                    Icon = new IconInfo("\uE769")
                });
            }

            items.Add(new ListItem(new StopPomodoroCommand(_apiClient, Refresh))
            {
                Title = "Stop Pomodoro",
                Icon = new IconInfo("\uE71A")
            });
        }
        else
        {
            items.Add(new ListItem(new StartPomodoroCommand(_apiClient, Refresh))
            {
                Title = "Start Pomodoro",
                Subtitle = "Start a new pomodoro session",
                Icon = new IconInfo("\uE916")
            });
        }

        // Stats
        if (_stats != null)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Sessions Completed",
                Subtitle = _stats.SessionsCompleted.ToString(),
                Icon = new IconInfo("\uE73E")
            });

            var focusTime = _stats.TotalFocusMinutes;
            var focusStr = focusTime >= 60 ? $"{focusTime / 60:F0}h {focusTime % 60:F0}m" : $"{focusTime:F0}m";
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Total Focus Time",
                Subtitle = focusStr,
                Icon = new IconInfo("\uE823")
            });

            if (_stats.CurrentStreak > 0)
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "Current Streak",
                    Subtitle = _stats.CurrentStreak.ToString(),
                    Icon = new IconInfo("\uE945") // Streak icon
                });
            }
        }

        // Recent sessions
        if (_sessions is { Count: > 0 })
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Recent Sessions",
                Subtitle = $"{_sessions.Count} sessions today",
                Icon = new IconInfo("\uE81C") // History icon
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
        // No search filtering needed for pomodoro page
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
            _status = await _apiClient.GetPomodoroStatusAsync();
            _stats = await _apiClient.GetPomodoroStatsAsync();
            _sessions = await _apiClient.GetPomodoroSessionsAsync(limit: 10);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PomodoroPage] FetchData - Exception: {ex.Message}");
            _errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            RaiseItemsChanged();
        }
    }
}
