using CommonLibrary.Messages;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonLibrary.Containers
{
    public class GroupItemWrap : INotifyPropertyChanged
    {
        private PublicGroupInfo group;
        private bool joined = true;

        public ObservableCollection<UserItemWrap> Members { get; set; } = new ObservableCollection<UserItemWrap>();
        public ObservableCollection<UserItemWrap> Admins { get; set; } = new ObservableCollection<UserItemWrap>();
        public PublicGroupInfo GroupChat
        {
            get => group;
            set
            {
                group = value;
                OnPropertyChanged();
                OnPropertyChanged("LastMessage");
            }
        }

        public bool Joined
        {
            get => joined;
            set
            {
                joined = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ImageContainer> Images { get; set; } = new ObservableCollection<ImageContainer>();
        public List<ChatMessage> Messages => GroupChat.Messages;
        public ChatMessage LastMessage => GroupChat.Messages.LastOrDefault();
        public ImageSource Avatar => Images.Count > 0 ? Images[0].ImageData.ImageSource : null;

        public string Name => GroupChat.Name;
        public string Description => GroupChat.Description;

        public GroupItemWrap(PublicGroupInfo group) => GroupChat = group;

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
