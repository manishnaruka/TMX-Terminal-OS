using System.Runtime.Serialization;

namespace TMXWinTerminal.Models
{
    [DataContract]
    public class AppSettings
    {
        [DataMember(Order = 1)]
        public TerminalPreference TerminalPreference { get; set; } = TerminalPreference.Auto;

        [DataMember(Order = 2)]
        public double WindowWidth { get; set; } = 1200;

        [DataMember(Order = 3)]
        public double WindowHeight { get; set; } = 760;

        [DataMember(Order = 4)]
        public bool IsWindowMaximized { get; set; }

        [DataMember(Order = 5)]
        public string LastSelectedEntryId { get; set; } = string.Empty;
    }
}
