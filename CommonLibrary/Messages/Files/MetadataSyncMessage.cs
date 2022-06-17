using MessageLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.Messages.Files
{
    [Serializable]
    public class MetadataSyncMessage : Message
    {
        public int LocalReturnId { get; set; }
     
        public MetadataSyncMessage(int lrId) => LocalReturnId = lrId;

        public MetadataSyncMessage()
        {

        }
    }
}
