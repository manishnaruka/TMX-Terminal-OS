using System;
using System.Collections.Generic;
using System.IO;
using TMXWinTerminal.Models;

namespace TMXWinTerminal.Services
{
    public class ValidationOutcome
    {
        public List<string> Errors { get; } = new List<string>();

        public List<string> Warnings { get; } = new List<string>();

        public bool HasErrors => Errors.Count > 0;
    }

    public class ValidationService
    {
        private readonly SshCommandBuilder _commandBuilder;

        public ValidationService(SshCommandBuilder commandBuilder)
        {
            _commandBuilder = commandBuilder;
        }

        public ValidationOutcome ValidateForSave(ConnectionEntry entry)
        {
            return Validate(entry, false);
        }

        public ValidationOutcome ValidateForConnect(ConnectionEntry entry)
        {
            return Validate(entry, true);
        }

        private ValidationOutcome Validate(ConnectionEntry entry, bool forConnect)
        {
            var outcome = new ValidationOutcome();
            if (entry == null)
            {
                outcome.Errors.Add("No entry is selected.");
                return outcome;
            }

            if (string.IsNullOrWhiteSpace(entry.Name))
            {
                outcome.Errors.Add("Name is required.");
            }

            var hasCommand = !string.IsNullOrWhiteSpace(entry.SshCommand);
            if (!hasCommand)
            {
                if (string.IsNullOrWhiteSpace(entry.Host) || string.IsNullOrWhiteSpace(entry.Username))
                {
                    outcome.Errors.Add("Provide a full SSH command or fill both Host and Username so the app can generate one.");
                }
            }

            if (!string.IsNullOrWhiteSpace(entry.Port))
            {
                if (!int.TryParse(entry.Port.Trim(), out var port) || port < 1 || port > 65535)
                {
                    outcome.Errors.Add("Port must be a number between 1 and 65535.");
                }
            }

            if (!string.IsNullOrWhiteSpace(entry.PemFilePath) && !File.Exists(entry.PemFilePath.Trim()))
            {
                var message = "The selected PEM/private key path does not exist on this machine.";
                if (forConnect && !hasCommand)
                {
                    outcome.Errors.Add(message);
                }
                else
                {
                    outcome.Warnings.Add(message);
                }
            }

            if (forConnect)
            {
                var command = _commandBuilder.Build(entry);
                if (string.IsNullOrWhiteSpace(command))
                {
                    outcome.Errors.Add("The SSH command is empty after validation.");
                }
            }

            return outcome;
        }
    }
}
