using CommonLibrary.Messages;
using CommonLibrary.Messages.Groups;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Containers
{
    public class GroupItemWrap : INotifyPropertyChanged
    {
        private GroupChat group;

        public GroupChat Group
        {
            get => group;
            set
            {
                group = value;
                OnPropertyChanged();
                OnPropertyChanged("LastMessage");
            }
        }
        public ChatMessage LastMessage => Group.Messages.LastOrDefault();
        public GroupItemWrap(GroupChat group)
        {
            Group = group;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
