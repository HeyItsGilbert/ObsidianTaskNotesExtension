// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
