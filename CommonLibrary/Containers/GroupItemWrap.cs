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

namespace CommonLibrary.Containers
{
    public class GroupItemWrap : INotifyPropertyChanged
    {
        private PublicGroupInfo group;

        public ObservableCollection<UserItemWrap> Members { get; set; } = new ObservableCollection< UserItemWrap >();
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
        public List<ImageContainer> Images { get; set; } = new List<ImageContainer>();
        public ChatMessage LastMessage => GroupChat.Messages.LastOrDefault();
        public GroupItemWrap(PublicGroupInfo group)
        {
            GroupChat = group;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
