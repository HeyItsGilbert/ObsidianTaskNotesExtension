// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
                Icon = new IconInfo("\uE783"),
                Tags = [new Tag("Error") { Background = ColorHelpers.FromRgb(220, 53, 69), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
            return items.ToArray();
        }

        // === CURRENT SESSION ===
        if (_status?.IsRunning == true && _status.CurrentSession != null)
        {
            var session = _status.CurrentSession;
            var remaining = $"{_status.TimeRemaining / 60}m {_status.TimeRemaining % 60}s";

            var stateTags = new List<ITag>();
            var sessionType = session.Type?.ToLowerInvariant() ?? "work";

            if (sessionType == "short-break" || sessionType == "long-break")
            {
                stateTags.Add(new Tag("Break")
                {
                    Icon = new IconInfo("\uE799"),
                    Background = ColorHelpers.FromRgb(40, 167, 69),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255)
                });
            }
            else
            {
                stateTags.Add(TagHelpers.CreateActiveTag("Working"));
            }

            stateTags.Add(new Tag(remaining)
            {
                Icon = new IconInfo("\uE823"),
                Background = ColorHelpers.FromRgb(102, 16, 242),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Time remaining"
            });

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = $"🍅 {session.Task?.Title ?? "Pomodoro Session"}",
                Subtitle = "Active session",
                Icon = new IconInfo("\uE916"),
                Tags = stateTags.ToArray()
            });

            items.Add(new ListItem(new PausePomodoroCommand(_apiClient, Refresh))
            {
                Title = "⏸️ Pause Pomodoro",
                Subtitle = "Take a quick break",
                Icon = new IconInfo("\uE769")
            });

            items.Add(new ListItem(new StopPomodoroCommand(_apiClient, Refresh))
            {
                Title = "⏹️ Stop Pomodoro",
                Subtitle = "End the current session",
                Icon = new IconInfo("\uE71A")
            });
        }
        else
        {
            items.Add(new ListItem(new StartPomodoroCommand(_apiClient, Refresh))
            {
                Title = "🍅 Start Pomodoro",
                Subtitle = "Begin a new 25-minute focus session",
                Icon = new IconInfo("\uE916"),
                Tags = [new Tag("Ready") { Background = ColorHelpers.FromRgb(40, 167, 69), Foreground = ColorHelpers.FromRgb(255, 255, 255) }]
            });
        }

        // === STATISTICS ===
        if (_stats != null)
        {
            var statsTags = new List<ITag>();

            if ((_status?.CurrentStreak ?? 0) > 0)
            {
                statsTags.Add(TagHelpers.CreateStreakTag(_status?.CurrentStreak ?? 0));
            }
            statsTags.Add(TagHelpers.CreateSessionCountTag(_stats.CompletedSessions));

            var focusTime = (double)_stats.TotalFocusTime;
            var focusStr = focusTime >= 60 ? $"{focusTime / 60:F0}h {focusTime % 60:F0}m" : $"{focusTime:F0}m";

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📊 Focus Statistics",
                Subtitle = $"Total focus time: {focusStr}",
                Icon = new IconInfo("\uE9D9"), // Chart icon
                Tags = statsTags.ToArray()
            });
        }

        // === RECENT SESSIONS ===
        if (_sessions is { Count: > 0 })
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "📜 Recent Sessions",
                Subtitle = $"{_sessions.Count} session{(_sessions.Count == 1 ? "" : "s")} today",
                Icon = new IconInfo("\uE81C"), // History icon
                Tags = [TagHelpers.CreateCountTag(_sessions.Count, "sessions")]
            });
        }

        // Refresh
        items.Add(new ListItem(new RefreshListCommand(Refresh))
        {
            Title = "🔄 Refresh",
            Subtitle = "Reload pomodoro data",
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
        IsLoading = true;
        RaiseItemsChanged();
        FetchDataAsync();
    }

    private async void FetchDataAsync()
    {
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
