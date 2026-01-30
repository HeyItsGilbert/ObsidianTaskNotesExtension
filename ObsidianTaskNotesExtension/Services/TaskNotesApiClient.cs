// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

public class TaskNotesApiClient : IDisposable
{
    private readonly SettingsManager _settings;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TaskNotesApiClient(SettingsManager settings)
    {
        _settings = settings;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    private T? DeserializeResponse<T>(string body, string caller)
    {
        Debug.WriteLine($"[TaskNotesApi] {caller} - Response preview: {body[..Math.Min(500, body.Length)]}");
        try
        {
            var result = JsonSerializer.Deserialize<T>(body, _jsonOptions);
            Debug.WriteLine($"[TaskNotesApi] {caller} - Deserialized successfully");
            return result;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[TaskNotesApi] {caller} - JSON deserialization failed: {ex.Message}");
            Debug.WriteLine($"[TaskNotesApi] {caller} - Full response: {body}");
            return default;
        }
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

    // --- Task CRUD ---

    public async Task<TaskItem?> CreateTaskAsync(CreateTaskRequest request)
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/tasks";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[TaskNotesApi] CreateTask - URL: {url}");
            var response = await _httpClient.PostAsync(url, content);
            Debug.WriteLine($"[TaskNotesApi] CreateTask - Status: {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<SingleTaskResponse>(body, "CreateTask");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] CreateTask - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<TaskItem?> GetTaskAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}";
            Debug.WriteLine($"[TaskNotesApi] GetTask - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<SingleTaskResponse>(body, "GetTask");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetTask - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<TaskItem?> UpdateTaskAsync(string taskId, UpdateTaskRequest request)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[TaskNotesApi] UpdateTask - URL: {url}");
            var response = await _httpClient.PutAsync(url, content);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<SingleTaskResponse>(body, "UpdateTask");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] UpdateTask - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> DeleteTaskAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}";
            Debug.WriteLine($"[TaskNotesApi] DeleteTask - URL: {url}");
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] DeleteTask - Exception: {ex.Message}");
            return false;
        }
    }

    // --- Time Tracking ---

    public async Task<bool> StartTimeTrackingAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/time/start";
            Debug.WriteLine($"[TaskNotesApi] StartTimeTracking - URL: {url}");
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] StartTimeTracking - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StartTimeTrackingWithDescriptionAsync(string taskId, string description)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/time/start-with-description";
            var json = JsonSerializer.Serialize(new { description });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[TaskNotesApi] StartTimeTrackingWithDescription - URL: {url}");
            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] StartTimeTrackingWithDescription - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StopTimeTrackingAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/time/stop";
            Debug.WriteLine($"[TaskNotesApi] StopTimeTracking - URL: {url}");
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] StopTimeTracking - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<TaskTimeData?> GetTaskTimeAsync(string taskId)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/time";
            Debug.WriteLine($"[TaskNotesApi] GetTaskTime - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<TaskTimeResponse>(body, "GetTaskTime");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetTaskTime - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<ActiveSession>> GetActiveTimeSessionsAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/time/active";
            Debug.WriteLine($"[TaskNotesApi] GetActiveTimeSessions - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<ActiveSession>();

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<ActiveSessionsResponse>(body, "GetActiveTimeSessions");
            return result?.Data ?? new List<ActiveSession>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetActiveTimeSessions - Exception: {ex.GetType().Name}: {ex.Message}");
            return new List<ActiveSession>();
        }
    }

    public async Task<TimeSummary?> GetTimeSummaryAsync(string? period = null, string? from = null, string? to = null)
    {
        try
        {
            ConfigureAuthHeader();
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(period)) queryParams.Add($"period={HttpUtility.UrlEncode(period)}");
            if (!string.IsNullOrEmpty(from)) queryParams.Add($"from={HttpUtility.UrlEncode(from)}");
            if (!string.IsNullOrEmpty(to)) queryParams.Add($"to={HttpUtility.UrlEncode(to)}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_settings.ApiBaseUrl}/api/time/summary{query}";
            Debug.WriteLine($"[TaskNotesApi] GetTimeSummary - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<TimeSummaryResponse>(body, "GetTimeSummary");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetTimeSummary - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    // --- Task Actions ---

    public async Task<bool> CompleteRecurringInstanceAsync(string taskId, string date)
    {
        try
        {
            ConfigureAuthHeader();
            var encodedId = HttpUtility.UrlEncode(taskId);
            var url = $"{_settings.ApiBaseUrl}/api/tasks/{encodedId}/complete-instance";
            var json = JsonSerializer.Serialize(new { date });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[TaskNotesApi] CompleteRecurringInstance - URL: {url}");
            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] CompleteRecurringInstance - Exception: {ex.Message}");
            return false;
        }
    }

    // --- Advanced Queries ---

    public async Task<List<TaskItem>> QueryTasksAsync(TaskQueryFilter filter)
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/tasks/query";
            var json = JsonSerializer.Serialize(filter);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            Debug.WriteLine($"[TaskNotesApi] QueryTasks - URL: {url}");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode) return new List<TaskItem>();

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<ApiResponse>(body, "QueryTasks");
            return result?.Data?.Tasks ?? new List<TaskItem>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] QueryTasks - Exception: {ex.GetType().Name}: {ex.Message}");
            return new List<TaskItem>();
        }
    }

    public async Task<FilterOptions?> GetFilterOptionsAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/filter-options";
            Debug.WriteLine($"[TaskNotesApi] GetFilterOptions - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<FilterOptionsResponse>(body, "GetFilterOptions");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetFilterOptions - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    // --- Statistics ---

    public async Task<TaskStats?> GetStatsAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/stats";
            Debug.WriteLine($"[TaskNotesApi] GetStats - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<TaskStatsResponse>(body, "GetStats");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetStats - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<TimeStats?> GetTimeStatsAsync(string? range = null, string? start = null, string? end = null)
    {
        try
        {
            ConfigureAuthHeader();
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(range)) queryParams.Add($"range={HttpUtility.UrlEncode(range)}");
            if (!string.IsNullOrEmpty(start)) queryParams.Add($"start={HttpUtility.UrlEncode(start)}");
            if (!string.IsNullOrEmpty(end)) queryParams.Add($"end={HttpUtility.UrlEncode(end)}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_settings.ApiBaseUrl}/api/time-stats{query}";
            Debug.WriteLine($"[TaskNotesApi] GetTimeStats - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<TimeStatsResponse>(body, "GetTimeStats");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetTimeStats - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    // --- Pomodoro ---

    public async Task<PomodoroSession?> StartPomodoroAsync(string? taskId = null)
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/start";
            StringContent? content = null;
            if (!string.IsNullOrEmpty(taskId))
            {
                var json = JsonSerializer.Serialize(new { taskId });
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            Debug.WriteLine($"[TaskNotesApi] StartPomodoro - URL: {url}");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<PomodoroActionResponse>(body, "StartPomodoro");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] StartPomodoro - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> StopPomodoroAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/stop";
            Debug.WriteLine($"[TaskNotesApi] StopPomodoro - URL: {url}");
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] StopPomodoro - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PausePomodoroAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/pause";
            Debug.WriteLine($"[TaskNotesApi] PausePomodoro - URL: {url}");
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] PausePomodoro - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ResumePomodoroAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/resume";
            Debug.WriteLine($"[TaskNotesApi] ResumePomodoro - URL: {url}");
            var response = await _httpClient.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] ResumePomodoro - Exception: {ex.Message}");
            return false;
        }
    }

    public async Task<PomodoroStatus?> GetPomodoroStatusAsync()
    {
        try
        {
            ConfigureAuthHeader();
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/status";
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroStatus - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<PomodoroStatusResponse>(body, "GetPomodoroStatus");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroStatus - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<PomodoroSession>> GetPomodoroSessionsAsync(int? limit = null, string? date = null)
    {
        try
        {
            ConfigureAuthHeader();
            var queryParams = new List<string>();
            if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
            if (!string.IsNullOrEmpty(date)) queryParams.Add($"date={HttpUtility.UrlEncode(date)}");
            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/sessions{query}";
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroSessions - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return new List<PomodoroSession>();

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<PomodoroSessionsResponse>(body, "GetPomodoroSessions");
            return result?.Data ?? new List<PomodoroSession>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroSessions - Exception: {ex.GetType().Name}: {ex.Message}");
            return new List<PomodoroSession>();
        }
    }

    public async Task<PomodoroStats?> GetPomodoroStatsAsync(string? date = null)
    {
        try
        {
            ConfigureAuthHeader();
            var query = !string.IsNullOrEmpty(date) ? $"?date={HttpUtility.UrlEncode(date)}" : "";
            var url = $"{_settings.ApiBaseUrl}/api/pomodoro/stats{query}";
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroStats - URL: {url}");
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode) return null;

            var body = await response.Content.ReadAsStringAsync();
            var result = DeserializeResponse<PomodoroStatsResponse>(body, "GetPomodoroStats");
            return result?.Data;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TaskNotesApi] GetPomodoroStats - Exception: {ex.GetType().Name}: {ex.Message}");
            return null;
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
