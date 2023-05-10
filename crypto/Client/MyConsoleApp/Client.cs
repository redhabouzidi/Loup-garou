using System;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Text;
public class Client
{
    static void Main(string[] args)
    {
        TcpClient client = new TcpClient("127.0.0.1", 8080);
        NetworkStream stream = client.GetStream();

        var publicKey = RecvCertificate(stream);

        // Afficher les détails du certificat
        Console.WriteLine("Certificat reçu depuis le serveur ");

        Aes aes = SendAes(stream, publicKey);
        
        Console.WriteLine("AES key sent");

        
        String test = "je suis un test";
        byte[] dataToEncrypt = Encoding.ASCII.GetBytes(test);

        byte[] data = EncryptMessage(Encoding.ASCII.GetBytes(test),aes);
        stream.Write(data, 0, data.Length);


        Console.WriteLine("crypted messsage sent {0} : {1}", data.Length,Encoding.ASCII.GetString(data,0,data.Length));
        byte[] text=new byte[1024];
        int read=stream.Read(text,0,1024);

        Console.WriteLine("le message est : {0}",Encoding.ASCII.GetString(DecryptMessage(text,aes,read)));
        // Fermer la connexion avec le serveur
        stream.Close();
        client.Close();
    }
    public static Aes SendAes(NetworkStream server, RSA publicKey)
    {
        Aes aes = Aes.Create();
        aes.GenerateKey();
        byte[] encryptedKey = publicKey.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512);
        server.Write(encryptedKey, 0, encryptedKey.Length);
        return aes;
    }

    public static RSA RecvCertificate(NetworkStream server)
    {
        // Recevoir la longueur du certificat
        byte[] lengthBytes = new byte[4];
        server.Read(lengthBytes, 0, 4);
        int length = BitConverter.ToInt32(lengthBytes, 0);

        // Recevoir le certificat
        byte[] certBytes = new byte[length];
        server.Read(certBytes, 0, length);
        X509Certificate2 cert = new X509Certificate2(certBytes);
        RSA publicKey = (RSA)cert.PublicKey.Key;

        return publicKey;
    }

    public static byte[] EncryptMessage(byte[] message,Aes aes)
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

    public static byte[] DecryptMessage(byte[] message, Aes aes, int tabSize)
    {
        aes.Mode = CipherMode.CBC;
        byte[] encryptedData = new byte[tabSize-aes.IV.Length];
        byte[] iv=new byte[aes.IV.Length];
        Array.Copy(message,0,iv,0,aes.IV.Length);
        Array.Copy(message, aes.IV.Length, encryptedData, 0, tabSize-aes.IV.Length);
        aes.IV=iv;
        ICryptoTransform decryptor = aes.CreateDecryptor();
        // Decrypt data
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        return decryptedBytes;
    }
}