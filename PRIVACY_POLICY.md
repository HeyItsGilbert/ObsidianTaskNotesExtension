# Privacy Policy

**Obsidian Task Notes Extension**
**Effective Date:** February 27, 2026

## Overview

Obsidian Task Notes Extension ("the Extension") is a PowerToys Command Palette extension that connects to your self-hosted TaskNotes HTTP API to manage tasks in your Obsidian vault. This privacy policy explains how the Extension handles your data.

## Data Collection

**The Extension does not collect, transmit, or share any personal data with the developer or any third parties.**

Specifically, the Extension:

- Does **not** collect telemetry or usage analytics
- Does **not** send crash reports or diagnostic data
- Does **not** use advertising or tracking technologies
- Does **not** transmit data to any external servers beyond your own self-hosted TaskNotes API

## Data Stored Locally

The Extension stores the following configuration data locally on your device in `%LOCALAPPDATA%\ObsidianTaskNotesExtension\settings.json`:

- **API Base URL** — The address of your self-hosted TaskNotes API (default: `http://localhost:8080`)
- **Authentication Token** — A bearer token you provide for authenticating with your TaskNotes API
- **Vault Name** — The name of your Obsidian vault
- **Icon Mappings** — Custom icon configuration preferences

This data never leaves your device except when the Extension communicates directly with the TaskNotes API server you have configured.

## Network Communication

The Extension communicates exclusively with the TaskNotes HTTP API server that **you** configure. This is typically a locally hosted server running on your own machine or network. The Extension does not contact any other servers or endpoints.

All network requests include your configured authentication token in the HTTP `Authorization` header. You are responsible for securing your TaskNotes API server and authentication credentials.

## Third-Party Services

The Extension does not integrate with or send data to any third-party services, analytics platforms, or advertising networks.

## Children's Privacy

The Extension does not knowingly collect any personal information from anyone, including children under the age of 13.

## Changes to This Policy

If this privacy policy is updated, the revised version will be posted in the project's GitHub repository at [https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension](https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension). The effective date at the top of this policy will be updated accordingly.

## Open Source

The Extension is open source under the MIT License. You can review the complete source code to verify these privacy practices at [https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension](https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension).

## Contact

If you have questions about this privacy policy, please open an issue on the project's GitHub repository:
[https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/issues](https://github.com/HeyItsGilbert/ObsidianTaskNotesExtension/issues)
