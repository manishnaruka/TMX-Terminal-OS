using System;
using System.Diagnostics;
using System.IO;
using TMXWinTerminal.Models;

namespace TMXWinTerminal.Services
{
    public class TerminalLaunchResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }

    public class TerminalLauncher
    {
        private readonly PathService _pathService;
        private readonly SshCommandBuilder _commandBuilder;

        public TerminalLauncher(PathService pathService, SshCommandBuilder commandBuilder)
        {
            _pathService = pathService;
            _commandBuilder = commandBuilder;
        }

        public TerminalLaunchResult Launch(ConnectionEntry entry, TerminalPreference preference)
        {
            try
            {
                CleanupOldScripts();
                var command = _commandBuilder.Build(entry);
                var scriptPath = CreateLaunchScript(entry, command);

                var preferredTerminal = ResolveTerminal(preference);
                if (preferredTerminal == TerminalPreference.WindowsTerminal)
                {
                    if (TryLaunchWindowsTerminal(entry, scriptPath, out var terminalError))
                    {
                        return new TerminalLaunchResult { Success = true, Message = "Opened in Windows Terminal." };
                    }

                    if (TryLaunchPowerShell(scriptPath, out var powerShellError))
                    {
                        return new TerminalLaunchResult { Success = true, Message = "Windows Terminal was unavailable. Opened in PowerShell instead." };
                    }

                    return new TerminalLaunchResult { Success = false, Message = terminalError + Environment.NewLine + powerShellError };
                }

                if (TryLaunchPowerShell(scriptPath, out var error))
                {
                    return new TerminalLaunchResult { Success = true, Message = "Opened in PowerShell." };
                }

                return new TerminalLaunchResult { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                return new TerminalLaunchResult { Success = false, Message = ex.Message };
            }
        }

        public bool IsWindowsTerminalAvailable()
        {
            var windowsAppsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "wt.exe");
            if (File.Exists(windowsAppsPath))
            {
                return true;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "wt.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return false;
                    }

                    process.WaitForExit(2000);
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private TerminalPreference ResolveTerminal(TerminalPreference preference)
        {
            if (preference == TerminalPreference.WindowsTerminal)
            {
                return TerminalPreference.WindowsTerminal;
            }

            if (preference == TerminalPreference.PowerShell)
            {
                return TerminalPreference.PowerShell;
            }

            return IsWindowsTerminalAvailable() ? TerminalPreference.WindowsTerminal : TerminalPreference.PowerShell;
        }

        private bool TryLaunchWindowsTerminal(ConnectionEntry entry, string scriptPath, out string error)
        {
            try
            {
                var title = string.IsNullOrWhiteSpace(entry?.Name) ? "TMXWinTerminal" : "TMXWinTerminal - " + entry.Name.Trim();
                var powerShellPath = GetPowerShellExecutablePath();
                var args = "new-window --title " + QuoteArgument(title) + " " + QuoteArgument(powerShellPath) + " -NoExit -ExecutionPolicy Bypass -File " + QuoteArgument(scriptPath);
                var startInfo = new ProcessStartInfo
                {
                    FileName = GetWindowsTerminalExecutablePath(),
                    Arguments = args,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "Windows Terminal launch failed: " + ex.Message;
                return false;
            }
        }

        private bool TryLaunchPowerShell(string scriptPath, out string error)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = GetPowerShellExecutablePath(),
                    Arguments = "-NoExit -ExecutionPolicy Bypass -File " + QuoteArgument(scriptPath),
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = "PowerShell launch failed: " + ex.Message;
                return false;
            }
        }

        private string CreateLaunchScript(ConnectionEntry entry, string command)
        {
            var scriptPath = Path.Combine(_pathService.ScriptCacheDirectory, "launch-" + Guid.NewGuid().ToString("N") + ".ps1");
            var escapedCommand = EscapePowerShellSingleQuotedString(command);
            var title = EscapePowerShellSingleQuotedString(string.IsNullOrWhiteSpace(entry?.Name) ? "TMXWinTerminal" : "TMXWinTerminal - " + entry.Name.Trim());
            var content = "$host.UI.RawUI.WindowTitle = '" + title + "'" + Environment.NewLine +
                          "$ErrorActionPreference = 'Stop'" + Environment.NewLine +
                          "$command = '" + escapedCommand + "'" + Environment.NewLine +
                          "try {" + Environment.NewLine +
                          "    Invoke-Expression $command" + Environment.NewLine +
                          "}" + Environment.NewLine +
                          "catch {" + Environment.NewLine +
                          "    Write-Host ''" + Environment.NewLine +
                          "    Write-Host 'TMXWinTerminal could not start the SSH command.' -ForegroundColor Red" + Environment.NewLine +
                          "    Write-Host $_.Exception.Message -ForegroundColor Red" + Environment.NewLine +
                          "}" + Environment.NewLine +
                          "Write-Host ''" + Environment.NewLine +
                          "Write-Host 'SSH session ended. PowerShell remains open for review.' -ForegroundColor DarkGray" + Environment.NewLine;

            File.WriteAllText(scriptPath, content);
            return scriptPath;
        }

        private void CleanupOldScripts()
        {
            foreach (var file in Directory.GetFiles(_pathService.ScriptCacheDirectory, "launch-*.ps1"))
            {
                try
                {
                    var lastWrite = File.GetLastWriteTimeUtc(file);
                    if (DateTime.UtcNow - lastWrite > TimeSpan.FromDays(7))
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                }
            }
        }

        private static string EscapePowerShellSingleQuotedString(string value)
        {
            return (value ?? string.Empty).Replace("'", "''");
        }

        private static string GetPowerShellExecutablePath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"WindowsPowerShell\v1.0\powershell.exe");
            return File.Exists(path) ? path : "powershell.exe";
        }

        private static string GetWindowsTerminalExecutablePath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\WindowsApps\wt.exe");
            return File.Exists(path) ? path : "wt.exe";
        }

        private static string QuoteArgument(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }
    }
}
