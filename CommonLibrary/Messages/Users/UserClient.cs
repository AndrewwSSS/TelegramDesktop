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

        public string MachineName { get; set; }
        public string Guid { get; set; }

        public UserClient(string machineName, string guid)
        {
            MachineName = machineName;
            Guid = guid;
        }

        public UserClient() { }

    }
}
