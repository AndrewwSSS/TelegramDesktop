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
        ObservableCollection<GroupItemWrap> Groups { get; set; }
        ObservableCollection<GroupItemWrap> FoundGroups { get; set; }
        private void RequestData(IEnumerable<int> id, RequestType type) => Client?.SendAsync(new DataRequestMessage(id, type));
        private void AcceptUserRequest(DataRequestResultMessage<UserItemWrap> msg) {
            foreach (var user in msg.Result)
            {
                CacheUser(user);
            }
        }
        
        private void CacheUser(UserItemWrap user)
        {
            if (!Users.Contains(user))
                Users.Add(user);
            
            CacheManager.Instance.SaveUser(user);
        }
        
        private void CacheGroup(GroupItemWrap group)
        {
            if (!Groups.Contains(group))
                Groups.Add(group);
            
            CacheManager.Instance.SaveGroup(group);
        }

        
        

        private void LoadUsers() => Users.AddRange(CacheManager.Instance.LoadAllUsers());
        private void LoadGroups() => CacheManager.Instance.LoadAllGroups().ForEach(g=>Groups.Add(g));
        private void LoadCache()
        {
            LoadUsers();
            CacheManager.Instance.LoadAllGroups();
        }
        private void SaveCache()
        {
            foreach (var user in Users)
                CacheManager.Instance.SaveUser(user);
            foreach (var group in Groups)
                CacheManager.Instance.SaveGroup(group);
        }
    }
}
