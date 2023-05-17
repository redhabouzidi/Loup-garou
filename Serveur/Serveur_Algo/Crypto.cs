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

        public static List<byte[]> DecryptMessage(byte[] message, Aes aes, int tabSize,Socket client)
        {
            bool read=true;
            List<byte[]> messages=new List<byte[]>();
            int[] size=new int[]{0};
            while(size[0]<tabSize){
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.ISO10126;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            int taille=0;
            if((size[0]+sizeof(int))>tabSize){
                byte[] temp=new byte[sizeof(int)];
                Array.Copy(message, size[0], temp, 0, tabSize-size[0]);
                int newsize=client.Receive(message);
                Array.Copy(message, 0, temp, tabSize-size[0], sizeof(int)-(tabSize-size[0]));
                int[] newint=new int[1]{0};
                taille=Messages.decodeInt(temp,newint);
                tabSize=newsize;
                size[0]=sizeof(int)-(tabSize-size[0]);

            }else{

            }
            taille=Messages.decodeInt(message,size);
            
            byte[] defragmentedMessage=new byte[taille];
            Array.Copy(message, size[0], defragmentedMessage, 0, taille);
            
            
            // Decrypt data
            if((size[0]+taille)>tabSize){
                int newsize=client.Receive(message);
                int diff=tabSize-size[0];
                Array.Copy(message, 0, defragmentedMessage, diff, taille-diff);
                tabSize=newsize;
                byte[] decryptedBytes = decryptor.TransformFinalBlock(defragmentedMessage, 0, taille);
                messages.Add(decryptedBytes);
                size[0]=taille-diff;


            }else{
                
                byte[] decryptedBytes = decryptor.TransformFinalBlock(defragmentedMessage, 0, taille);
                messages.Add(decryptedBytes);
                size[0]+=taille;
            }
            
            
            }
            return messages;

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