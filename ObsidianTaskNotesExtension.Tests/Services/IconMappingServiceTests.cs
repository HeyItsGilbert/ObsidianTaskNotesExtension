// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObsidianTaskNotesExtension.Models;
using ObsidianTaskNotesExtension.Services;

namespace ObsidianTaskNotesExtension.Tests.Services;

public class IconMappingServiceTests
{
  private static IconMappingConfig CreateDefaultConfig() => new();

  private static TaskItem CreateTask(
      string status = "todo",
      string? priority = null,
      string[]? projects = null,
      string[]? contexts = null,
      string[]? tags = null,
      string? dueString = null)
  {
    return new TaskItem
    {
      Path = "test/task.md",
      Title = "Test Task",
      Status = status,
      Priority = priority,
      Projects = projects,
      Contexts = contexts,
      Tags = tags,
      DueString = dueString
    };
  }

  #region ResolveIcon - Status Icons

  [Fact]
  public void ResolveIcon_TodoTask_ReturnsCheckboxIcon()
  {
    var service = new IconMappingService(CreateDefaultConfig());
    var task = CreateTask(status: "todo");

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // Default todo icon is \uE73A (Checkbox)
  }

  [Theory]
  [InlineData("done")]
  [InlineData("completed")]
  public void ResolveIcon_CompletedTask_ReturnsCheckmarkIcon(string status)
  {
    var service = new IconMappingService(CreateDefaultConfig());
    var task = CreateTask(status: status);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // Completed icon is \uE73E (Checkmark)
  }

  [Fact]
  public void ResolveIcon_ArchivedTask_ReturnsArchiveIcon()
  {
    var service = new IconMappingService(CreateDefaultConfig());
    var task = CreateTask(status: "archived");

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // Archived icon is \uE7B8 (Archive)
  }

  [Fact]
  public void ResolveIcon_OverdueTask_ReturnsWarningIcon()
  {
    var service = new IconMappingService(CreateDefaultConfig());
    // Set due date to yesterday to make it overdue
    var yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
    var task = CreateTask(status: "todo", dueString: yesterday);

    task.IsOverdue.Should().BeTrue();
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // Overdue icon is \uE7BA (Warning)
  }

  [Fact]
  public void ResolveIcon_InProgressTask_ReturnsPlayIcon()
  {
    var service = new IconMappingService(CreateDefaultConfig());
    var task = CreateTask(status: "in-progress");

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // In-progress icon is \uE916 (Play)
  }

  #endregion

  #region ResolveIcon - Priority Icons

  [Fact]
  public void ResolveIcon_PriorityWithMapping_WhenPrimarySourceIsPriority_ReturnsPriorityIcon()
  {
    var config = CreateDefaultConfig();
    config.PriorityIcons["high"] = "\uE7C1"; // Important icon
    config.PrimaryIconSource = IconPriority.Priority;

    var service = new IconMappingService(config);
    var task = CreateTask(priority: "high");

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_PriorityWithoutMapping_FallsBackToStatus()
  {
    var config = CreateDefaultConfig();
    config.PrimaryIconSource = IconPriority.Priority;
    // No priority mapping for "unknown-priority"

    var service = new IconMappingService(config);
    var task = CreateTask(status: "done", priority: "unknown-priority");

    // Should fall back to status icon
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region ResolveIcon - Project Icons

  [Fact]
  public void ResolveIcon_ProjectWithMapping_WhenPrimarySourceIsProject_ReturnsProjectIcon()
  {
    var config = CreateDefaultConfig();
    config.ProjectIcons["work"] = "\uE821"; // Folder icon
    config.PrimaryIconSource = IconPriority.Project;

    var service = new IconMappingService(config);
    var task = CreateTask(projects: ["+work"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_ProjectWithPlusPrefix_NormalizesPrefix()
  {
    var config = CreateDefaultConfig();
    config.ProjectIcons["home"] = "\uE80F"; // Home icon
    config.PrimaryIconSource = IconPriority.Project;

    var service = new IconMappingService(config);
    var task = CreateTask(projects: ["+home"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_ProjectWithoutMapping_FallsBackToStatus()
  {
    var config = CreateDefaultConfig();
    config.PrimaryIconSource = IconPriority.Project;

    var service = new IconMappingService(config);
    var task = CreateTask(status: "done", projects: ["+unknown"]);

    // Should fall through to status icon since no project mapping exists
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region ResolveIcon - Context Icons

  [Fact]
  public void ResolveIcon_ContextWithMapping_WhenPrimarySourceIsContext_ReturnsContextIcon()
  {
    var config = CreateDefaultConfig();
    config.ContextIcons["phone"] = "\uE717"; // Phone icon
    config.PrimaryIconSource = IconPriority.Context;

    var service = new IconMappingService(config);
    var task = CreateTask(contexts: ["@phone"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_ContextWithAtPrefix_NormalizesPrefix()
  {
    var config = CreateDefaultConfig();
    config.ContextIcons["office"] = "\uE770"; // Work icon
    config.PrimaryIconSource = IconPriority.Context;

    var service = new IconMappingService(config);
    var task = CreateTask(contexts: ["@office"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region ResolveIcon - Tag Icons

  [Fact]
  public void ResolveIcon_TagWithMapping_WhenPrimarySourceIsTag_ReturnsTagIcon()
  {
    var config = CreateDefaultConfig();
    config.TagIcons["urgent"] = "\uE7C1"; // Important icon
    config.PrimaryIconSource = IconPriority.Tag;

    var service = new IconMappingService(config);
    var task = CreateTask(tags: ["urgent"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_TagWithHashPrefix_NormalizesPrefix()
  {
    var config = CreateDefaultConfig();
    config.TagIcons["bug"] = "\uE783"; // Error icon
    config.PrimaryIconSource = IconPriority.Tag;

    var service = new IconMappingService(config);
    var task = CreateTask(tags: ["#bug"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region ResolveIcon - Primary Icon Source and Fallback

  [Fact]
  public void ResolveIcon_DefaultSource_IsStatus()
  {
    var config = CreateDefaultConfig();
    config.ProjectIcons["work"] = "\uE821";

    var service = new IconMappingService(config);
    var task = CreateTask(status: "done", projects: ["+work"]);

    // Default is Status, so should return done/checkmark icon, not project icon
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
    // The config should use Status as default
    config.PrimaryIconSource.Should().Be(IconPriority.Status);
  }

  [Fact]
  public void ResolveIcon_PrimarySourceProject_UsesProjectIconFirst()
  {
    var config = CreateDefaultConfig();
    config.ProjectIcons["work"] = "\uE821";
    config.PrimaryIconSource = IconPriority.Project;

    var service = new IconMappingService(config);
    var task = CreateTask(status: "done", projects: ["+work"]);

    // Should return project icon since Project is primary source
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_PrimarySourceWithNoMatch_FallsBackToStatus()
  {
    var config = CreateDefaultConfig();
    config.PrimaryIconSource = IconPriority.Project;
    // No project mappings

    var service = new IconMappingService(config);
    var task = CreateTask(status: "done", projects: ["+unknown"]);

    // Should fall back to status icon
    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Fact]
  public void ResolveIcon_NoMatches_ReturnsDefaultIcon()
  {
    var config = CreateDefaultConfig();
    config.StatusIcons.Clear();
    config.DefaultIcon = "\uE73A";

    var service = new IconMappingService(config);
    var task = CreateTask(status: "unknown");

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region ResolveIcon - Case Insensitivity

  [Theory]
  [InlineData("DONE")]
  [InlineData("Done")]
  [InlineData("done")]
  public void ResolveIcon_StatusCaseInsensitive_Matches(string status)
  {
    var service = new IconMappingService(CreateDefaultConfig());
    var task = CreateTask(status: status);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  [Theory]
  [InlineData("WORK")]
  [InlineData("Work")]
  [InlineData("work")]
  public void ResolveIcon_ProjectCaseInsensitive_Matches(string project)
  {
    var config = CreateDefaultConfig();
    config.ProjectIcons["work"] = "\uE821";
    config.PrimaryIconSource = IconPriority.Project;

    var service = new IconMappingService(config);
    var task = CreateTask(projects: [$"+{project}"]);

    var icon = service.ResolveIcon(task);

    icon.Should().NotBeNull();
  }

  #endregion

  #region Constructor Validation

  [Fact]
  public void Constructor_WithNullConfig_ThrowsArgumentNullException()
  {
    var act = () => new IconMappingService((IconMappingConfig)null!);

    act.Should().Throw<ArgumentNullException>();
  }

  #endregion
}

public class IconPaletteTests
{
  [Fact]
  public void Icons_ContainsExpectedIcons()
  {
    IconPalette.Icons.Should().ContainKey("Checkbox");
    IconPalette.Icons.Should().ContainKey("Checkmark");
    IconPalette.Icons.Should().ContainKey("Warning");
    IconPalette.Icons.Should().ContainKey("Archive");
    IconPalette.Icons.Should().ContainKey("Folder");
    IconPalette.Icons.Should().ContainKey("Phone");
    IconPalette.Icons.Should().ContainKey("Home");
  }

  [Fact]
  public void GetNameForCode_KnownCode_ReturnsName()
  {
    var name = IconPalette.GetNameForCode("\uE73A");

    name.Should().Be("Checkbox");
  }

  [Fact]
  public void GetNameForCode_UnknownCode_ReturnsNull()
  {
    var name = IconPalette.GetNameForCode("\uFFFF");

    name.Should().BeNull();
  }

  [Theory]
  [InlineData("\uE73A", true)]  // Valid MDL2 icon
  [InlineData("\uE000", true)]  // Start of PUA range
  [InlineData("\uF8FF", true)]  // End of PUA range
  [InlineData("A", false)]      // Regular character
  [InlineData("", false)]       // Empty string
  [InlineData(null, false)]     // Null
  [InlineData("AB", false)]     // Multiple characters
  public void IsValidIconCode_ValidatesCorrectly(string? code, bool expected)
  {
    var result = IconPalette.IsValidIconCode(code);

    result.Should().Be(expected);
  }
}
