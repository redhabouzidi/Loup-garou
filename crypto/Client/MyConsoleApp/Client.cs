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

        // Recevoir la longueur du certificat
        byte[] lengthBytes = new byte[4];
        stream.Read(lengthBytes, 0, 4);
        int length = BitConverter.ToInt32(lengthBytes, 0);

        // Recevoir le certificat
        byte[] certBytes = new byte[length];
        stream.Read(certBytes, 0, length);
        X509Certificate2 cert = new X509Certificate2(certBytes);
        var publicKey = (RSA)cert.PublicKey.Key;

        // Afficher les détails du certificat
        Console.WriteLine("Certificat reçu depuis le serveur ");
        // Console.WriteLine(cert.ToString(true));
        // byte[] dataToEncrypt=Encoding.ASCII.GetBytes(test);
        // byte[] encryptedData=publicKey.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA1);

        // stream.Write(encryptedData,0,encryptedData.Length);
        // Console.WriteLine("message sent : {0} est sa taille est de {1}",Encoding.ASCII.GetString(encryptedData),encryptedData.Length);

        Aes aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        byte[] encryptedData = publicKey.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512);

        stream.Write(encryptedData, 0, encryptedData.Length);

        Console.WriteLine("AES key sent");

        String test = "je suis un test";
        byte[] dataToEncrypt = Encoding.ASCII.GetBytes(test);
        var encryptor = aes.CreateEncryptor();
        // byte[] encryptedBytes;
        // using (var encryptor = aes.CreateEncryptor())
        // using (var ms = new MemoryStream())
        // using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        // {
        //     // Écrire le vecteur d'initialisation dans le flux de sortie
        //     ms.Write(aes.IV, 0, aes.IV.Length);

        //     // Chiffrer les données dans le flux de chiffrement
        //     cs.Write(dataToEncrypt, 0, dataToEncrypt.Length);
        //     cs.FlushFinalBlock();

        //     // Lire les données chiffrées depuis le flux de sortie
        //     encryptedBytes = ms.ToArray();
        // }

        // // Convertir les données chiffrées en base 64 pour l'envoi
        // // string encryptedMessageBase64 = Convert.ToBase64String(encryptedBytes);

        // byte[] data = new byte[encryptedBytes.Length+ aes.IV.Length];

        // int index=ajoutIv(data,aes.IV);
        // ajoutMessage(data,encryptedBytes,index);

        byte[] data = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
        byte[] combinedData = new byte[aes.IV.Length + data.Length];
        Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length);
        Array.Copy(data, 0, combinedData, aes.IV.Length, data.Length);
        stream.Write(combinedData, 0, combinedData.Length);


        Console.WriteLine("crypted messsage sent {0}", combinedData.Length);
        // Fermer la connexion avec le serveur
        stream.Close();
        client.Close();
    }

    public static int ajoutIv(byte[] message, byte[] iv)
    {
        Array.Copy(iv, 0, message, 0, iv.Length);
        return iv.Length;
    }

    public static void ajoutMessage(byte[] result, byte[] message, int index)
    {
        Array.Copy(message, 0, result, index, message.Length);
    }
}
