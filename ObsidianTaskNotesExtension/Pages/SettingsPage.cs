// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

internal sealed partial class SettingsFormContent : FormContent
{
    private readonly SettingsManager _settingsManager;
    private readonly TaskNotesApiClient _apiClient;

    public SettingsFormContent(SettingsManager settingsManager, TaskNotesApiClient apiClient)
    {
        _settingsManager = settingsManager;
        _apiClient = apiClient;

        var settings = _settingsManager.GetSettings();

        TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "TaskNotes API Configuration",
                    "weight": "bolder",
                    "size": "medium"
                },
                {
                    "type": "TextBlock",
                    "text": "Configure the connection to your TaskNotes HTTP API. Make sure the API is enabled in TaskNotes settings (Settings -> HTTP API -> Enable).",
                    "wrap": true,
                    "spacing": "small"
                },
                {
                    "type": "Input.Text",
                    "id": "apiBaseUrl",
                    "label": "API Base URL",
                    "placeholder": "http://localhost:8080",
                    "value": "{{settings.ApiBaseUrl}}"
                },
                {
                    "type": "Input.Text",
                    "id": "authToken",
                    "label": "Auth Token (optional)",
                    "placeholder": "Leave empty if not using authentication",
                    "value": "{{settings.AuthToken}}"
                },
                {
                    "type": "Input.Text",
                    "id": "vaultName",
                    "label": "Vault Name (for Obsidian links)",
                    "placeholder": "Your vault name",
                    "value": "{{settings.VaultName}}"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Save Settings",
                    "data": {
                        "action": "save"
                    }
                }
            ]
        }
        """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();

        if (formInput == null)
        {
            return CommandResult.KeepOpen();
        }

        var apiBaseUrl = formInput["apiBaseUrl"]?.GetValue<string>() ?? "http://localhost:8080";
        var authToken = formInput["authToken"]?.GetValue<string>() ?? "";
        var vaultName = formInput["vaultName"]?.GetValue<string>() ?? "";

        // Preserve existing icon mappings when saving connection settings
        var currentSettings = _settingsManager.GetSettings();
        _settingsManager.SaveSettings(new ExtensionSettings
        {
            ApiBaseUrl = apiBaseUrl,
            AuthToken = authToken,
            VaultName = vaultName,
            IconMappings = currentSettings.IconMappings
        });

        return CommandResult.GoBack();
    }
}

internal sealed partial class TestConnectionCommand : InvokableCommand
{
    private readonly TaskNotesApiClient _apiClient;

    public TestConnectionCommand(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;
        Name = "Test Connection";
        Icon = new IconInfo("\uE703"); // Network icon
    }

    public override CommandResult Invoke()
    {
        _ = TestAsync();
        return CommandResult.KeepOpen();
    }

    private async System.Threading.Tasks.Task TestAsync()
    {
        var (success, message) = await _apiClient.TestConnectionAsync();
        // The result will be shown via toast or status
        System.Diagnostics.Debug.WriteLine($"Connection test: {success} - {message}");
    }
}

internal sealed partial class SettingsPage : ListPage
{
    private readonly SettingsManager _settingsManager;
    private readonly TaskNotesApiClient _apiClient;
    private readonly IconMappingService _iconMappingService;

    public SettingsPage(SettingsManager settingsManager, TaskNotesApiClient apiClient, IconMappingService iconMappingService)
    {
        _settingsManager = settingsManager;
        _apiClient = apiClient;
        _iconMappingService = iconMappingService;

        Icon = new IconInfo("\uE713"); // Settings icon
        Title = "Settings";
        Name = "Settings";
    }

    public override IListItem[] GetItems()
    {
        var settingsForm = new SettingsFormPage(_settingsManager, _apiClient);
        var testCommand = new TestConnectionCommand(_apiClient);
        var iconMappingsPage = new IconMappingSettingsPage(_settingsManager, _iconMappingService);

        return
        [
            new ListItem(settingsForm)
            {
                Title = "Configure API Connection",
                Subtitle = $"Current: {_settingsManager.ApiBaseUrl}",
                Icon = new IconInfo("\uE713")
            },
            new ListItem(testCommand)
            {
                Title = "Test Connection",
                Subtitle = "Verify TaskNotes API is reachable",
                Icon = new IconInfo("\uE703")
            },
            new ListItem(iconMappingsPage)
            {
                Title = "Icon Mappings",
                Subtitle = "Configure task icons by status, project, context, or tag",
                Icon = new IconInfo("\uE790")
            }
        ];
    }
}

internal sealed partial class SettingsFormPage : ContentPage
{
    private readonly SettingsManager _settingsManager;
    private readonly TaskNotesApiClient _apiClient;

    public SettingsFormPage(SettingsManager settingsManager, TaskNotesApiClient apiClient)
    {
        _settingsManager = settingsManager;
        _apiClient = apiClient;

        Icon = new IconInfo("\uE713");
        Title = "Configure API";
        Name = "Configure";
    }

    public override IContent[] GetContent()
    {
        return [new SettingsFormContent(_settingsManager, _apiClient)];
    }
}
