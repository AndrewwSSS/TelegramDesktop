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

         public SignUpResultMessage(AuthenticationResult Result, string Message = null)
         {
             this.Result = Result;
             this.Message = Message;
         }

         public SignUpResultMessage SetRegistrationDate(DateTime RegistrationDate) {
              this.RegistrationDate = RegistrationDate;
              return this;
         }
    }
}
