// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class OpenInObsidianCommand : InvokableCommand
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;

    public OpenInObsidianCommand(TaskItem task, TaskNotesApiClient apiClient)
    {
        _task = task;
        _apiClient = apiClient;

        Name = "Open in Obsidian";
        Icon = new IconInfo("\uE8A7"); // OpenFile icon
    }

    public override CommandResult Invoke()
    {
        var uri = _apiClient.BuildObsidianUri(_task.Id);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if Obsidian is not installed
        }

        return CommandResult.Dismiss();
    }
}
