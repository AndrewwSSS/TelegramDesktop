using System;
using MessageLibrary;

namespace CommonLibrary
{
    [Serializable]
    public class SignUpResultMessage : Message
    { 
         public AuthenticationResult Result { get; set; }
         public string Message { get; set; }
         public DateTime RegistrationDate { get; set; }

         public SignUpResultMessage(AuthenticationResult Result, DateTime RegistrationDate, string Message = null)
         {
             this.Result = Result;
             this.Message = Message;
             this.RegistrationDate = RegistrationDate;
         }
    }
}
