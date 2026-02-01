// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class ArchiveTaskCommand : InvokableCommand
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action _refreshCallback;

    public ArchiveTaskCommand(TaskItem task, TaskNotesApiClient apiClient, Action refreshCallback)
    {
        _task = task;
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Archive Task";
        Icon = new IconInfo("\uE7B8"); // Archive icon
    }

    public override CommandResult Invoke()
    {
        _ = ArchiveAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task ArchiveAsync()
    {
        var success = await _apiClient.ArchiveTaskAsync(_task.Id);

        if (success)
        {
            _refreshCallback?.Invoke();
        }
    }
}
