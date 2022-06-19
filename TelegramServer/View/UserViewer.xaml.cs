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
    public partial class UserViewer : Window, INotifyPropertyChanged
    {
        private User user;
        private TelegramDb TelegramDb;
        private ImageSource groupAvatar;

        public int UserId { get; private set; }
        public User User
        {
            get => user;
            set
            {
                user = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<GroupItemWrap> Groups { get; set; }
        public ImageSource UserAvatar
        {
            get => groupAvatar;
            set
            {
                groupAvatar = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public UserViewer(TelegramDb dataBase, int userId)
        {
            InitializeComponent();
            TelegramDb = dataBase;
            UserId = userId;
            Groups = new ObservableCollection<GroupItemWrap>();
            DataContext = this;
            Updade();
        }


        public void Updade()
        {
            User user = TelegramDb.Users.FirstOrDefault(u => u.Id == UserId);

            if(user != null) {
                User = user;

                Groups.Clear();

                if(User.Chats.Count > 0) {
                    foreach (var chat in User.Chats) {
                        GroupItemWrap itemWrap = new GroupItemWrap(new PublicGroupInfo() { Name = chat.NameByGroupType });
                        if(chat.ImagesId.Count > 0)
                        {
                            ImageContainer avatar = TelegramDb.Images.FirstOrDefault(u => u.Id == chat.ImagesId[0]);
                            itemWrap.Images.Add(avatar);
                        }
                            
                        Groups.Add(itemWrap);
                    }
                }

                if (User.ImagesId.Count > 0)
                {
                    ImageContainer imageSource
                        = TelegramDb.Images.FirstOrDefault(u => u.Id == User.ImagesId[0]);

                    UserAvatar
                        = ImageConverter.Resize(imageSource.ImageData.Bytes, 70, 70);
                }
                else
                    UserAvatar = null;

            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void BTN_Update_Click(object sender, RoutedEventArgs e)
        {
            Updade();
        }
    }
}
