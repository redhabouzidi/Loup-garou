using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;

public class Crypto
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
        aes.GenerateKey();
        byte[] aesKey = aes.Key;
        byte[] encryptedKey = pubKey.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA1);
        server.Send(encryptedKey, encryptedKey.Length, SocketFlags.None);
        return aes;
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
}