using CommonLibrary.Messages;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class MessageItemWrap : INotifyPropertyChanged
    {
        // для WPFов
        public MessageItemWrap Self => this;

        private bool showAvatar = false;
        private bool showUsername = false;
        public ObservableCollection<FileMetadata> FilesMetadata { get; set; } = new ObservableCollection<FileMetadata>();
        public ObservableCollection<ImageMetadata> ImagesMetadata { get; set; } = new ObservableCollection<ImageMetadata>();
        public ChatMessage Message { get; set; }
        public UserItemWrap FromUser { get; set; }
        public MessageItemWrap RespondingTo { get; set; }
        public UserItemWrap RepostUser { get; set; }
        public string FormattedTime => Message.LocalTime.ToString("HH:mm");
        public bool ShowAvatar
        {
            get => showAvatar;
            set
            {
                showAvatar = value;
                OnPropertyChanged();
            }
        }
        public bool ShowUsername { 
            get => showUsername;
            set
            {
                showUsername = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MessageItemWrap(ChatMessage msg) => Message = msg;
    }
}
