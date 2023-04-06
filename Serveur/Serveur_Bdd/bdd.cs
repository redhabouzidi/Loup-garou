using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using dataBase;
using MySql.Data.MySqlClient;

public class bdd
{
    public static MySqlConnection conn = new MySqlConnection("Server='127.0.0.1';port=3306;DATABASE='lg_db';user ID='root';password='';Pooling=true;charset='utf8'");
    public static int sendMessage(Socket client, byte[] message)
    {
        return client.Send(message, message.Length, SocketFlags.None);

    }
    public static int getStringLength(string[] tab)
    {
        int size = 0;
        foreach (string s in tab)
        {
            size += s.Length;
        }
        return size;
    }
    public static int decodeInt(byte[] message, int[] size)
    {
        int result = BitConverter.ToInt32(message, size[0]);
        size[0] += sizeof(int);
        return result;
    }
    public static bool decodeBool(byte[] message, int[] size)
    {
        bool result = BitConverter.ToBoolean(message, size[0]);
        size[0] += sizeof(bool);
        return result;
    }
    public static string decodeString(byte[] message, int[] size)
    {
        int dataSize = decodeInt(message, size);
        string name = Encoding.ASCII.GetString(message, size[0], dataSize);
        size[0] += dataSize;

        return name;
    }
    public static void encode(byte[] message, int val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(int));
        size[0] += sizeof(int);
    }
    public static void encode(byte[] message, bool val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(bool));
        size[0] += sizeof(bool);
    }
    public static void encode(byte[] message, string val, int[] size)
    {
        encode(message, val.Length, size);
        Console.WriteLine(size[0]);
        Array.Copy(Encoding.ASCII.GetBytes(val), 0, message, size[0], val.Length);
        size[0] += val.Length;

    }
    public static void encodeString(byte[] message, string val, int[] size)
    {
        string name = Encoding.ASCII.GetString(message, size[0], val.Length);
        size[0] += val.Length;

    }

    public static void Main(string[] args)
    {




        conn.Open();
        Console.WriteLine("ca marche");
        Socket bdd = setupSocketServer();
        List<Socket> server = new List<Socket> { };
        byte[] message = new byte[4096];
        while (true)
        {

            bdd.Poll(-1, SelectMode.SelectRead);
            if (bdd.Available != 0)
            {
                int val = bdd.Receive(message);

                switch (message[0])
                {
                    case 105:
                        connexion(bdd, message);
                        break;
                    case 104:
                        inscription(bdd, message);
                        break;
                    case 153:
                        ajoutAmi(bdd, message);
                        break;
                    case 154:
                        supprimerAmi(bdd, message);
                        break;
                    case 155:
                        reponseAmi(bdd, message);
                        break;

                }
            }
            else
            {
                bdd.Close();
                break;
            }
        }

    }

    public static Socket setupSocketServer()
    {
        string ia = "192.168.100.116";
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), 10001);
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(iep);
        Console.WriteLine("Conenxion to server achieved");

        return client;
    }
    public static void inscription(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        string username = decodeString(message, size);
        string password = decodeString(message, size);
        string email = decodeString(message, size);
        Console.WriteLine("username=" + username + " password=" + password + " email=" + email);
        int val = Inscription.inscription_user(conn, username, email, password);
        inscriptionAnswer(bdd, queueId, true, 7);

    }
    public static int inscriptionAnswer(Socket bdd, int queueId, bool answer, int id)
    {
        int msgSize = 1 + sizeof(int) * 2 + sizeof(bool);
        byte[] message = new byte[msgSize];
        int[] size = new int[1] { 1 };
        message[0] = 104;
        encode(message, queueId, size);
        encode(message, answer, size);
        encode(message, id, size);
        bdd.Send(message);
        return 0;
    }

    public static int connexionAnswer(Socket bdd, int queueId, bool answer, int idPlayer, string username,int[] friends)
    {
        int msgSize = 1 + sizeof(bool) + sizeof(int) * 3 + username.Length+sizeof(int)+friends.Length*sizeof(int);
        byte[] message = new byte[msgSize];
        int[] size = new int[1] { 1 };
        message[0] = 105;
        encode(message, queueId, size);
        encode(message, answer, size);
        encode(message, idPlayer, size);
        encode(message, username, size);
        encode(message,friends.Length,size);
        foreach(int i in friends)
        {
            encode(message, i, size);
        }
        Console.WriteLine("hey");
        return bdd.Send(message);
    }
    public static void connexion(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        string username = decodeString(message, size);
        string password = decodeString(message, size);
        Console.WriteLine($"l'utilisateur {username} avec l'id 5 et le mdp {password} s'est connecté");
        if (Login.login_user(conn, username, password) == 0)
        {
            int id=Login.get_id(conn, username);
            //recuperer les amis
            connexionAnswer(bdd, queueId, true, id, username,new int[0]);
        }
        else
            connexionAnswer(bdd, queueId, false, 0, username, new int[0] );
    }

    public static int ajoutAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        string username = decodeString(message, size);
        Console.WriteLine($"l'utilisateur {id} rajoute la personne avec le nom {username}");
        byte[] newMessage = new byte[1 + sizeof(int) + sizeof(bool)];
        size[0] = 1;
        newMessage[0] = 153;
        encode(newMessage, queueId, size);
        encode(newMessage, true, size);
        return sendMessage(bdd, newMessage);

    }
    public static int supprimerAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idDelete = decodeInt(message, size);
        Console.WriteLine($"l'utilisateur {id} supprime la personne avec le nom {idDelete}");
        byte[] newMessage = new byte[1 + sizeof(int) + sizeof(bool)];
        size[0] = 1;
        newMessage[0] = 154;
        encode(newMessage, queueId, size);
        encode(newMessage, true, size);
        return sendMessage(bdd, newMessage);

    }
    public static int reponseAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idReponse = decodeInt(message, size);
        bool answer = decodeBool(message, size);
        Console.WriteLine($"l'utilisateur {id} repond a l'invitation de {idReponse} avec {answer}");
        byte[] newMessage = new byte[1 + sizeof(int) + sizeof(bool)];
        size[0] = 1;
        newMessage[0] = 155;
        encode(newMessage, queueId, size);
        encode(newMessage, true, size);
        return sendMessage(bdd, newMessage);

    }
    public static int sendPlayerInfo(Socket bdd, int queueId, bool answer, int idPlayer, string username)
    {
        int msgSize = 1 + sizeof(int) * 3 + sizeof(bool) + username.Length;
        byte[] message = new byte[msgSize];
        message[0] = 105;
        int[] size = new int[1] { 1 };
        encode(message, queueId, size);
        encode(message, answer, size);
        encode(message, idPlayer, size);
        encode(message, username, size);
        return bdd.Send(message);
    }
    public static int sendLobbies(Socket bdd, int queueId, int[] idRoom, int[] nbPlayers, string[] name)
    {
        int pname = getStringLength(name);
        int msgSize = 1 + sizeof(int) * (2 + idRoom.Length + nbPlayers.Length) + pname;
        byte[] message = new byte[msgSize];
        int[] size = new int[1] { 1 };
        message[0] = 103;
        encode(message, queueId, size);
        encode(message, idRoom.Length, size);
        for (int i = 0; i < idRoom.Length; i++)
        {
            encode(message, idRoom[i], size);
        }
        for (int i = 0; i < idRoom.Length; i++)
        {
            encode(message, nbPlayers[i], size);
        }
        for (int i = 0; i < idRoom.Length; i++)
        {
            encode(message, name[i], size);
        }
        return bdd.Send(message);

    }

}