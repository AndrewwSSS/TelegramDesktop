using System;
using MessageLibrary;

namespace CommonLibrary.Messages.Groups
{
    [Serializable]
    public class ChatLookupMessage : Message
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string UserGuid { get; set; }

        public ChatLookupMessage(string name, int userId, string Guid){
            Name = name;
            UserId = userId; 
            UserGuid = Guid;
        }


    }
}
