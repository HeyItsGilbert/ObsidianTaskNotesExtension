// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class StartPomodoroCommand : InvokableCommand
{
    private readonly TaskItem? _task;
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action? _refreshCallback;

    public StartPomodoroCommand(TaskNotesApiClient apiClient, Action? refreshCallback = null, TaskItem? task = null)
    {
        _task = task;
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Start Pomodoro";
        Icon = new IconInfo("\uE916"); // Play icon
    }

    public override CommandResult Invoke()
    {
        _ = StartAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task StartAsync()
    {
        await _apiClient.StartPomodoroAsync(_task?.Id);
        _refreshCallback?.Invoke();
    }
}

internal sealed partial class StopPomodoroCommand : InvokableCommand
{
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action? _refreshCallback;

    public StopPomodoroCommand(TaskNotesApiClient apiClient, Action? refreshCallback = null)
    {
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Stop Pomodoro";
        Icon = new IconInfo("\uE71A"); // Stop icon
    }

    public override CommandResult Invoke()
    {
        _ = StopAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task StopAsync()
    {
        await _apiClient.StopPomodoroAsync();
        _refreshCallback?.Invoke();
    }
}

internal sealed partial class PausePomodoroCommand : InvokableCommand
{
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action? _refreshCallback;

    public PausePomodoroCommand(TaskNotesApiClient apiClient, Action? refreshCallback = null)
    {
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Pause Pomodoro";
        Icon = new IconInfo("\uE769"); // Pause icon
    }

    public override CommandResult Invoke()
    {
        _ = PauseAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task PauseAsync()
    {
        await _apiClient.PausePomodoroAsync();
        _refreshCallback?.Invoke();
    }
}

internal sealed partial class ResumePomodoroCommand : InvokableCommand
{
    private readonly TaskNotesApiClient _apiClient;
    private readonly Action? _refreshCallback;

    public ResumePomodoroCommand(TaskNotesApiClient apiClient, Action? refreshCallback = null)
    {
        _apiClient = apiClient;
        _refreshCallback = refreshCallback;

        Name = "Resume Pomodoro";
        Icon = new IconInfo("\uE768"); // Resume/Play icon
    }

    public override CommandResult Invoke()
    {
        _ = ResumeAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task ResumeAsync()
    {
        await _apiClient.ResumePomodoroAsync();
        _refreshCallback?.Invoke();
    }
}
