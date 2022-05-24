using CacheLibrary;
using CommonLibrary.Messages.Groups;
using CommonLibrary.Messages.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
    public partial class MainWindow
    {
        private void CacheUser(PublicUserInfo user)
        {
            if (!Users.Contains(user))
            {
                Users.Add(user);
            }
            CacheManager.Instance.SaveUser(user);
        }
        
        private void CacheGroup(PublicGroupInfo group)
        {
            if (!Groups.Contains(group))
            {
                Groups.Add(group);
            }
            CacheManager.Instance.SaveGroup(group);
            foreach (var user in group.Members)
                CacheUser(user);
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
