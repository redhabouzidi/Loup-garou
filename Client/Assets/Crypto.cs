using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class Crypto : MonoBehaviour
{

    public static RSA RecvCertificate(Socket server)
    {
        // Recevoir la longueur du certificat
        byte[] lengthBytes = new byte[4];
        server.Receive(lengthBytes, 0, 4, SocketFlags.None);
        int length = BitConverter.ToInt32(lengthBytes, 0);

        // Recevoir le certificat
        byte[] certBytes = new byte[length];
        server.Receive(certBytes, 0, length, SocketFlags.None);
        X509Certificate2 cert = new X509Certificate2(certBytes);
        RSA publicKey = (RSA)cert.PublicKey.Key;
        return publicKey;
    }

    public static Aes SendAes(Socket server, RSA pubKey)
    {
        Aes aes = Aes.Create();
        aes.GenerateIV();
        aes.GenerateKey();
        byte[] aesKey = aes.Key;
        byte[] encryptedKey = pubKey.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA1);
        server.Send(encryptedKey, encryptedKey.Length, SocketFlags.None);

        byte[] encryptedIV = pubKey.Encrypt(aes.IV, RSAEncryptionPadding.OaepSHA1);
        server.Send(encryptedIV, encryptedIV.Length, SocketFlags.None);
        return aes;
    }
    public static void DecryptMessage(byte[] message, Aes aes, int tabSize)
    {
        Console.WriteLine(BitConverter.ToString(message));
        int[] size = new int[1] { 0 };
        
        byte[] decryptedBytes = new byte[0];
        int taille=0;
        while (size[0] < tabSize)
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.ISO10126;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            taille = NetworkManager.decode(message, size);
            Debug.Log(BitConverter.ToString(message));
            byte[] msg = new byte[taille];
            Debug.Log(taille);
            Array.Copy(message, size[0], msg, 0, taille);
            Debug.Log(BitConverter.ToString(msg));
            byte[] decryptedPart = decryptor.TransformFinalBlock(msg, 0, taille);
            NetworkManager.rep.Add(decryptedPart);
            Debug.Log(BitConverter.ToString(decryptedPart));
            size[0] += taille;
        }
    }
    public static byte[] EncryptMessage(byte[] message, Aes aes)
    {

        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.ISO10126;
        var encryptor = aes.CreateEncryptor();
        byte[] data = encryptor.TransformFinalBlock(message, 0, message.Length);
        byte[] toSend = new byte[data.Length + sizeof(int)];
        int[] size = new int[1] { 0 };
        NetworkManager.encode(toSend, data.Length, size);
        Array.Copy(data, 0, toSend, size[0], data.Length);
        Debug.Log(BitConverter.ToString(toSend));
        return toSend;
    }
}