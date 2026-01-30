// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

internal sealed partial class CreateTaskFormContent : FormContent
{
    private readonly TaskNotesApiClient _apiClient;

    public CreateTaskFormContent(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        TemplateJson = """
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Create New Task",
                    "weight": "bolder",
                    "size": "medium"
                },
                {
                    "type": "Input.Text",
                    "id": "title",
                    "label": "Title",
                    "placeholder": "Task title (required)",
                    "isRequired": true
                },
                {
                    "type": "Input.ChoiceSet",
                    "id": "priority",
                    "label": "Priority",
                    "value": "",
                    "choices": [
                        { "title": "None", "value": "" },
                        { "title": "Urgent", "value": "1-urgent" },
                        { "title": "High", "value": "2-high" },
                        { "title": "Medium", "value": "3-medium" },
                        { "title": "Normal", "value": "4-normal" },
                        { "title": "Low", "value": "5-low" }
                    ]
                },
                {
                    "type": "Input.Date",
                    "id": "due",
                    "label": "Due Date"
                },
                {
                    "type": "Input.Date",
                    "id": "scheduled",
                    "label": "Scheduled Date"
                },
                {
                    "type": "Input.Text",
                    "id": "tags",
                    "label": "Tags",
                    "placeholder": "Comma-separated tags"
                },
                {
                    "type": "Input.Text",
                    "id": "projects",
                    "label": "Projects",
                    "placeholder": "Comma-separated projects"
                },
                {
                    "type": "Input.Text",
                    "id": "details",
                    "label": "Details",
                    "placeholder": "Additional details",
                    "isMultiline": true
                },
                {
                    "type": "Input.Text",
                    "id": "timeEstimate",
                    "label": "Time Estimate",
                    "placeholder": "e.g. 2h, 30m"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Create Task",
                    "data": { "action": "create" }
                }
            ]
        }
        """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null) return CommandResult.KeepOpen();

        var title = formInput["title"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(title)) return CommandResult.KeepOpen();

        var request = new CreateTaskRequest
        {
            Title = title,
            Priority = NullIfEmpty(formInput["priority"]?.GetValue<string>()),
            Due = NullIfEmpty(formInput["due"]?.GetValue<string>()),
            Scheduled = NullIfEmpty(formInput["scheduled"]?.GetValue<string>()),
            Tags = ParseCommaSeparated(formInput["tags"]?.GetValue<string>()),
            Projects = ParseCommaSeparated(formInput["projects"]?.GetValue<string>()),
            Details = NullIfEmpty(formInput["details"]?.GetValue<string>()),
            TimeEstimate = NullIfEmpty(formInput["timeEstimate"]?.GetValue<string>()),
        };

        _ = CreateAsync(request);
        return CommandResult.GoBack();
    }

    private async System.Threading.Tasks.Task CreateAsync(CreateTaskRequest request)
    {
        var result = await _apiClient.CreateTaskAsync(request);
        Debug.WriteLine($"[CreateTaskPage] Created task: {result?.Title ?? "(failed)"}");
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string[]? ParseCommaSeparated(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts : null;
    }
}

internal sealed partial class CreateTaskPage : ContentPage
{
    private readonly TaskNotesApiClient _apiClient;

    public CreateTaskPage(TaskNotesApiClient apiClient)
    {
        _apiClient = apiClient;

        Icon = new IconInfo("\uE710"); // Add icon
        Title = "Create Task";
        Name = "Create Task";
    }

    public override IContent[] GetContent()
    {
        return [new CreateTaskFormContent(_apiClient)];
    }
}
