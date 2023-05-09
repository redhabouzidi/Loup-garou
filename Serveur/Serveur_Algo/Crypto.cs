using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;
namespace Server
{
    public class Crypto
    {
        private RSA rsa;
        private X509Certificate2 cert;
        public Crypto()
        {
            this.rsa = RSA.Create();
            string privateKeyBytes = File.ReadAllText("/home/ubuntu/certificat/private.key");
            rsa.ImportFromPem(privateKeyBytes);
            this.cert = new X509Certificate2("/home/ubuntu/certificat/certificate.crt");
        }

        public int SendCertificateToClient(Socket client)
        {
            byte[] certBytes = cert.Export(X509ContentType.Cert);
            byte[] certLength = BitConverter.GetBytes(certBytes.Length);
            byte[] message = certLength.Concat(certBytes).ToArray();
            Console.WriteLine("cryptolen="+ message.Length+"msg = ");
            Console.WriteLine(BitConverter.ToString(message));

            return (client.Send(message, message.Length, SocketFlags.None));
        }

        public static byte[] EncryptMessage(byte[] message, Aes aes)
        {

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.ISO10126;
            var encryptor = aes.CreateEncryptor();
            byte[] data = encryptor.TransformFinalBlock(message, 0, message.Length);
            byte[] toSend = new byte[data.Length + sizeof(int)];
            int[] size = new int[1] { 0 };
            Messages.encode(toSend, data.Length, size);
            Array.Copy(data, 0, toSend, size[0], data.Length);
            return toSend;
        }
        public static byte[] EncryptMessage(byte[] message, Aes aes, int size)
        {
            byte[] msg = new byte[size];
            Array.Copy(message, 0, msg, 0, size);
            return (EncryptMessage(msg, aes));
        }

        public static byte[] DecryptMessage(byte[] message, Aes aes, int tabSize)
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.ISO10126;
            Console.WriteLine("iv received");
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            Console.WriteLine("iv received");
            // Decrypt data
            byte[] decryptedBytes = decryptor.TransformFinalBlock(message, 0, tabSize);
            Console.WriteLine("iv received");
            return decryptedBytes;

        }

        public Aes RecvAes(Socket client)
        {
            byte[] recvMessage = new byte[256];
            int read = client.Receive(recvMessage, 0, 256, SocketFlags.None);
            Aes aes = Aes.Create();
            aes.Key = this.rsa.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA1);
            client.Receive(recvMessage, 0, 256, SocketFlags.None);
            aes.IV = this.rsa.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA1);
            return aes;
        }
    }
}