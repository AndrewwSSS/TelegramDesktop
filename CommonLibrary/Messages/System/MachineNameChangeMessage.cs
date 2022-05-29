using MessageLibrary;
using System;

namespace CommonLibrary.Messages.System
{
    [Serializable]
    public class MachineNameChangeMessage : Message
    {
        public string OldName { get; set; }
        public string NewName { get; set; }

        public MachineNameChangeMessage(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }
    }
}
