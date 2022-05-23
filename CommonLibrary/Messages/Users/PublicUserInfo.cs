using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class PublicUserInfo : UserEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Login { get; set; }

        public PublicUserInfo(int id, string login, string name, string desc)
        {
            Name = name;
            Login = login;
            Description = desc;
            Id = id;
        }

        public PublicUserInfo() { }


    }
}
