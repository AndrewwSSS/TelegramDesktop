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
        private int associatedUserId = -1;
        private bool? isUserOnline;

        public ObservableCollection<UserItemWrap> Members { get; set; } = new ObservableCollection<UserItemWrap>();
        public ObservableCollection<UserItemWrap> Admins { get; set; } = new ObservableCollection<UserItemWrap>();
        public PublicGroupInfo GroupChat
        {
            get => group;
            set
            {
                group = value;

                if (GroupChat.GroupType == GroupType.Personal)
                    IsUserOnline = true;
                else IsUserOnline = null;

                OnPropertyChanged();
                OnPropertyChanged("LastMessage");
            }
        }

        /// <summary>
        /// Если группа обычная, то это поле становится null
        /// </summary>
        public bool? IsUserOnline
        {
            get => isUserOnline;
            set
            {
                isUserOnline = value;
                OnPropertyChanged();
            }
        }
        public int AssociatedUserId
        {
            get => associatedUserId;
            set
            {
                associatedUserId = value;
                OnPropertyChanged();
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

        public GroupItemWrap(PublicGroupInfo group)
        {
            GroupChat = group;
            Members.CollectionChanged += Members_CollectionChanged;
        }

        private void Members_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.Count > 0)
                foreach (var item in e.NewItems)
                    if (!GroupChat.MembersId.Contains((item as UserItemWrap).User.Id))
                        GroupChat.MembersId.Add((item as UserItemWrap).User.Id);
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
