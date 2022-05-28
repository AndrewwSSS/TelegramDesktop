using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class FastLoginMessage : Message
    {
        public int UserId { get; set; }
        public string DesktopName { get; set; }
        public string Guid { get; set; }

        public FastLoginMessage(string descName, string guid, int userId)
        {
            DesktopName = descName;
            Guid = guid;
            UserId = userId;
        }
    }
}
