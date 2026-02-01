// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Services;

public class ExtensionSettings
{
    public string ApiBaseUrl { get; set; } = "http://localhost:8080";
    public string AuthToken { get; set; } = string.Empty;
    public string VaultName { get; set; } = string.Empty;
    public IconMappingConfig IconMappings { get; set; } = new();
}

public class SettingsManager
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ObsidianTaskNotesExtension");

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

    private ExtensionSettings _settings;

    public SettingsManager()
    {
        Debug.WriteLine($"[SettingsManager] Loading settings from: {SettingsFilePath}");
        _settings = LoadSettings();
        Debug.WriteLine($"[SettingsManager] Loaded - ApiBaseUrl: '{_settings.ApiBaseUrl}', AuthToken: '{(string.IsNullOrEmpty(_settings.AuthToken) ? "(empty)" : "(set)")}', VaultName: '{_settings.VaultName}'");
    }

    public string ApiBaseUrl => _settings.ApiBaseUrl;
    public string AuthToken => _settings.AuthToken;
    public string VaultName => _settings.VaultName;
    public IconMappingConfig IconMappings => _settings.IconMappings;

    public ExtensionSettings GetSettings() => _settings;

    /// <summary>
    /// Updates the icon mapping configuration.
    /// </summary>
    public void UpdateIconMappings(IconMappingConfig iconMappings)
    {
        _settings.IconMappings = iconMappings;
        SaveSettings(_settings);
    }

    /// <summary>
    /// Exports icon mappings to a JSON file.
    /// </summary>
    /// <param name="filePath">Path to export the mappings to.</param>
    /// <returns>True if export succeeded, false otherwise.</returns>
    public bool ExportIconMappings(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings.IconMappings, TaskNotesJsonContext.Default.IconMappingConfig);
            File.WriteAllText(filePath, json);
            Debug.WriteLine($"[SettingsManager] Exported icon mappings to: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsManager] Failed to export icon mappings: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Imports icon mappings from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to import the mappings from.</param>
    /// <returns>True if import succeeded, false otherwise.</returns>
    public bool ImportIconMappings(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.WriteLine($"[SettingsManager] Import file not found: {filePath}");
                return false;
            }

            var json = File.ReadAllText(filePath);
            var iconMappings = JsonSerializer.Deserialize(json, TaskNotesJsonContext.Default.IconMappingConfig);

            if (iconMappings != null)
            {
                _settings.IconMappings = iconMappings;
                SaveSettings(_settings);
                Debug.WriteLine($"[SettingsManager] Imported icon mappings from: {filePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsManager] Failed to import icon mappings: {ex.Message}");
            return false;
        }
    }

    public void SaveSettings(ExtensionSettings settings)
    {
        _settings = settings;

        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            var json = JsonSerializer.Serialize(settings, TaskNotesJsonContext.Default.ExtensionSettings);

            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception)
        {
            // Silently fail if we can't save settings
        }
    }

    public void UpdateApiBaseUrl(string url)
    {
        _settings.ApiBaseUrl = url;
        SaveSettings(_settings);
    }

    public void UpdateAuthToken(string token)
    {
        _settings.AuthToken = token;
        SaveSettings(_settings);
    }

    public void UpdateVaultName(string vaultName)
    {
        _settings.VaultName = vaultName;
        SaveSettings(_settings);
    }

    private static ExtensionSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                Debug.WriteLine($"[SettingsManager] Settings file contents: {json}");
                var settings = JsonSerializer.Deserialize<ExtensionSettings>(json, TaskNotesJsonContext.Default.ExtensionSettings);
                return settings ?? new ExtensionSettings();
            }

            Debug.WriteLine("[SettingsManager] No settings file found, using defaults");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsManager] Error loading settings: {ex.Message}");
        }

        return new ExtensionSettings();
    }
}
