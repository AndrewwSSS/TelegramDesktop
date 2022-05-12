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
        public bool DisplayTime = false;
        public TextMessage(string text)
        {
            Type = MessageType.Text;
            Time = DateTime.Now;
            Text = text;
        }
        public override string ToString() => $"{(DisplayTime ? "[" + Time.ToLongTimeString() + " " + Time.ToShortDateString() + "] " : "")}{Text}";
        
    }
}
