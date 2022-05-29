using CommonLibrary.Messages.Groups;
using MessageLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TESTS
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<PublicGroupInfo> list = new List<PublicGroupInfo>() {
                new PublicGroupInfo("adat","",-1)
            };
            ArrayMessage<PublicGroupInfo> msg = new ArrayMessage<PublicGroupInfo>(list);
            Message result = Message.FromByteArray(msg.ToByteArray());
        }

        [TestMethod]
        public void TestUserCaching()
        {

        }
    }
}
