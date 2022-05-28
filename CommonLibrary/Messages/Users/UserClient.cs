using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class UserClient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Guid { get; set; }

        public UserClient(string name, string guid)
        {
            Name = name;
            Guid = guid;
        }

        public UserClient() { }

    }
}
