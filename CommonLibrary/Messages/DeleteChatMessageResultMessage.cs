using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLibrary.Messages.Auth;

namespace CommonLibrary.Messages
{
    [Serializable]
    public class DeleteChatMessageResultMessage : BaseMessage
    {
        public AuthenticationResult Result { get; set; }

        public DeleteChatMessageResultMessage(AuthenticationResult result) => Result = result;

        public DeleteChatMessageResultMessage() { }
    }
}
