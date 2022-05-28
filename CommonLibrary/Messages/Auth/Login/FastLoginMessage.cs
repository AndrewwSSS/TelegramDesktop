using MessageLibrary;
using System;

namespace CommonLibrary.Messages.Auth.Login
{
    [Serializable]
    public class FastLoginMessage : Message
    {
        public string DesktopName { get; set; }
        public string Guid { get; set; }

        public FastLoginMessage(string descName, string guid)
        {
            DesktopName = descName;
            Guid = guid;
        }
    }
}
