using System.Text;
using TMXWinTerminal.Models;

namespace TMXWinTerminal.Services
{
    public class SshCommandBuilder
    {
        public string Build(ConnectionEntry entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(entry.SshCommand))
            {
                return entry.SshCommand.Trim();
            }

            if (string.IsNullOrWhiteSpace(entry.Username) || string.IsNullOrWhiteSpace(entry.Host))
            {
                return string.Empty;
            }

            var builder = new StringBuilder("ssh");
            if (!string.IsNullOrWhiteSpace(entry.PemFilePath))
            {
                builder.Append(" -i ");
                builder.Append('"').Append(entry.PemFilePath.Trim()).Append('"');
            }

            builder.Append(' ');
            builder.Append(entry.Username.Trim()).Append('@').Append(entry.Host.Trim());

            if (!string.IsNullOrWhiteSpace(entry.Port))
            {
                builder.Append(" -p ").Append(entry.Port.Trim());
            }

            return builder.ToString();
        }
    }
}
