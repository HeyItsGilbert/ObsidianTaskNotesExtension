// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Commands;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

internal sealed partial class ObsidianTaskNotesExtensionPage : DynamicListPage
{
    private readonly TaskNotesApiClient _apiClient;
    private List<TaskItem> _tasks = new();
    private string? _errorMessage;
    private string _searchText = string.Empty;

    public ObsidianTaskNotesExtensionPage(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Obsidian Task Notes";
        Name = "Tasks";

        // Start loading tasks
        FetchTasksAsync();
    }

    public override IListItem[] GetItems()
    {
        var items = new List<IListItem>();

        if (_errorMessage != null)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "Connection Error",
                Subtitle = _errorMessage,
                Icon = new IconInfo("\uE783") // Error icon
            });
        }
        else if (_tasks.Count == 0)
        {
            items.Add(new ListItem(new NoOpCommand())
            {
                Title = "No tasks found",
                Subtitle = "Create tasks in Obsidian with TaskNotes plugin",
                Icon = new IconInfo("\uE8E5") // Empty icon
            });
        }
        else
        {
            var filteredTasks = string.IsNullOrWhiteSpace(_searchText)
                ? _tasks
                : _tasks.Where(t => t.Title.Contains(_searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            if (filteredTasks.Count == 0 && !string.IsNullOrWhiteSpace(_searchText))
            {
                items.Add(new ListItem(new NoOpCommand())
                {
                    Title = "No matching tasks",
                    Subtitle = $"No tasks found matching '{_searchText}'",
                    Icon = new IconInfo("\uE721") // Search icon
                });
            }
            else
            {
                var taskItems = filteredTasks
                    .OrderBy(t => t.IsOverdue ? 0 : 1)
                    .ThenBy(t => t.Due ?? DateTime.MaxValue)
                    .ThenBy(t => GetPrioritySortOrder(t.Priority))
                    .Select(task => CreateTaskListItem(task));

                items.AddRange(taskItems);
            }
        }

        // Always add refresh command at the end
        var refreshCommand = new RefreshListCommand(RefreshTasks);
        items.Add(new ListItem(refreshCommand)
        {
            Title = "Refresh Tasks",
            Subtitle = "Reload tasks from TaskNotes API",
            Icon = new IconInfo("\uE72C") // Sync/Refresh icon
        });

        return items.ToArray();
    }

    private ListItem CreateTaskListItem(TaskItem task)
    {
        var toggleCommand = new ToggleTaskStatusCommand(task, _apiClient, RefreshTasks);
        var openCommand = new OpenInObsidianCommand(task, _apiClient);
        var archiveCommand = new ArchiveTaskCommand(task, _apiClient, RefreshTasks);
        var copyLinkCommand = new CopyTaskLinkCommand(task, _apiClient);

        return new ListItem(toggleCommand)
        {
            Title = task.Title,
            Subtitle = FormatDueDate(task),
            Icon = GetPriorityIcon(task),
            MoreCommands = [
                new CommandContextItem(openCommand),
                new CommandContextItem(archiveCommand),
                new CommandContextItem(copyLinkCommand)
            ]
        };
    }

    private static string FormatDueDate(TaskItem task)
    {
        if (!task.Due.HasValue)
        {
            return "No due date";
        }

        var due = task.Due.Value;

        if (task.IsOverdue)
        {
            var daysOverdue = (DateTime.Today - due.Date).Days;
            return daysOverdue == 1 ? "Overdue by 1 day" : $"Overdue by {daysOverdue} days";
        }

        if (task.IsDueToday)
        {
            return "Due today";
        }

        if (task.IsDueTomorrow)
        {
            return "Due tomorrow";
        }

        var daysUntil = (due.Date - DateTime.Today).Days;
        if (daysUntil <= 7)
        {
            return $"Due in {daysUntil} days";
        }

        return $"Due: {due:MMM d}";
    }

    private static IconInfo GetPriorityIcon(TaskItem task)
    {
        if (task.IsOverdue)
        {
            return new IconInfo("\uE7BA"); // Warning icon for overdue
        }

        var priority = task.Priority?.ToLowerInvariant() ?? "";

        return priority switch
        {
            "1-urgent" or "urgent" or "1" => new IconInfo("\uE91B"), // Red circle
            "2-high" or "high" or "2" => new IconInfo("\uE91B"),     // Orange/yellow
            "3-medium" or "medium" or "3" => new IconInfo("\uE91B"), // Green
            "4-normal" or "normal" or "4" => new IconInfo("\uE91B"), // Blue
            "5-low" or "low" or "5" => new IconInfo("\uE91B"),       // Gray
            _ => new IconInfo("\uE73A")                               // Default checkbox
        };
    }

    private static int GetPrioritySortOrder(string? priority)
    {
        var p = priority?.ToLowerInvariant() ?? "";

        return p switch
        {
            "1-urgent" or "urgent" or "1" => 1,
            "2-high" or "high" or "2" => 2,
            "3-medium" or "medium" or "3" => 3,
            "4-normal" or "normal" or "4" => 4,
            "5-low" or "low" or "5" => 5,
            _ => 4 // Default to normal priority
        };
    }

    private void RefreshTasks()
    {
        FetchTasksAsync();
    }

    private async void FetchTasksAsync()
    {
        IsLoading = true;
        _errorMessage = null;

        try
        {
            // First test connection
            var (success, message) = await _apiClient.TestConnectionAsync();

            if (!success)
            {
                _errorMessage = message;
                _tasks = new List<TaskItem>();
            }
            else
            {
                _tasks = await _apiClient.GetActiveTasksAsync();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
            _tasks = new List<TaskItem>();
        }
        finally
        {
            IsLoading = false;
            RaiseItemsChanged();
        }
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        _searchText = newSearch ?? string.Empty;
        RaiseItemsChanged();
    }
}
