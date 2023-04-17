using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using System.Net;

public class Program
{
    public static void Main(String[] args)
    {
        if (args.Length != 1)
        {
            throw new ArgumentException("le nombre d'arguments est incorrecte");
        }
        string privateKeyFile = "private_key.pem";
        // string publicKeyFile = "public_key.pem";
        string passwd = args[0];
        byte[] bytes = Encoding.ASCII.GetBytes(passwd);
        ReadOnlySpan<byte> byteSpan = new ReadOnlySpan<byte>(bytes);


        string encryptedPrivateKeyBytes = File.ReadAllText(privateKeyFile);
        // byte[] privateKeyBytes = Encoding.ASCII.GetBytes(privateKeyText);
        var rsa = RSA.Create();
        rsa.ImportFromEncryptedPem(encryptedPrivateKeyBytes.AsSpan(), passwd.AsSpan());



        X509Certificate2 cert = new X509Certificate2("cert.pem");
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
        server.Start();
        Console.WriteLine("waiting for clients");
        TcpClient client = server.AcceptTcpClient();

        NetworkStream stream = client.GetStream();


        // byte[] certBytes = cert.Export(X509ContentType.Cert);
        // byte[] certLength = BitConverter.GetBytes(certBytes.Length);
        // stream.Write(certLength, 0, certLength.Length);
        // stream.Write(certBytes, 0, certBytes.Length);

        SendCertificateToClient(stream, cert);
        ///////////////////////////////////////////////////////////////////////////
        Aes aes = RecvAes(stream, rsa);
        ///////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////
        byte[] recv = new byte[4096];
        int read = stream.Read(recv, 0, 4096);
        byte[] decryptedBytes = DecryptMessage(recv, aes, read);
        string decryptedString = Encoding.ASCII.GetString(decryptedBytes);
        Console.WriteLine("le message est : {0}", decryptedString);

        String t = "j'ai recu ton message";

        byte[] send=EncryptMessage(Encoding.ASCII.GetBytes(t),aes);
        stream.Write(send,0,send.Length);

    }
    public static void SendCertificateToClient(NetworkStream client, X509Certificate2 cert)
    {
        byte[] certBytes = cert.Export(X509ContentType.Cert);
        byte[] certLength = BitConverter.GetBytes(certBytes.Length);

        byte[] message = certLength.Concat(certBytes).ToArray();


        client.Write(message, 0, message.Length);
    }

    public static Aes RecvAes(NetworkStream client, RSA private_key)
    {
        byte[] recvMessage = new byte[512];
        int read = client.Read(recvMessage, 0, 512);
        Aes aes = Aes.Create();
        aes.Key = private_key.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA512);
        return aes;
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

}


////////////////////////////////////////////////////////////////////////////////////////////
