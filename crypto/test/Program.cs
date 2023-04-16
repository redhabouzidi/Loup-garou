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


        byte[] certBytes = cert.Export(X509ContentType.Cert);
        byte[] certLength = BitConverter.GetBytes(certBytes.Length);
        stream.Write(certLength, 0, certLength.Length);
        stream.Write(certBytes, 0, certBytes.Length);
        byte[] recvMessage = new byte[512];
        int read = stream.Read(recvMessage, 0, 512);

        Aes aes = Aes.Create();
        aes.Key = rsa.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA512);

        // read = stream.Read(recvMessage, 0, 512);

        // aes.IV = rsa.Decrypt(recvMessage, RSAEncryptionPadding.OaepSHA512);

        aes.Mode = CipherMode.CBC;

        byte[] recv = new byte[4096];
        read = stream.Read(recv, 0, 4096);
        Console.WriteLine("read=={0}", read);
        Console.WriteLine(aes.IV.Length);
        // byte[] iv = new byte[aes.BlockSize / 8];
        // byte[] encryptedBytes = new byte[read - iv.Length];

        // Array.Copy(recv, 0, iv, 0, iv.Length);
        // aes.IV = iv;
        // Array.Copy(recv,iv.Length,encryptedBytes,0,read-iv.Length);
        // Console.WriteLine("le message avant de dechiffrage {0}",Encoding.ASCII.GetString(encryptedBytes));
        // Buffer.BlockCopy(recv, 0, encryptedBytes, 0, read-iv.Length);


        // string encryptedMessageBase64 = Encoding.ASCII.GetString(encryptedBytes);
        // byte[] encryptedMessage = Convert.FromBase64String(encryptedMessageBase64);

        // string decryptedMessage;


        // ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        // using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
        // {
        //     using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //     {
        //         using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //         {
        //             decryptedMessage = srDecrypt.ReadToEnd();
        //         }
        //     }
        // }

        byte[] encryptedData = new byte[read];
        Array.Copy(recv, 0, encryptedData, 0, read);
        

        // Split IV and encrypted data
        byte[] iv = new byte[aes.IV.Length];
        byte[] encryptedBytes = new byte[encryptedData.Length - aes.IV.Length];
        Array.Copy(encryptedData, 0, iv, 0, aes.IV.Length);
        Array.Copy(encryptedData, aes.IV.Length, encryptedBytes, 0, encryptedData.Length - aes.IV.Length);
        Console.WriteLine("le message avant de dechiffrage {0}",Encoding.ASCII.GetString(encryptedBytes));

        // Set IV on aes instance
        aes.IV = iv;

        // Create decryptor
        ICryptoTransform decryptor = aes.CreateDecryptor();

        // Decrypt data
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        // Convert decrypted bytes to string
        string decryptedString = Encoding.ASCII.GetString(decryptedBytes);
        Console.WriteLine("le iv est de {0}", Encoding.ASCII.GetString(iv));
        Console.WriteLine("Message déchiffré : " + decryptedString);

    }
}


////////////////////////////////////////////////////////////////////////////////////////////
