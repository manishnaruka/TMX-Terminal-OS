using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace TMXWinTerminal.Models
{
    [DataContract]
    public class ConnectionEntry : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _host;
        private string _port;
        private string _username;
        private string _pemFilePath;
        private string _sshCommand;
        private string _notes;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        [DataMember(Order = 1)]
        public string Id
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        [DataMember(Order = 2)]
        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        [DataMember(Order = 3)]
        public string Host
        {
            get => _host;
            set => SetField(ref _host, value);
        }

        [DataMember(Order = 4)]
        public string Port
        {
            get => _port;
            set => SetField(ref _port, value);
        }

        [DataMember(Order = 5)]
        public string Username
        {
            get => _username;
            set => SetField(ref _username, value);
        }

        [DataMember(Order = 6)]
        public string PemFilePath
        {
            get => _pemFilePath;
            set => SetField(ref _pemFilePath, value);
        }

        [DataMember(Order = 7)]
        public string SshCommand
        {
            get => _sshCommand;
            set => SetField(ref _sshCommand, value);
        }

        [DataMember(Order = 8)]
        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        [DataMember(Order = 9)]
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetField(ref _createdAt, value);
        }

        [DataMember(Order = 10)]
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetField(ref _updatedAt, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static ConnectionEntry CreateEmpty()
        {
            var now = DateTime.UtcNow;
            return new ConnectionEntry
            {
                Id = string.Empty,
                Name = string.Empty,
                Host = string.Empty,
                Port = string.Empty,
                Username = string.Empty,
                PemFilePath = string.Empty,
                SshCommand = string.Empty,
                Notes = string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public ConnectionEntry Clone()
        {
            return new ConnectionEntry
            {
                Id = Id,
                Name = Name,
                Host = Host,
                Port = Port,
                Username = Username,
                PemFilePath = PemFilePath,
                SshCommand = SshCommand,
                Notes = Notes,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt
            };
        }

        public void CopyFrom(ConnectionEntry source)
        {
            if (source == null)
            {
                return;
            }

            Id = source.Id;
            Name = source.Name;
            Host = source.Host;
            Port = source.Port;
            Username = source.Username;
            PemFilePath = source.PemFilePath;
            SshCommand = source.SshCommand;
            Notes = source.Notes;
            CreatedAt = source.CreatedAt;
            UpdatedAt = source.UpdatedAt;
        }

        public string GetSummary()
        {
            if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Host))
            {
                return Username.Trim() + "@" + Host.Trim();
            }

            return Host ?? string.Empty;
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
