using CommonLibrary.Containers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserContainer
    {

        public PublicUserInfo User { get; set; }
        public List<ImageContainer> Images { get; set; }
      
        public UserContainer() { 
            
        }
        public UserContainer(UserItemWrap user)
        {
            User = user.User;
            Images = user.Images.ToList();
        }
        public UserItemWrap ToWrap()
        {
            UserItemWrap result = new UserItemWrap(User)
            {
                Images = new ObservableCollection<ImageContainer>(Images.ToList())
            };
            return result;
        }
    }
}
