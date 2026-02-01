# Icon Customization Guide

This guide explains how to customize task icons in the Obsidian TaskNotes Extension.

## Overview

By default, tasks display icons based on their **status** (todo, done, archived, etc.). You can change this to show icons based on:

- **Status** — Task status (todo, done, archived, overdue, in-progress)
- **Priority** — Task priority (urgent, high, medium, normal, low)
- **Project** — Project membership (+work, +home, +personal)
- **Context** — Context tags (@phone, @office, @home)
- **Tag** — Custom tags (#urgent, #bug, #meeting)

## Accessing Icon Settings

1. Open PowerToys Command Palette (`Alt + Space`)
2. Type `Settings` or `Obsidian Settings`
3. Select **Icon Settings**

## Choosing an Icon Source

Navigate to **Icon Settings → Icon Source** to select what determines task icons:

| Source | Description | Example |
|--------|-------------|---------|
| Status | Icons based on task status | ☐ Todo, ✓ Done, 📦 Archived |
| Priority | Icons based on priority level | ❗ Urgent, 🚩 Medium, ☆ Low |
| Project | Icons based on project | 📁 work, 🏠 home |
| Context | Icons based on context | 📱 phone, 💼 office |
| Tag | Icons based on tags | ❗ urgent, ❌ bug |

**Fallback Behavior:** If the selected source has no matching icon for a task, it will automatically fall back to the Status icon.

## Custom Icon Mappings

For advanced customization, navigate to **Icon Settings → Customize Mappings**.

### Format

Enter mappings as `name=Icon` with one mapping per line:

```
work=Folder
home=Home
urgent=Important
```

### Specifying Icons

You can specify icons three ways:

1. **By friendly name:**

   ```
   work=Folder
   phone=Phone
   bug=Error
   ```

2. **By Unicode hex code (4 characters):**

   ```
   work=E821
   phone=E717
   bug=E783
   ```

3. **By Unicode escape sequence:**

   ```
   work=\uE821
   phone=\uE717
   bug=\uE783
   ```

### Common Icon Names

| Category | Available Icons |
|----------|----------------|
| Status | Checkbox, Checkmark, CheckmarkCircle, Warning, Error, Info, Question |
| Priority | Important, Flag, Star, StarFilled, Heart |
| Actions | Play, Pause, Stop, Sync, Clock |
| Organization | Folder, Tag, Category, List, Archive |
| Context | Phone, Mail, Chat, Home, Work, Globe, Location, Car |

### Finding More Icons

All icons come from the **Segoe MDL2 Assets** font included with Windows.

📖 **[Browse all available icons](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)**

To use an icon from the reference:

1. Find the icon you want on the Microsoft documentation page
2. Note the Unicode code point (e.g., `E721` for Camera)
3. Use it in your mapping: `photos=E721`

## Import/Export Settings

You can share icon configurations between machines:

1. **Export:** Go to **Icon Settings → Import/Export → Export Mappings**
   - Saves to `Documents/obsidian-tasknotes-icon-mappings.json`

2. **Import:** Go to **Icon Settings → Import/Export → Import Mappings**
   - Reads from `Documents/obsidian-tasknotes-icon-mappings.json`

3. **Reset:** Go to **Icon Settings → Import/Export → Reset to Defaults**
   - Restores all default icon mappings

## Example Configurations

### Project-Based Icons

Set **Icon Source** to `Project`, then add custom mappings:

```
work=Work
home=Home
personal=Contact
client=People
opensource=Globe
```

### Context-Based Icons

Set **Icon Source** to `Context`, then add custom mappings:

```
phone=Phone
email=Mail
office=Work
home=Home
computer=Devices
errands=Car
```

### Priority-Based with Custom Icons

Set **Icon Source** to `Priority` (uses built-in defaults):

| Priority | Default Icon |
|----------|-------------|
| Urgent/High | ❗ Important |
| Medium | 🚩 Flag |
| Normal | ★ StarFilled |
| Low | ☆ Star |

## Troubleshooting

### Icons not changing?

Settings are persisted, so after updating the code/extension you may need to:

- Go to **Reset to Defaults** to get new default mappings
- Or manually update your custom mappings

### Icon shows as a box (□)?

The Unicode code may be invalid. Verify:

- The code is exactly 4 hex characters (0-9, A-F)
- The icon exists in the [Segoe MDL2 Assets font](https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)

### Fallback not working?

Ensure your Status icons are configured. The fallback always uses Status icons when the primary source has no match.
