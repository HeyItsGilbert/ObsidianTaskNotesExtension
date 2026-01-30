// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Pages;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension;

public partial class ObsidianTaskNotesExtensionCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;
    private readonly SettingsManager _settingsManager;
    private readonly TaskNotesApiClient _apiClient;

    public ObsidianTaskNotesExtensionCommandsProvider()
    {
        DisplayName = "Obsidian Task Notes";
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

        // Initialize shared services
        _settingsManager = new SettingsManager();
        _apiClient = new TaskNotesApiClient(_settingsManager);

        // Create pages
        var tasksPage = new ObsidianTaskNotesExtensionPage(_apiClient);
        var allTasksPage = new AllTasksPage(_apiClient);
        var settingsPage = new SettingsPage(_settingsManager, _apiClient);

        _commands =
        [
            new CommandItem(tasksPage)
            {
                Title = "Obsidian Tasks",
                Subtitle = "View and manage your TaskNotes tasks"
            },
            new CommandItem(allTasksPage)
            {
                Title = "Obsidian All Tasks",
                Subtitle = "View all tasks including completed and archived"
            },
            new CommandItem(settingsPage)
            {
                Title = "Obsidian Tasks Settings",
                Subtitle = "Configure API connection"
            }
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
