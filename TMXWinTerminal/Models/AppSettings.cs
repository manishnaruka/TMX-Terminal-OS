using System.Runtime.Serialization;

namespace TMXWinTerminal.Models
{
    [DataContract]
    public class AppSettings
    {
        [DataMember(Order = 1)]
        public TerminalPreference TerminalPreference { get; set; } = TerminalPreference.WindowsTerminal;

        [DataMember(Order = 2)]
        public double WindowWidth { get; set; } = 920;

        [DataMember(Order = 3)]
        public double WindowHeight { get; set; } = 680;

        [DataMember(Order = 4)]
        public bool IsWindowMaximized { get; set; }

        [DataMember(Order = 5)]
        public string LastSelectedEntryId { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public ThemeMode ThemeMode { get; set; } = ThemeMode.Light;

        [DataMember(Order = 7)]
        public bool HasAppliedCompactLayoutDefaults { get; set; }
    }
}
