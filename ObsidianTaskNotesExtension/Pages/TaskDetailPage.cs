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

internal sealed partial class TaskDetailPage : DynamicListPage
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action _refreshParent;
    private TaskTimeData? _timeData;

    public TaskDetailPage(TaskItem task, TaskNotesApiClient apiClient, Action refreshParent)
    {
        _task = task;
        _apiClient = apiClient;
        _refreshParent = refreshParent;

        Icon = new IconInfo("\uE8A5"); // Detail icon
        Title = task.Title;
        Name = "Task Details";

        LoadTimeDataAsync();
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        // Task info
        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "Status",
            Subtitle = _task.Archived ? "Archived" : _task.Completed ? "Completed" : "Active",
            Icon = new IconInfo("\uE73E")
        });

        if (_task.Due.HasValue)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Due Date",
                Subtitle = _task.Due.Value.ToString("yyyy-MM-dd"),
                Icon = new IconInfo("\uE787") // Calendar icon
            });
        }

        if (!string.IsNullOrEmpty(_task.Priority))
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Priority",
                Subtitle = _task.Priority,
                Icon = new IconInfo("\uE8CB") // Flag icon
            });
        }

        if (_task.Tags is { Length: > 0 })
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Tags",
                Subtitle = string.Join(", ", _task.Tags),
                Icon = new IconInfo("\uE8EC") // Tag icon
            });
        }

        if (_task.Projects is { Length: > 0 })
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Projects",
                Subtitle = string.Join(", ", _task.Projects),
                Icon = new IconInfo("\uE821") // Folder icon
            });
        }

        // Time tracking info
        if (_timeData?.Summary != null)
        {
            var minutes = _timeData.Summary.TotalMinutes;
            var timeStr = minutes >= 60 ? $"{minutes / 60:F0}h {minutes % 60:F0}m" : $"{minutes:F0}m";
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Time Tracked",
                Subtitle = $"{timeStr} ({_timeData.Summary.EntryCount} entries)",
                Icon = new IconInfo("\uE823") // Clock icon
            });
        }

        if (_timeData?.ActiveSession != null)
        {
            items.Add(new ListItem(new StopTimeTrackingCommand(_task, _apiClient, Refresh))
            {
                Title = "Active Timer",
                Subtitle = $"Started: {_timeData.ActiveSession.Start}",
                Icon = new IconInfo("\uE71A") // Stop icon
            });
        }

        // Actions
        items.Add(new ListItem(new EditTaskPage(_task, _apiClient))
        {
            Title = "Edit Task",
            Icon = new IconInfo("\uE70F")
        });

        var toggleCommand = new ToggleTaskStatusCommand(_task, _apiClient, () => { _refreshParent(); Refresh(); });
        items.Add(new ListItem(toggleCommand)
        {
            Title = _task.Completed ? "Reopen Task" : "Complete Task",
            Icon = _task.Completed ? new IconInfo("\uE73E") : new IconInfo("\uE73A")
        });

        items.Add(new ListItem(new StartTimeTrackingCommand(_task, _apiClient, Refresh))
        {
            Title = "Start Time Tracking",
            Icon = new IconInfo("\uE916")
        });

        items.Add(new ListItem(new StartPomodoroCommand(_apiClient, Refresh, _task))
        {
            Title = "Start Pomodoro",
            Icon = new IconInfo("\uE916")
        });

        items.Add(new ListItem(new OpenInObsidianCommand(_task, _apiClient))
        {
            Title = "Open in Obsidian",
            Icon = new IconInfo("\uE8A7")
        });

        items.Add(new ListItem(new ArchiveTaskCommand(_task, _apiClient, () => { _refreshParent(); }))
        {
            Title = "Archive Task",
            Icon = new IconInfo("\uE7B8")
        });

        items.Add(new ListItem(new DeleteTaskCommand(_task, _apiClient, () => { _refreshParent(); }))
        {
            Title = "Delete Task",
            Icon = new IconInfo("\uE74D")
        });

        return items.ToArray();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        // No search filtering needed for task detail page
    }

    private void Refresh()
    {
        LoadTimeDataAsync();
    }

    private async void LoadTimeDataAsync()
    {
        try
        {
            _timeData = await _apiClient.GetTaskTimeAsync(_task.Id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskDetailPage] LoadTimeData - Exception: {ex.Message}");
        }
        finally
        {
            RaiseItemsChanged();
        }
    }
}
