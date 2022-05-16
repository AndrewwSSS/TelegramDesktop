using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class PublicUserInfo : UserEntity
    {
        public int Id { get; set; }
        public DateTime LastVisitDate { get; set; }

        public PublicUserInfo(string Name, string Description, int Id, DateTime LastVisitDate)
        {
            this.Name = Name;
            this.Description = Description;
            this.Id = Id;
            this.LastVisitDate = LastVisitDate;
        }

    }
}
