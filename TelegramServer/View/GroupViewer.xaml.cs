using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using ImageConverter = CommonLibrary.ImageConverter;

namespace TelegramServer.View
{
    public partial class GroupViewer : Window, INotifyPropertyChanged
    {
        private ImageSource groupAvatar;
        private GroupChat group;


        public TelegramDb TelegramDb { get; set; }
        public int GroupId { get; set; }
        public GroupChat Group
        {
            get => group;
            set
            {
                group = value;
                OnPropertyChanged();
            }
        }
        public ImageSource GroupAvatar
        {
            get => groupAvatar;
            set
            {
                groupAvatar = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<UserItemWrap> Users;
        public event PropertyChangedEventHandler PropertyChanged;


        public GroupViewer(TelegramDb telegramDb, int groupId)
        {
            InitializeComponent();
            TelegramDb = telegramDb;
            GroupId = groupId;
            Users = new ObservableCollection<UserItemWrap>();

            DataContext = this;
            LB_Members.ItemsSource = Users;

            Update();
        }


        private void Update()
        {
            GroupChat group
                = TelegramDb.GroupChats.FirstOrDefault(g => g.Id == GroupId);

            Users.Clear();

            if (group != null)
            {
                Group = group;
                LB_Messages.ItemsSource = group.Messages;

                foreach(var member in group.Members) { 

                    UserItemWrap userItemWrap
                        = new UserItemWrap(new PublicUserInfo()
                            {
                                Name = member.Name
                            });

                    foreach(var imageId in member.ImagesId) {

                        ImageContainer Image = TelegramDb.Images.FirstOrDefault(i => i.Id == imageId);

                        if (Image != null)
                            userItemWrap.Images.Add(Image);
                    }

                    Users.Add(userItemWrap);
                }
              
                if (group.ImagesId.Count > 0)
                { 
                    ImageContainer imgSource
                        = TelegramDb.Images.FirstOrDefault(i => i.Id == group.ImagesId[0]);

                    if (imgSource != null)
                        GroupAvatar = ImageConverter.Resize(imgSource.ImageData.Bytes, 70, 70);
                    else
                        GroupAvatar = null;
                }
            }
        }

        private void BTN_Update_Click(object sender, RoutedEventArgs e)
        {
            Update();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
