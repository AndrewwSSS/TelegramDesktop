using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace TelegramServer.View
{
    /// <summary>
    /// Interaction logic for GroupViewer.xaml
    /// </summary>
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
            //GroupAvatar = ImageContainer.FromFile("cat.jpg").ImageData.ImageSource;

            UpdateInterface();
        }

        private void UpdateInterface()
        {
            GroupChat group = TelegramDb.GroupChats.FirstOrDefault(g => g.Id == GroupId);


            if (group != null)
            {
                Group = group;

                TB_GroupName.Text = group.Name;
                TB_Description.Text = group.Description;
                TB_MembersCount.Text = group.Members.Count.ToString();
                TB_GroupType.Text = group.Type.ToString();

                LB_Members.ItemsSource = group.Members;
                LB_Messages.ItemsSource = group.Messages;

                if(group.ImagesId.Count > 0)
                    GroupAvatar = TelegramDb.Images.FirstOrDefault(i => i.Id == group.ImagesId[0]).ImageData.ImageSource;

               

                
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
