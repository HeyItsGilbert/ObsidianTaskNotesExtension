// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Pages;

/// <summary>
/// Settings page for configuring task icon mappings.
/// </summary>
internal sealed partial class IconMappingSettingsPage : ListPage
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;

  public IconMappingSettingsPage(SettingsManager settingsManager, IconMappingService iconMappingService)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;

    Icon = new IconInfo("\uE790"); // Personalize icon
    Title = "Icon Settings";
    Name = "Icon Settings";
  }

  public override IListItem[] GetItems()
  {
    var currentSource = _settingsManager.IconMappings.PrimaryIconSource;
    var sourceDescription = currentSource switch
    {
      IconPriority.Status => "Task status (todo, done, archived, etc.)",
      IconPriority.Priority => "Task priority (urgent, high, medium, etc.)",
      IconPriority.Project => "Project membership (+work, +home, etc.)",
      IconPriority.Context => "Context (@phone, @office, etc.)",
      IconPriority.Tag => "Tags (#urgent, #bug, etc.)",
      _ => "Status"
    };

    var iconSourcePage = new IconSourceSelectionPage(_settingsManager, _iconMappingService, () => RaiseItemsChanged());
    var customMappingPage = new CustomIconMappingPage(_settingsManager, _iconMappingService);
    var importExportPage = new IconMappingImportExportPage(_settingsManager, _iconMappingService);

    return
    [
      new ListItem(iconSourcePage)
      {
        Title = "Icon Source",
        Subtitle = $"Current: {sourceDescription}",
        Icon = new IconInfo("\uE771") // Tiles icon
      },
      new ListItem(customMappingPage)
      {
        Title = "Customize Mappings",
        Subtitle = "Advanced: Add custom icons for specific values",
        Icon = new IconInfo("\uE70F") // Edit icon
      },
      new ListItem(importExportPage)
      {
        Title = "Import / Export",
        Subtitle = "Share icon configurations via JSON files",
        Icon = new IconInfo("\uE8B5") // Download/Upload icon
      }
    ];
  }
}

/// <summary>
/// Page for selecting the primary icon source.
/// </summary>
internal sealed partial class IconSourceSelectionPage : ContentPage
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;
  private readonly Action _refreshCallback;

  public IconSourceSelectionPage(SettingsManager settingsManager, IconMappingService iconMappingService, Action refreshCallback)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;
    _refreshCallback = refreshCallback;

    Icon = new IconInfo("\uE771");
    Title = "Icon Source";
    Name = "Icon Source";
  }

  public override IContent[] GetContent()
  {
    return [new IconSourceSelectionFormContent(_settingsManager, _iconMappingService, _refreshCallback)];
  }
}

/// <summary>
/// Form content for selecting the primary icon source.
/// </summary>
internal sealed partial class IconSourceSelectionFormContent : FormContent
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;
  private readonly Action _refreshCallback;

  public IconSourceSelectionFormContent(SettingsManager settingsManager, IconMappingService iconMappingService, Action refreshCallback)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;
    _refreshCallback = refreshCallback;

    var config = _settingsManager.IconMappings;
    var currentSource = config.PrimaryIconSource.ToString();

    TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Choose Icon Source",
                    "weight": "bolder",
                    "size": "medium"
                },
                {
                    "type": "TextBlock",
                    "text": "Select which task field determines the icon shown in task lists. If the selected source has no matching icon, it will fall back to Status.",
                    "wrap": true,
                    "spacing": "small"
                },
                {
                    "type": "Input.ChoiceSet",
                    "id": "primarySource",
                    "label": "Primary Icon Source",
                    "value": "{{currentSource}}",
                    "choices": [
                        { "title": "📋 Status — Show icons based on task status (todo, done, archived, overdue)", "value": "Status" },
                        { "title": "⚡ Priority — Show icons based on task priority (urgent, high, medium, low)", "value": "Priority" },
                        { "title": "📁 Project — Show icons based on project (+work, +home, +personal)", "value": "Project" },
                        { "title": "📍 Context — Show icons based on context (@phone, @office, @home)", "value": "Context" },
                        { "title": "🏷 Tag — Show icons based on tags (#urgent, #bug, #meeting)", "value": "Tag" }
                    ]
                },
                {
                    "type": "TextBlock",
                    "text": "Examples of how tasks will appear:",
                    "weight": "bolder",
                    "spacing": "large"
                },
                {
                    "type": "FactSet",
                    "spacing": "small",
                    "facts": [
                        { "title": "Status", "value": "☐ Todo  ·  ✓ Done  ·  📦 Archived  ·  ⚠️ Overdue" },
                        { "title": "Priority", "value": "❗ Urgent  ·  🚩 High  ·  ★ Medium  ·  ☆ Normal" },
                        { "title": "Project", "value": "📁 work  ·  🏠 home  ·  👤 personal" },
                        { "title": "Context", "value": "📱 phone  ·  ✉ email  ·  💼 office  ·  🏠 home" },
                        { "title": "Tag", "value": "❗ urgent  ·  ❌ bug  ·  💡 idea  ·  👥 meeting" }
                    ]
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Save",
                    "data": { "action": "save" }
                }
            ]
        }
        """;
  }

  public override CommandResult SubmitForm(string payload)
  {
    var formInput = JsonNode.Parse(payload)?.AsObject();
    if (formInput == null) return CommandResult.KeepOpen();

    var primarySourceStr = formInput["primarySource"]?.GetValue<string>() ?? "Status";

    if (Enum.TryParse<IconPriority>(primarySourceStr, out var primarySource))
    {
      var config = _settingsManager.IconMappings.Clone();
      config.PrimaryIconSource = primarySource;
      _settingsManager.UpdateIconMappings(config);
      // SettingsManager updated - IconMappingService reads from it on each call

      Debug.WriteLine($"[IconMappingSettings] Set primary icon source to: {primarySource}");
      _refreshCallback?.Invoke();
    }

    return CommandResult.GoBack();
  }
}



/// <summary>
/// Form page for adding custom project/context/tag icon mappings.
/// </summary>
internal sealed partial class CustomIconMappingPage : ContentPage
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;

  public CustomIconMappingPage(SettingsManager settingsManager, IconMappingService iconMappingService)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;

    Icon = new IconInfo("\uE8EC");
    Title = "Custom Mappings";
    Name = "Custom Mappings";
  }

  public override IContent[] GetContent()
  {
    return [new CustomIconMappingFormContent(_settingsManager, _iconMappingService)];
  }
}

/// <summary>
/// Form content for custom icon mappings (projects, contexts, tags).
/// </summary>
internal sealed partial class CustomIconMappingFormContent : FormContent
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;

  public CustomIconMappingFormContent(SettingsManager settingsManager, IconMappingService iconMappingService)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;

    var config = _settingsManager.IconMappings;

    // Serialize current mappings for display
    var projectMappings = FormatMappingsForDisplay(config.ProjectIcons);
    var contextMappings = FormatMappingsForDisplay(config.ContextIcons);
    var tagMappings = FormatMappingsForDisplay(config.TagIcons);

    // Build a short list of common icon names for quick reference
    var commonIcons = "Folder, Home, Phone, Mail, Work, Star, Flag, Important, Warning, Error, Info, Tag, Archive, Clock, Globe, Car, Chat";

    TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "TextBlock",
                    "text": "Custom Icon Mappings",
                    "weight": "bolder",
                    "size": "medium"
                },
                {
                    "type": "TextBlock",
                    "text": "Add custom icons for specific projects, contexts, or tags.",
                    "wrap": true,
                    "spacing": "small"
                },
                {
                    "type": "TextBlock",
                    "text": "Format: name=Icon (one per line)",
                    "wrap": true,
                    "spacing": "small",
                    "weight": "bolder"
                },
                {
                    "type": "FactSet",
                    "spacing": "small",
                    "facts": [
                        { "title": "By Name", "value": "work=Folder" },
                        { "title": "By Unicode", "value": "urgent=E7C1 or urgent=\\uE7C1" }
                    ]
                },
                {
                    "type": "TextBlock",
                    "text": "Common icons: {{commonIcons}}",
                    "wrap": true,
                    "size": "small",
                    "isSubtle": true,
                    "spacing": "small"
                },
                {
                    "type": "TextBlock",
                    "text": "📖 [Browse all Segoe MDL2 icons](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)",
                    "wrap": true,
                    "size": "small",
                    "spacing": "small"
                },
                {
                    "type": "Input.Text",
                    "id": "projectMappings",
                    "label": "Project Icons (without + prefix)",
                    "placeholder": "work=Folder\nhome=Home\nclient=E716",
                    "isMultiline": true,
                    "value": "{{projectMappings}}"
                },
                {
                    "type": "Input.Text",
                    "id": "contextMappings",
                    "label": "Context Icons (without @ prefix)",
                    "placeholder": "phone=Phone\noffice=Work\nerrands=Car",
                    "isMultiline": true,
                    "value": "{{contextMappings}}"
                },
                {
                    "type": "Input.Text",
                    "id": "tagMappings",
                    "label": "Tag Icons",
                    "placeholder": "urgent=Important\nbug=Error\nidea=E82F",
                    "isMultiline": true,
                    "value": "{{tagMappings}}"
                }
            ],
            "actions": [
                {
                    "type": "Action.Submit",
                    "title": "Save",
                    "data": { "action": "save" }
                }
            ]
        }
        """;
  }

  private static string FormatMappingsForDisplay(Dictionary<string, string> mappings)
  {
    var lines = new List<string>();
    foreach (var kvp in mappings)
    {
      // Try to get friendly name, otherwise show hex code without \u prefix
      var iconName = IconPalette.GetNameForCode(kvp.Value);
      if (iconName != null)
      {
        lines.Add($"{kvp.Key}={iconName}");
      }
      else
      {
        // Show as hex code (E7C1 format)
        var hexCode = ((int)kvp.Value[0]).ToString("X4", System.Globalization.CultureInfo.InvariantCulture);
        lines.Add($"{kvp.Key}={hexCode}");
      }
    }
    return string.Join("\\n", lines);
  }

  public override CommandResult SubmitForm(string payload)
  {
    var formInput = JsonNode.Parse(payload)?.AsObject();
    if (formInput == null) return CommandResult.KeepOpen();

    var config = _settingsManager.IconMappings.Clone();

    // Parse and update project mappings
    ParseMappings(formInput["projectMappings"]?.GetValue<string>(), config.ProjectIcons);
    ParseMappings(formInput["contextMappings"]?.GetValue<string>(), config.ContextIcons);
    ParseMappings(formInput["tagMappings"]?.GetValue<string>(), config.TagIcons);

    _settingsManager.UpdateIconMappings(config);
    Debug.WriteLine("[IconMappingSettings] Saved custom icon mappings");

    return CommandResult.GoBack();
  }

  private static void ParseMappings(string? input, Dictionary<string, string> target)
  {
    target.Clear();

    if (string.IsNullOrWhiteSpace(input)) return;

    // Handle both actual newlines and escaped newlines from Adaptive Cards
    var lines = input.Replace("\\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);

    foreach (var line in lines)
    {
      var parts = line.Split('=', 2);
      if (parts.Length != 2) continue;

      var name = parts[0].Trim();
      var iconValue = parts[1].Trim();

      if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(iconValue)) continue;

      // Try to resolve icon name to code (e.g., "Folder" -> "\uE821")
      if (IconPalette.Icons.TryGetValue(iconValue, out var code))
      {
        target[name] = code;
      }
      // Try to parse as hex code with \u prefix (e.g., "\uE7C1")
      else if (iconValue.StartsWith("\\u", StringComparison.OrdinalIgnoreCase) &&
               iconValue.Length == 6 &&
               int.TryParse(iconValue[2..], System.Globalization.NumberStyles.HexNumber, null, out var unicodeValue1))
      {
        target[name] = ((char)unicodeValue1).ToString();
      }
      // Try to parse as hex code without prefix (e.g., "E7C1")
      else if (iconValue.Length == 4 &&
               int.TryParse(iconValue, System.Globalization.NumberStyles.HexNumber, null, out var unicodeValue2))
      {
        target[name] = ((char)unicodeValue2).ToString();
      }
      // Allow raw MDL2 codes for power users (single character in PUA range)
      else if (IconPalette.IsValidIconCode(iconValue))
      {
        target[name] = iconValue;
      }
      // Otherwise skip invalid entries
      else
      {
        Debug.WriteLine($"[IconMappingSettings] Skipping invalid icon value: {iconValue}");
      }
    }
  }
}

/// <summary>
/// Page for importing and exporting icon mapping configurations.
/// </summary>
internal sealed partial class IconMappingImportExportPage : ListPage
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;

  public IconMappingImportExportPage(SettingsManager settingsManager, IconMappingService iconMappingService)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;

    Icon = new IconInfo("\uE8B5");
    Title = "Import / Export";
    Name = "Import / Export";
  }

  public override IListItem[] GetItems()
  {
    var exportCommand = new ExportIconMappingsCommand(_settingsManager);
    var importCommand = new ImportIconMappingsCommand(_settingsManager, _iconMappingService, () => RaiseItemsChanged());
    var resetCommand = new ResetIconMappingsCommand(_settingsManager, _iconMappingService, () => RaiseItemsChanged());

    return
    [
        new ListItem(exportCommand)
            {
                Title = "Export Mappings",
                Subtitle = "Save icon configuration to a JSON file",
                Icon = new IconInfo("\uE898") // Save icon
            },
            new ListItem(importCommand)
            {
                Title = "Import Mappings",
                Subtitle = "Load icon configuration from a JSON file",
                Icon = new IconInfo("\uE8E5") // Open file icon
            },
            new ListItem(resetCommand)
            {
                Title = "Reset to Defaults",
                Subtitle = "Restore default icon mappings",
                Icon = new IconInfo("\uE72C") // Refresh icon
            }
    ];
  }
}

/// <summary>
/// Command to export icon mappings to a file.
/// </summary>
internal sealed partial class ExportIconMappingsCommand : InvokableCommand
{
  private readonly SettingsManager _settingsManager;

  public ExportIconMappingsCommand(SettingsManager settingsManager)
  {
    _settingsManager = settingsManager;
    Name = "Export Mappings";
    Icon = new IconInfo("\uE898");
  }

  public override CommandResult Invoke()
  {
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var filePath = Path.Combine(documentsPath, "obsidian-tasknotes-icon-mappings.json");

    var success = _settingsManager.ExportIconMappings(filePath);

    if (success)
    {
      Debug.WriteLine($"[IconMappingSettings] Exported to: {filePath}");
      // Open the folder to show the file
      try
      {
        Process.Start("explorer.exe", $"/select,\"{filePath}\"");
      }
      catch
      {
        // Silently fail if explorer cannot be opened
      }
    }

    return CommandResult.KeepOpen();
  }
}

/// <summary>
/// Command to import icon mappings from a file.
/// </summary>
internal sealed partial class ImportIconMappingsCommand : InvokableCommand
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;
  private readonly Action _refreshCallback;

  public ImportIconMappingsCommand(SettingsManager settingsManager, IconMappingService iconMappingService, Action refreshCallback)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;
    _refreshCallback = refreshCallback;
    Name = "Import Mappings";
    Icon = new IconInfo("\uE8E5");
  }

  public override CommandResult Invoke()
  {
    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    var filePath = Path.Combine(documentsPath, "obsidian-tasknotes-icon-mappings.json");

    if (_settingsManager.ImportIconMappings(filePath))
    {
      // SettingsManager updated - IconMappingService reads from it on each call
      Debug.WriteLine($"[IconMappingSettings] Imported from: {filePath}");
      _refreshCallback?.Invoke();
    }
    else
    {
      Debug.WriteLine($"[IconMappingSettings] Import failed or file not found: {filePath}");
    }

    return CommandResult.KeepOpen();
  }
}

/// <summary>
/// Command to reset icon mappings to defaults.
/// </summary>
internal sealed partial class ResetIconMappingsCommand : InvokableCommand
{
  private readonly SettingsManager _settingsManager;
  private readonly IconMappingService _iconMappingService;
  private readonly Action _refreshCallback;

  public ResetIconMappingsCommand(SettingsManager settingsManager, IconMappingService iconMappingService, Action refreshCallback)
  {
    _settingsManager = settingsManager;
    _iconMappingService = iconMappingService;
    _refreshCallback = refreshCallback;
    Name = "Reset to Defaults";
    Icon = new IconInfo("\uE72C");
  }

  public override CommandResult Invoke()
  {
    var defaultConfig = new IconMappingConfig();
    _settingsManager.UpdateIconMappings(defaultConfig);
    // SettingsManager updated - IconMappingService reads from it on each call

    Debug.WriteLine("[IconMappingSettings] Reset to defaults");
    _refreshCallback?.Invoke();

    return CommandResult.KeepOpen();
  }
}
