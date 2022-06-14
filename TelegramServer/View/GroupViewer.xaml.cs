using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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



        private ObservableCollection<UserItemWrap> Users;

        public event PropertyChangedEventHandler PropertyChanged;
        public GroupViewer(TelegramDb telegramDb, int groupId)
        {
            InitializeComponent();
            TelegramDb = telegramDb;
            GroupId = groupId;
            Users = new ObservableCollection<UserItemWrap>();

            DataContext = this;
            LB_Members.ItemsSource = Users;

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            GroupChat group = TelegramDb.GroupChats.FirstOrDefault(g => g.Id == GroupId);

            Users.Clear();

            if (group != null)
            {
                Group = group;

                foreach (var member in group.Members) { 
                    UserItemWrap userItemWrap = new UserItemWrap(new PublicUserInfo()
                    {
                        Name = member.Name
                    });

                    foreach (var imageId in member.ImagesId)  {

                        ImageContainer Image = TelegramDb.Images.FirstOrDefault(i => i.Id == imageId);

                        if (Image != null)
                            userItemWrap.Images.Add(Image);
  
                    }

                    Users.Add(userItemWrap);
                }

               
                LB_Messages.ItemsSource = group.Messages;




                if (group.ImagesId.Count > 0)
                {
                    ImageContainer img
                        = TelegramDb.Images.FirstOrDefault(i => i.Id == group.ImagesId[0]);
                    MemoryStream ms1 = new MemoryStream(img.ImageData.Bytes);

                    Bitmap bitmap = new Bitmap(Image.FromStream(ms1));

                    Bitmap bitmap1 = new Bitmap(bitmap, 70, 70);

                    MemoryStream ms2 = new MemoryStream();
                    bitmap1.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);

                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = ms2;
                    imageSource.EndInit();

                    GroupAvatar = imageSource;
                }
                    




            }



        }

        private void BTN_Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateInterface();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
