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

        public string Login { get; set; }
        public DateTime RegistrationDate { get; set; }

        public PublicUserInfo(int id, string login, string name, string desc, DateTime regDate)
        {
            Name = name;
            Login = login;
            Description = desc;
            Id = id;
            RegistrationDate = regDate;
        }

        public PublicUserInfo() { }

    }
}
