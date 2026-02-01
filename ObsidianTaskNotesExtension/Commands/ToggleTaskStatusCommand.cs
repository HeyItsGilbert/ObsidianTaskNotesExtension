// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class ToggleTaskStatusCommand : InvokableCommand
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action _refreshCallback;

    public ToggleTaskStatusCommand(TaskItem task, TaskNotesApiClient apiClient, Action refreshCallback)
    {
        _task = task;
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = _task.Completed ? "Reopen Task" : "Complete Task";
        Icon = _task.Completed ? new IconInfo("\uE73E") : new IconInfo("\uE73A"); // Checkbox icons
    }

    public override CommandResult Invoke()
    {
        _ = ToggleStatusAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task ToggleStatusAsync()
    {
        var success = await _apiClient.ToggleStatusAsync(_task.Id);

        if (success)
        {
            _refreshCallback?.Invoke();
        }
    }
}
