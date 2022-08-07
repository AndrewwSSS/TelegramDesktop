using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.SignUp
{
    [Serializable]
    public class SignUpStage2ResultMessage : Message
    {
        public AuthResult result { get; set; }
        public int? RemainingAttempts { get; set; }
        public int? UserId { get; set; }

        public SignUpStage2ResultMessage(AuthResult result) => this.result = result;


        public SignUpStage2ResultMessage(AuthResult result, int? userId) : this(result)
        {
            UserId = userId;
        }
    }
}
