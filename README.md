# TMX Terminal OS

TMX Terminal OS is a Windows desktop app for saving SSH connection details, generating SSH commands, and launching sessions in your preferred terminal.

## What it does

- Save and manage SSH connection profiles.
- Search saved connections by name, host, user, notes, or command.
- Generate SSH commands from host, user, port, and key file details.
- Launch connections using the selected terminal preference.
- Keep simple metadata such as created and updated timestamps.

## Tech stack

- .NET Framework 4.8
- WPF
- C#

## Project layout

- `TMXWinTerminal/` contains the WPF application source.
- `TMXWinTerminal/ViewModels/` contains the main UI state and commands.
- `TMXWinTerminal/Services/` contains persistence, validation, SSH command building, and terminal launch helpers.
- `TMXWinTerminal/Models/` contains the app data models.

## Getting started

1. Open the project in Visual Studio.
2. Build the app targeting `.NET Framework 4.8`.
3. Run the project and start adding SSH connection entries.

## Repository notes

The `.gitignore` excludes build artifacts and editor-specific files such as `bin/`, `obj/`, `.vs/`, and user-local settings so only the source code and project files are committed.
