// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

public class TaskNotesApiClient : IDisposable
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
        var healthUrl = $"{_settings.ApiBaseUrl}/api/health";
        Debug.WriteLine($"[TaskNotesApi] TestConnection - URL: {healthUrl}");
        try
        {
            ConfigureAuthHeader();
            var response = await _httpClient.GetAsync(healthUrl);
            Debug.WriteLine($"[TaskNotesApi] TestConnection - Status: {(int)response.StatusCode} {response.ReasonPhrase}");

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[TaskNotesApi] TestConnection - Response: {body}");
                return (true, "Connection successful!");
            }

            return (false, $"API returned status {(int)response.StatusCode}: {response.ReasonPhrase}");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[TaskNotesApi] TestConnection - HttpRequestException: {ex.Message}");
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Debug.WriteLine("[TaskNotesApi] TestConnection - Timed out");
            return (false, "Connection timed out. Is Obsidian running with TaskNotes HTTP API enabled?");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] TestConnection - Exception: {ex.GetType().Name}: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<List<TaskItem>> GetActiveTasksAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/tasks";
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - URL: {url}");
            var response = await _httpClient.GetAsync(url);
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - Status: {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - Failed with status {(int)response.StatusCode}");
                return new List<TaskItem>();
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - Response length: {json.Length} chars");
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - Response preview: {json[..Math.Min(500, json.Length)]}");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var tasks = apiResponse?.Data?.Tasks;

            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - API success: {apiResponse?.Success}, total: {apiResponse?.Data?.Total}, deserialized: {tasks?.Count ?? 0}");

            if (tasks != null)
            {
                foreach (var t in tasks.Take(5))
                {
                    Debug.WriteLine($"[TaskNotesApi]   Task: path='{t.Path}', title='{t.Title}', status='{t.Status}', completed={t.Completed}, archived={t.Archived}, due={t.DueString}");
                }
            }

            // Filter for active tasks (not completed and not archived) plus tasks completed today
            var activeTasks = tasks?
                .Where(t => (!t.Completed && !t.Archived) || t.CompletedToday)
                .ToList() ?? new List<TaskItem>();

            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - After filtering: {activeTasks.Count} active tasks (including completed today)");

            return activeTasks;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - Exception: {ex.GetType().Name}: {ex.Message}");
            Debug.WriteLine($"[TaskNotesApi] GetActiveTasks - StackTrace: {ex.StackTrace}");
            return new List<TaskItem>();
        }
    }

    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/tasks";
            Debug.WriteLine($"[TaskNotesApi] GetAllTasks - URL: {url}");
            var response = await _httpClient.GetAsync(url);
            Debug.WriteLine($"[TaskNotesApi] GetAllTasks - Status: {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                return new List<TaskItem>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var tasks = apiResponse?.Data?.Tasks;
            Debug.WriteLine($"[TaskNotesApi] GetAllTasks - Total: {tasks?.Count ?? 0}");

            return tasks ?? new List<TaskItem>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetAllTasks - Exception: {ex.GetType().Name}: {ex.Message}");
            return new List<TaskItem>();
        }
    }

    public async Task<bool> ToggleStatusAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/toggle-status";
            var response = await _httpClient.PostAsync(url, null);

            Debug.WriteLine($"[TaskNotesApi] ToggleStatusAsync - API success: {response.IsSuccessStatusCode},");
            Debug.WriteLine($"[TaskNotesApi] ToggleStatusAsync - Response preview: {response.Content}");

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

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
