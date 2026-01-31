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

internal sealed partial class EditTaskFormContent : FormContent
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;

    public EditTaskFormContent(TaskItem task, TaskNotesApiClient apiClient)
    {
        _task = task;
        _apiClient = apiClient;

        var priorityValue = task.Priority ?? "";
        var dueValue = task.DueString ?? "";
        var scheduledValue = task.Scheduled ?? "";
        var tagsValue = task.Tags != null ? string.Join(", ", task.Tags) : "";
        var projectsValue = task.Projects != null ? string.Join(", ", task.Projects) : "";

        TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Edit Task",
                    "weight": "bolder",
                    "size": "medium"
                },
                {
                    "type": "Input.Text",
                    "id": "title",
                    "label": "Title",
                    "value": "{{EscapeJson(_task.Title)}}"
                },
                {
                    "type": "Input.ChoiceSet",
                    "id": "priority",
                    "label": "Priority",
                    "value": "{{priorityValue}}",
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
                    "label": "Due Date",
                    "value": "{{dueValue}}"
                },
                {
                    "type": "Input.Date",
                    "id": "scheduled",
                    "label": "Scheduled Date",
                    "value": "{{scheduledValue}}"
                },
                {
                    "type": "Input.Text",
                    "id": "tags",
                    "label": "Tags",
                    "placeholder": "Comma-separated tags",
                    "value": "{{tagsValue}}"
                },
                {
                    "type": "Input.Text",
                    "id": "projects",
                    "label": "Projects",
                    "placeholder": "Comma-separated projects",
                    "value": "{{projectsValue}}"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Save Changes",
                    "data": { "action": "update" }
                }
            ]
        }
        """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        var formInput = JsonNode.Parse(payload)?.AsObject();
        if (formInput == null) return CommandResult.KeepOpen();

        var request = new UpdateTaskRequest
        {
            Title = NullIfEmpty(formInput["title"]?.GetValue<string>()),
            Priority = NullIfEmpty(formInput["priority"]?.GetValue<string>()),
            Due = NullIfEmpty(formInput["due"]?.GetValue<string>()),
            Scheduled = NullIfEmpty(formInput["scheduled"]?.GetValue<string>()),
            Tags = ParseCommaSeparated(formInput["tags"]?.GetValue<string>()),
            Projects = ParseCommaSeparated(formInput["projects"]?.GetValue<string>()),
        };

        Debug.WriteLine($"[EditTaskPage] Submitting update for task: {_task.Id}");
        _ = UpdateAsync(request);
        return CommandResult.GoBack();
    }

    private async System.Threading.Tasks.Task UpdateAsync(UpdateTaskRequest request)
    {
        var result = await _apiClient.UpdateTaskAsync(_task.Id, request);
        if (result != null)
        {
            Debug.WriteLine($"[EditTaskPage] Successfully updated task: {result.Title}");
        }
        else
        {
            Debug.WriteLine($"[EditTaskPage] Failed to update task: {_task.Id}");
        }
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string[]? ParseCommaSeparated(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        return parts.Length > 0 ? parts : null;
    }

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

internal sealed partial class EditTaskPage : ContentPage
{
    private readonly TaskItem _task;
    private readonly TaskNotesApiClient _apiClient;

    public EditTaskPage(TaskItem task, TaskNotesApiClient apiClient)
    {
        _task = task;
        _apiClient = apiClient;

        Icon = new IconInfo("\uE70F"); // Edit icon
        Title = $"Edit: {task.Title}";
        Name = "Edit Task";
    }

    public override IContent[] GetContent()
    {
        return [new EditTaskFormContent(_task, _apiClient)];
    }
}
