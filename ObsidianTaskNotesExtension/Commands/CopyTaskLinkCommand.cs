// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;
using Windows.ApplicationModel.DataTransfer;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class CopyTaskLinkCommand : InvokableCommand
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;

    public CopyTaskLinkCommand(TaskItem task, TaskNotesApiClient apiClient)
    {
        _task = task;
        _apiClient = apiClient;

        Name = "Copy Obsidian Link";
        Icon = new IconInfo("\uE8C8"); // Copy icon
    }

    public override CommandResult Invoke()
    {
        var uri = _apiClient.BuildObsidianUri(_task.Id);

        var dataPackage = new DataPackage();
        dataPackage.SetText(uri);
        Clipboard.SetContent(dataPackage);

        return CommandResult.Dismiss();
    }
}
