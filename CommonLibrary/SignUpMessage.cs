using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageLibrary;


namespace CommonLibrary
{
    [Serializable]
    public class SignUpMessage : Message
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public SignUpMessage() => Type = MessageType.Custom;

    }
}
