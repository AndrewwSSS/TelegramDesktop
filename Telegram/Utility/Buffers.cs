using CommonLibrary.Containers;
using CommonLibrary.Messages.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Utility
{
    public static class Buffers
    {
        //id - pair of name and desc
        public static Dictionary<int, KeyValuePair<string, string>> EditGroupSettings { get; set; } = new Dictionary<int, KeyValuePair<string, string>>();
        public static Dictionary<int, KeyValuePair<string, string>> NewGroupSettings { get; set; } = new Dictionary<int, KeyValuePair<string, string>>();

    }

}
