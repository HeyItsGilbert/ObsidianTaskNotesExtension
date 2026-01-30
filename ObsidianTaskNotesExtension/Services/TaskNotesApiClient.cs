// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

public class TaskNotesApiClient
{
    private readonly SettingsManager _settings;
    private readonly HttpClient _httpClient;

    public TaskNotesApiClient(SettingsManager settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    private void ConfigureAuthHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrEmpty(_settings.AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.AuthToken);
        }
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var response = await _httpClient.GetAsync($"{_settings.ApiBaseUrl}/api/health");

            if (response.IsSuccessStatusCode)
            {
                return (true, "Connection successful!");
            }

            return (false, $"API returned status {(int)response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timed out. Is Obsidian running with TaskNotes HTTP API enabled?");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<List<TaskItem>> GetActiveTasksAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/tasks";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return new List<TaskItem>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Filter for active tasks (not completed and not archived)
            var activeTasks = tasks?
                .Where(t => !t.Completed && !t.Archived)
                .ToList() ?? new List<TaskItem>();

            return activeTasks;
        }
        catch (Exception)
        {
            return new List<TaskItem>();
        }
    }

    public async Task<bool> ToggleStatusAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/toggle";
            var response = await _httpClient.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ArchiveTaskAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/archive";
            var response = await _httpClient.PostAsync(url, null);

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string BuildObsidianUri(string taskId)
    {
        var vaultName = _settings.VaultName;
        var filePath = taskId;

        // Remove .md extension if present for the URI
        if (filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            filePath = filePath[..^3];
        }

        if (!string.IsNullOrEmpty(vaultName))
        {
            return $"obsidian://open?vault={HttpUtility.UrlEncode(vaultName)}&file={HttpUtility.UrlEncode(filePath)}";
        }

        return $"obsidian://open?file={HttpUtility.UrlEncode(filePath)}";
    }
}
