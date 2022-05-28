using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class FastLoginMessage : Message
    {
        public int UserId { get; set; }
        public string MachineName { get; set; }
        public string Guid { get; set; }

        public FastLoginMessage(string descName, string guid, int userId)
        {
            MachineName = descName;
            Guid = guid;
            UserId = userId;
        }
    }
}
