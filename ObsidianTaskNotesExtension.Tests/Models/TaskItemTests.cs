// Copyright (c) 2025 Gilbert Sanchez
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ObsidianTaskNotesExtension.Models;

namespace ObsidianTaskNotesExtension.Tests.Models;

public class TaskItemTests
{
  [Fact]
  public void Id_ReturnsPath()
  {
    var task = new TaskItem { Path = "TaskNotes/Tasks/Test.md" };

    task.Id.Should().Be("TaskNotes/Tasks/Test.md");
  }

  [Theory]
  [InlineData("2025-01-15", 2025, 1, 15)]
  [InlineData("2024-12-31", 2024, 12, 31)]
  public void Due_ParsesValidDateString(string dueString, int year, int month, int day)
  {
    var task = new TaskItem { DueString = dueString };

    task.Due.Should().NotBeNull();
    task.Due!.Value.Year.Should().Be(year);
    task.Due!.Value.Month.Should().Be(month);
    task.Due!.Value.Day.Should().Be(day);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("invalid-date")]
  [InlineData("not a date")]
  public void Due_ReturnsNullForInvalidDates(string? dueString)
  {
    var task = new TaskItem { DueString = dueString };

    task.Due.Should().BeNull();
  }

  [Theory]
  [InlineData("done", true)]
  [InlineData("Done", true)]
  [InlineData("DONE", true)]
  [InlineData("completed", true)]
  [InlineData("Completed", true)]
  [InlineData("COMPLETED", true)]
  [InlineData("todo", false)]
  [InlineData("in-progress", false)]
  [InlineData("", false)]
  public void Completed_IdentifiesCompletedStatuses(string status, bool expected)
  {
    var task = new TaskItem { Status = status };

    task.Completed.Should().Be(expected);
  }

  [Theory]
  [InlineData("archived", true)]
  [InlineData("Archived", true)]
  [InlineData("ARCHIVED", true)]
  [InlineData("done", false)]
  [InlineData("todo", false)]
  [InlineData("", false)]
  public void Archived_IdentifiesArchivedStatus(string status, bool expected)
  {
    var task = new TaskItem { Status = status };

    task.Archived.Should().Be(expected);
  }

  [Fact]
  public void IsOverdue_ReturnsTrueForPastDueDateAndNotCompleted()
  {
    var task = new TaskItem
    {
      Status = "todo",
      DueString = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")
    };

    task.IsOverdue.Should().BeTrue();
  }

  [Fact]
  public void IsOverdue_ReturnsFalseForPastDueDateWhenCompleted()
  {
    var task = new TaskItem
    {
      Status = "done",
      DueString = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd")
    };

    task.IsOverdue.Should().BeFalse();
  }

  [Fact]
  public void IsOverdue_ReturnsFalseForFutureDueDate()
  {
    var task = new TaskItem
    {
      Status = "todo",
      DueString = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")
    };

    task.IsOverdue.Should().BeFalse();
  }

  [Fact]
  public void IsOverdue_ReturnsFalseForTodayDueDate()
  {
    var task = new TaskItem
    {
      Status = "todo",
      DueString = DateTime.Today.ToString("yyyy-MM-dd")
    };

    task.IsOverdue.Should().BeFalse();
  }

  [Fact]
  public void IsOverdue_ReturnsFalseWhenNoDueDate()
  {
    var task = new TaskItem { Status = "todo", DueString = null };

    task.IsOverdue.Should().BeFalse();
  }

  [Fact]
  public void IsDueToday_ReturnsTrueForTodaysDueDate()
  {
    var task = new TaskItem
    {
      DueString = DateTime.Today.ToString("yyyy-MM-dd")
    };

    task.IsDueToday.Should().BeTrue();
  }

  [Fact]
  public void IsDueToday_ReturnsFalseForOtherDates()
  {
    var task = new TaskItem
    {
      DueString = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")
    };

    task.IsDueToday.Should().BeFalse();
  }

  [Fact]
  public void IsDueTomorrow_ReturnsTrueForTomorrowsDueDate()
  {
    var task = new TaskItem
    {
      DueString = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")
    };

    task.IsDueTomorrow.Should().BeTrue();
  }

  [Fact]
  public void IsDueTomorrow_ReturnsFalseForOtherDates()
  {
    var task = new TaskItem
    {
      DueString = DateTime.Today.ToString("yyyy-MM-dd")
    };

    task.IsDueTomorrow.Should().BeFalse();
  }

  [Fact]
  public void Modified_ParsesValidDateTimeString()
  {
    var task = new TaskItem
    {
      DateModified = "2025-01-15T10:30:00Z"
    };

    task.Modified.Should().NotBeNull();
    task.Modified!.Value.Year.Should().Be(2025);
    task.Modified!.Value.Month.Should().Be(1);
    task.Modified!.Value.Day.Should().Be(15);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("invalid")]
  public void Modified_ReturnsNullForInvalidValues(string? dateModified)
  {
    var task = new TaskItem { DateModified = dateModified };

    task.Modified.Should().BeNull();
  }

  [Fact]
  public void CompletedToday_ReturnsTrueWhenCompletedAndModifiedToday()
  {
    var task = new TaskItem
    {
      Status = "done",
      // Use ISO 8601 format that DateTime.TryParse can parse
      DateModified = DateTime.Today.AddHours(10).ToString("o")
    };

    task.CompletedToday.Should().BeTrue();
  }

  [Fact]
  public void CompletedToday_ReturnsFalseWhenNotCompleted()
  {
    var task = new TaskItem
    {
      Status = "todo",
      DateModified = DateTime.Today.AddHours(10).ToString("o")
    };

    task.CompletedToday.Should().BeFalse();
  }

  [Fact]
  public void CompletedToday_ReturnsFalseWhenCompletedOnDifferentDay()
  {
    var task = new TaskItem
    {
      Status = "done",
      DateModified = DateTime.Today.AddDays(-1).AddHours(10).ToString("o")
    };

    task.CompletedToday.Should().BeFalse();
  }
}
