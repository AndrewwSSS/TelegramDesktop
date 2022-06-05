using CommonLibrary.Containers;
using CommonLibrary.Messages;
using CommonLibrary.Messages.Groups;
using MessageLibrary;
using MessageLibrary.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace TESTS
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<ChatMessage> list = new List<ChatMessage>() {
                new ChatMessage("adat")
            };
            ArrayMessage<ChatMessage> msg = new ArrayMessage<ChatMessage>(list);
            byte[] arr = msg.ToByteArray();
            Message result = Message.FromByteArray(arr.ToList().GetRange(4,arr.Length-4).ToArray());
        }

        [TestMethod]
        public void TestFileClient()
        {
            TcpFileClientWrap client = new TcpFileClientWrap(IPAddress.Parse("127.0.0.1"), 5001);
            TcpFileServerWrap server = new TcpFileServerWrap();
            server.Start(5001, 1);
            server.FileChunkReceived += Server_FileChunkReceived;
            client.Connect();
            FileContainer file = FileContainer.FromFile("testimage.png");
            client.SendAsync(new FileMessage(file.FileData, -1));
        }
        bool open = true;
        
        private StreamWriter writer = new StreamWriter("receivedimage.png");
        private void Server_FileChunkReceived(TcpFileClientWrap client, int fileId, byte[] chunk, bool isLast)
        {
            writer.Write(chunk);
            if (isLast)
            {
                writer.Close();
                open = false;
            }
        }
    }
}
