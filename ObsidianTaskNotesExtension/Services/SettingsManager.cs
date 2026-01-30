// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ObsidianTaskNotesExtension.Services;

public class ExtensionSettings
{
    public string ApiBaseUrl { get; set; } = "http://localhost:8080";
    public string AuthToken { get; set; } = string.Empty;
    public string VaultName { get; set; } = string.Empty;
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

    public ExtensionSettings GetSettings() => _settings;

    public void SaveSettings(ExtensionSettings settings)
    {
        _settings = settings;

        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

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
                var settings = JsonSerializer.Deserialize<ExtensionSettings>(json);
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
