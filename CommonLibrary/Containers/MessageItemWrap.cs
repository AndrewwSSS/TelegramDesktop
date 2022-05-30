using CommonLibrary.Messages;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class MessageItemWrap : INotifyPropertyChanged
    {
        public MessageItemWrap Self => this;

        private bool showAvatar = false;
        public ChatMessage Message { get; set; }
        public UserItemWrap FromUser { get; set; }
        public MessageItemWrap RespondingTo { get; set; }
        public UserItemWrap RepostUser { get; set; }
        public string FormattedTime => Message.Time.ToString("HH:mm");
        public bool ShowAvatar
        {
            get => showAvatar;
            set
            {
                showAvatar = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MessageItemWrap(ChatMessage msg) => Message = msg;
    }
}
