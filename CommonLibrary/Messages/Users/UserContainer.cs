using CommonLibrary.Containers;
using System;
using System.Collections.Generic;
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
      
        public UserContainer() { }
    }
}
