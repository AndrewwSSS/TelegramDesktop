using CacheLibrary;
using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
    public partial class MainWindow
    {
        private void RequestData(IEnumerable<int> id, RequestType type) => Client?.SendAsync(new DataRequestMessage(id, type));
        private void AcceptUserRequest(DataRequestResultMessage<UserItemWrap> msg) {
            foreach (var user in msg.Result)
            {
                CacheUser(user);
            }
        }
        private void AcceptGroupRequest(DataRequestResultMessage<PublicGroupInfo> msg)
        {
            foreach (var group in msg.Result)
                CacheGroup(group);
        }
        
        private void CacheUser(UserItemWrap user)
        {
            if (!Users.Contains(user))
                Users.Add(user);
            
            CacheManager.Instance.SaveUser(user);
        }
        
        private void CacheGroup(PublicGroupInfo group)
        {
            if (!Groups.Contains(group))
            
                Groups.Add(group);
            
            CacheManager.Instance.SaveGroup(group);

            RequestData(group.MembersId.Where(id => Users.FirstOrDefault(u => u.Id == id) == null), RequestType.User);            
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
