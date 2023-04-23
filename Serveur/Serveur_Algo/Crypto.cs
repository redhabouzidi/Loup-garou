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
            return (client.Send(message, message.Length, SocketFlags.None));
        }

        public static byte[] EncryptMessage(byte[] message, Aes aes)
        {
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            var encryptor = aes.CreateEncryptor();
            byte[] data = encryptor.TransformFinalBlock(message, 0, message.Length);
            byte[] combinedData = new byte[aes.IV.Length + data.Length];
            Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length);
            Array.Copy(data, 0, combinedData, aes.IV.Length, data.Length);

            return combinedData;
        }
        public static byte[] EncryptMessage(byte[] message, Aes aes,int size)
        {
            byte [] msg = new byte[size];
            Array.Copy(message,0,msg,0,size);
            return (EncryptMessage(msg,aes));
        }

        public static byte[] DecryptMessage(byte[] message, Aes aes, int tabSize)
        {
            aes.Mode = CipherMode.CBC;
            byte[] encryptedData = new byte[tabSize - aes.IV.Length];
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(message, 0, iv, 0, aes.IV.Length);
            Array.Copy(message, aes.IV.Length, encryptedData, 0, tabSize - aes.IV.Length);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            // Decrypt data
            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return decryptedBytes;
        }

        public Aes RecvAes(Socket client)
        {
            byte[] recvMessage = new byte[256];
            int read = client.Receive(recvMessage,0,256,SocketFlags.None);
            Aes aes = Aes.Create();
            aes.Key = this.rsa.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA1);
            return aes;
        }
    }
}