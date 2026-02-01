// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace ObsidianTaskNotesExtension.Commands;

internal sealed partial class RefreshListCommand : InvokableCommand
{
    private readonly Action _refreshCallback;

    public RefreshListCommand(Action refreshCallback)
    {
        _refreshCallback = refreshCallback;

        Name = "Refresh Tasks";
        Icon = new IconInfo("\uE72C"); // Sync/Refresh icon
    }

    public override CommandResult Invoke()
    {
        _refreshCallback?.Invoke();
        return CommandResult.KeepOpen();
    }
}
