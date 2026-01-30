// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class DeleteTaskCommand : InvokableCommand
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action _refreshCallback;

    public DeleteTaskCommand(TaskItem task, TaskNotesApiClient apiClient, Action refreshCallback)
    {
        _task = task;
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Delete Task";
        Icon = new IconInfo("\uE74D"); // Delete icon
    }

    public override CommandResult Invoke()
    {
        _ = DeleteAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task DeleteAsync()
    {
        var success = await _apiClient.DeleteTaskAsync(_task.Id);

        if (success)
        {
            _refreshCallback?.Invoke();
        }
    }
}
