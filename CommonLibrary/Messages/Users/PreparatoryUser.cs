using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Messages.Users
{
    [Serializable]
    public class PreparatoryUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public DateTime StartTime { get; set; }

        public string ExpectedCode { get; set; }

        public PreparatoryUser() { }


    }
}
