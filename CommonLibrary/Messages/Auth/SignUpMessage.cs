using System;


namespace CommonLibrary.Messages.Auth
{
    [Serializable]
    public class SignUpMessage : BaseMessage
    {

        public string Login { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public SignUpMessage() { }


    }
}
