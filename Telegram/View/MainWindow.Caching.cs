using CacheLibrary;
using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
    public partial class MainWindow
    {
        public ObservableCollection<GroupItemWrap> Groups { get; set; } = new ObservableCollection<GroupItemWrap>();
        public ObservableCollection<GroupItemWrap> FoundGroups { get; set; } = new ObservableCollection<GroupItemWrap>();

        public Dictionary<int, string> CachedImages { get; set; } = new Dictionary<int, string>();
        
        private void LoadUsers() => CachedUsers.AddRange(CacheManager.Instance.LoadAllUsers());
        private void LoadGroups() {
            List<PublicGroupInfo> list = CacheManager.Instance.LoadAllGroups();
            foreach(var info in list)
            {
                GroupItemWrap item = new GroupItemWrap(info);
                foreach(var id in info.MembersId)
                {
                    UserItemWrap user = CachedUsers.FirstOrDefault(u => u.User.Id == id);
                    if (user != null)
                        item.Members.Add(user);
                }
                Groups.Add(item);
            }
        }
        private void LoadCache()
        {
            LoadUsers();
            LoadGroups();
            CacheManager.Instance.LoadAllGroups();
        }
        private void SaveCache()
        {
            foreach (var user in CachedUsers)
                CacheManager.Instance.SaveUser(user);
            foreach (var group in Groups)
                CacheManager.Instance.SaveGroup(group.GroupChat);
        }
    }
}
