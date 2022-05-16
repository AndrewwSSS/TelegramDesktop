﻿using System;
using MessageLibrary;

namespace CommonLibrary
{
    [Serializable]
    public class LoginResultMessage : Message
    {
        public AuthenticationResult Result { get; set; }
        public string Message { get; set; }

        public string Name { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }

        public LoginResultMessage(AuthenticationResult Result, string Message=null)
        {
            this.Result = Result;
            this.Message = Message;
        }
        
    }
}
