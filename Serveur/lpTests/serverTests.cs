using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;
using Server;
namespace Server.Tests
{
    [TestClass()]
    public class serverTests
    {
        [TestMethod()]
        public void encodeTest()
        {
            byte[] message = new byte[sizeof(int)],test= new byte[sizeof(int)] { 5, 0, 0, 0 };
            
            int[ ] size = new int[1]{ 0}; 
            int val = 5;
            server.encode(message, val, size);
            for(int i=0;i<message.Length;i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
            val = -1;
            size[0] = 0;
            server.encode(message, val, size);
            test = new byte[sizeof(int)] { 255, 255, 255, 255 };
            for (int i = 0; i < message.Length; i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
            message = new byte[sizeof(int) + 10];
            val = -1;
            size[0] = 10;
            server.encode(message, val, size);
            test = new byte[sizeof(int)+10] {0,0,0,0,0,0,0,0,0,0,255,255,255,255};
            for (int i = 0; i < message.Length; i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
            message = new byte[sizeof(bool)];

            size[0] = 0;
            server.encode(message, false, size);
            test = new byte[sizeof(bool)] { 0 };
            for (int i = 0; i < message.Length; i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
            message = new byte[sizeof(bool)];

            size[0] = 0;
            server.encode(message, true, size);
            test = new byte[sizeof(bool)] { 1 };
            for (int i = 0; i < message.Length; i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
            message = new byte[sizeof(int)+7];

            size[0] = 0;
            server.encode(message, "bonjour", size);
            
            test = new byte[sizeof(int)+7] { 7,0,0,0,98,111,110,106,111,117,114 };
            for (int i = 0; i < message.Length; i++)
            {
                Assert.IsTrue(message[i] == test[i]);
            }
                message = null;
                size[0] = 0;
                Assert.ThrowsException<System.ArgumentNullException>(() => server.encode(message, 0, size));


                message = new byte[sizeof(int)];
                size[0] = 1;
                Assert.ThrowsException<System.ArgumentException>(() => server.encode(message, 0, size));
            
        }

        [TestMethod()]
        public void decodeIntTest()
        {
            byte[] message = new byte[sizeof(int)] { 5, 0, 0, 0 };
            int[] size = new int[1] { 0 };
            int result = server.decodeInt(message, size);
            if (result != 5)
            {
                Assert.Fail();
            }
            message = new byte[sizeof(int)] { 255,255, 255, 255 };
            size[0] =0;
            result = server.decodeInt(message, size);
            if (result != -1)
            {
                Assert.Fail();
            }
            size[0] = 1;
                
            Assert.ThrowsException<ArgumentException>(()=> result = server.decodeInt(message, size));
            message = null;
            Assert.ThrowsException<ArgumentNullException>(()=> server.decodeInt(message, size));
        }

        [TestMethod()]
        public void decodeBoolTest()
        {
            byte[] message = new byte[sizeof(bool)] { 1 };
            int[] size = new int[1] { 0 };
            bool result = server.decodeBool(message, size);
            Assert.AreEqual(true, result);
            message = new byte[sizeof(bool)] { 0 };
            size[0] = 0;
            result = server.decodeBool(message, size);
            Assert.AreEqual(false,result);
            size[0] = 1;

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => server.decodeBool(message, size));

            message = null;
            Assert.ThrowsException<ArgumentNullException>(() => server.decodeBool(message, size));
        }

        [TestMethod()]
        public void decodeStringTest()
        {
            byte[] message = new byte[sizeof(int) + 7] { 7, 0, 0, 0, 98, 111, 110, 106, 111, 117, 114 };

            int[] size = new int[1] {0};
            string result = server.decodeString(message,size);
            string test = "bonjour";    
            Assert.AreEqual(result.CompareTo(test),0);            
            size[0] = 5;
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => server.decodeString(message, size));
            message = null;
            Assert.ThrowsException<ArgumentNullException>(() => server.decodeString(message, size));


        }
        [TestMethod()]
        public void alreadyConnectedTest()
        {
            Dictionary<Socket, int> connected = new Dictionary<Socket, int>();
            int id = 0;
            Socket s = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            connected[s] = id;
            Assert.AreEqual(server.alreadyConnected(connected, 0), true);
            Assert.AreEqual(server.alreadyConnected(connected, 75), false);
            Assert.ThrowsException<NullReferenceException>(() => server.alreadyConnected(null, 0));
            Assert.AreEqual(server.alreadyConnected(connected, -1),false);
        }
    }
}

