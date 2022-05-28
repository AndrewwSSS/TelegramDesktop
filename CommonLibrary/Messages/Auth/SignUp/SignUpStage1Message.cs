using System;


namespace CommonLibrary.Messages.Auth.SignUp
{
    [Serializable]
    public class SignUpStage1Message : BaseMessage
    {

        public string Login { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public SignUpStage1Message() { }


    }
}
