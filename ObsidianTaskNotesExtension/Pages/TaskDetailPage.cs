// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Commands;
using ObsidianTaskNotesExtension.Helpers;
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

        // === PROPERTIES ===
        // Status with tags
        var statusTags = new List<ITag>();
        var statusTag = TagHelpers.CreateStatusTag(_task);
        if (statusTag != null) statusTags.Add(statusTag);
        var dueTag = TagHelpers.CreateDueStatusTag(_task);
        if (dueTag != null) statusTags.Add(dueTag);

        items.Add(new ListItem(new NoOpCommand())
        {
            Title = "Status",
            Subtitle = _task.Archived ? "Archived" : _task.Completed ? "Completed" : "Active",
            Icon = new IconInfo("\uE73E"),
            Tags = statusTags.ToArray()
        });

        if (_task.Due.HasValue)
        {
            var dueTags = new List<ITag>();
            if (_task.IsOverdue)
            {
                var daysOverdue = (DateTime.Today - _task.Due.Value.Date).Days;
                dueTags.Add(new Tag($"{daysOverdue}d overdue")
                {
                    Background = ColorHelpers.FromRgb(220, 53, 69),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255)
                });
            }
            else if (_task.IsDueToday)
            {
                dueTags.Add(new Tag("Today")
                {
                    Background = ColorHelpers.FromRgb(255, 193, 7),
                    Foreground = ColorHelpers.FromRgb(33, 37, 41)
                });
            }
            else if (_task.IsDueTomorrow)
            {
                dueTags.Add(new Tag("Tomorrow")
                {
                    Background = ColorHelpers.FromRgb(40, 167, 69),
                    Foreground = ColorHelpers.FromRgb(255, 255, 255)
                });
            }

            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Due Date",
                Subtitle = _task.Due.Value.ToString("ddd, MMM d yyyy", System.Globalization.CultureInfo.InvariantCulture),
                Icon = new IconInfo("\uE787"), // Calendar icon
                Tags = dueTags.ToArray()
            });
        }

        if (!string.IsNullOrEmpty(_task.Priority))
        {
            var priorityTag = TagHelpers.CreatePriorityTag(_task.Priority);
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Priority",
                Subtitle = FormatPriorityLabel(_task.Priority),
                Icon = new IconInfo("\uE8CB"), // Flag icon
                Tags = priorityTag != null ? [priorityTag] : []
            });
        }

        if (_task.Tags is { Length: > 0 })
        {
            var userTags = _task.Tags.Select(TagHelpers.CreateUserTag).ToArray();
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Tags",
                Subtitle = $"{_task.Tags.Length} tag{(_task.Tags.Length == 1 ? "" : "s")}",
                Icon = new IconInfo("\uE8EC"), // Tag icon
                Tags = userTags
            });
        }

        if (_task.Projects is { Length: > 0 })
        {
            var projectTags = _task.Projects.Select(TagHelpers.CreateProjectTag).ToArray();
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Projects",
                Subtitle = $"{_task.Projects.Length} project{(_task.Projects.Length == 1 ? "" : "s")}",
                Icon = new IconInfo("\uE821"), // Folder icon
                Tags = projectTags
            });
        }

        // === TIME TRACKING ===
        if (_timeData?.Summary != null)
        {
            var minutes = _timeData.Summary.TotalMinutes;
            var timeTag = TagHelpers.CreateTimeTag(minutes);
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Time Tracked",
                Subtitle = $"{_timeData.Summary.EntryCount} entries",
                Icon = new IconInfo("\uE823"), // Clock icon
                Tags = [timeTag]
            });
        }

        if (_timeData?.ActiveSession != null)
        {
            items.Add(new ListItem(new StopTimeTrackingCommand(_task, _apiClient, Refresh))
            {
                Title = "⏱️ Active Timer",
                Subtitle = $"Started: {_timeData.ActiveSession.Start}",
                Icon = new IconInfo("\uE71A"), // Stop icon
                Tags = [TagHelpers.CreateActiveTag("Running")]
            });
        }

        // === ACTIONS ===
        items.Add(new ListItem(new EditTaskPage(_task, _apiClient))
        {
            Title = "✏️ Edit Task",
            Subtitle = "Modify task properties",
            Icon = new IconInfo("\uE70F")
        });

        var toggleCommand = new ToggleTaskStatusCommand(_task, _apiClient, () => { _refreshParent(); Refresh(); });
        items.Add(new ListItem(toggleCommand)
        {
            Title = _task.Completed ? "↩️ Reopen Task" : "✅ Complete Task",
            Subtitle = _task.Completed ? "Mark as active" : "Mark as done",
            Icon = _task.Completed ? new IconInfo("\uE73E") : new IconInfo("\uE73A")
        });

        items.Add(new ListItem(new StartTimeTrackingCommand(_task, _apiClient, Refresh))
        {
            Title = "⏱️ Start Time Tracking",
            Subtitle = "Begin tracking time on this task",
            Icon = new IconInfo("\uE916")
        });

        items.Add(new ListItem(new StartPomodoroCommand(_apiClient, Refresh, _task))
        {
            Title = "🍅 Start Pomodoro",
            Subtitle = "Start a focused work session",
            Icon = new IconInfo("\uE916")
        });

        items.Add(new ListItem(new OpenInObsidianCommand(_task, _apiClient))
        {
            Title = "📂 Open in Obsidian",
            Subtitle = "View task in Obsidian vault",
            Icon = new IconInfo("\uE8A7")
        });

        items.Add(new ListItem(new ArchiveTaskCommand(_task, _apiClient, () => { _refreshParent(); }))
        {
            Title = "📦 Archive Task",
            Subtitle = "Move to archive",
            Icon = new IconInfo("\uE7B8")
        });

        items.Add(new ListItem(new DeleteTaskCommand(_task, _apiClient, () => { _refreshParent(); }))
        {
            Title = "🗑️ Delete Task",
            Subtitle = "Permanently remove task",
            Icon = new IconInfo("\uE74D")
        });

        return items.ToArray();
    }

    private static string FormatPriorityLabel(string priority)
    {
        var p = priority.ToLowerInvariant();
        return p switch
        {
            "1-urgent" or "urgent" or "1" => "Urgent - Do immediately",
            "2-high" or "high" or "2" => "High - Important",
            "3-medium" or "medium" or "3" => "Medium - Standard",
            "4-normal" or "normal" or "4" => "Normal - Default",
            "5-low" or "low" or "5" => "Low - When time permits",
            _ => priority
        };
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
