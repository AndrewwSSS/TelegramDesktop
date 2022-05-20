using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class PublicUserInfo : UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
