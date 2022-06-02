using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatLookupMessage : Message
    {
        public string Name { get; set; }

        public ChatLookupMessage(string name){
            Name = name;
        }


    }
}
