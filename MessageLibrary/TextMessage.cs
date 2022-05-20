using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MessageLibrary
{
    /// <summary>
    /// Message containing text
    /// Type: text
    /// </summary>
    [Serializable]
    public class TextMessage : Message
    {
        public string Text { get; set; }
        public TextMessage(string text) => Text = text;
        public override string ToString() => Text;
    }
}
