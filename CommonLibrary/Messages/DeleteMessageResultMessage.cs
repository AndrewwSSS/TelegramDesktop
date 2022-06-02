using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Messages.Auth;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class DeleteMessageResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }

        public DeleteMessageResultMessage(AuthenticationResult result) => Result = result;

        public DeleteMessageResultMessage() { }
    }
}
