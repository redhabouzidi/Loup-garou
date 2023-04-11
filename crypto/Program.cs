using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Program
{
    public static void Main(String [] args)
    {
        if(args.Length!=1)
        {
            throw new ArgumentException("le nombre d'arguments est incorrecte");
        }
        string privateKeyFile = "private_key.pem";
        string publicKeyFile = "public_key.pem";
        string passwd = args[0];
        byte[] bytes = Encoding.ASCII.GetBytes(passwd);
        ReadOnlySpan<byte> byteSpan = new ReadOnlySpan<byte>(bytes);

        
        string encryptedPrivateKeyBytes = File.ReadAllText(privateKeyFile);
        // byte[] privateKeyBytes = Encoding.ASCII.GetBytes(privateKeyText);
        var rsa = RSA.Create();
        rsa.ImportFromEncryptedPem(encryptedPrivateKeyBytes.AsSpan(),passwd.AsSpan());




        RSA publicKey = RSA.Create();
        string publicKeyContent = File.ReadAllText(publicKeyFile);
        publicKey.ImportFromPem(publicKeyContent.AsSpan());

        string test="assalas";

        Console.WriteLine("string ==\n{0}",test);
        byte[] dataToEncrypt=Encoding.ASCII.GetBytes(test);
        Console.WriteLine("la taille du tableau avant de crypter est de {0}",dataToEncrypt.Length );

        byte[] encryptedData = publicKey.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA1);

        string result=Encoding.ASCII.GetString(encryptedData);
        Console.WriteLine("le resultat est apres le cryptage ==\n{0}",result);
        Console.WriteLine("la taille du tableau est {0}",encryptedData.Length);
        Console.WriteLine("/////////////////////////////////////////////");
        result=Encoding.ASCII.GetString(rsa.Decrypt(encryptedData,RSAEncryptionPadding.OaepSHA1));
        Console.WriteLine("le resultat apres le decryptage est ==\n{0}",result);

    }
}


////////////////////////////////////////////////////////////////////////////////////////////
