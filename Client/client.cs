using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;




public class Client
{
    static int id;
    static string username;
    public class answer
    {
        bool error;
        int errType;
        int code;
        byte[] message;
        public answer()
        {
            this.error = false;
            this.errType = 0;
            this.code = 0;
            this.message = null;
        }
        public answer(bool error, int errType, int code, byte[] message)
        {
            this.error = error;
            this.errType = errType;
            this.code = code;
            this.message = message;

        }
    }
    public static void Main(string[] args)
    {

        /*if (args.Length != 2)
        {
            return;
        }
        int port;
        bool answer = Int32.TryParse(args[0], out port);
        if (answer == false)
        {
            return;
        }
        string ia = args[1];*/
        // int port = 18000;
        int port = 10000;
        string ia = "127.0.0.1";
        // string ia = "185.155.93.105";
        bool a = true, reading = true;
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), port);
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {

            client.Connect(iep);
            Console.Write("Connected to the server\n");
            var inputTask = Task.Run(() =>
            {
                while (reading)
                {
                    byte[] messageBytesAsync;
                    string messageAsync = Console.ReadLine();

                    if (messageAsync.Equals("ajout"))
                    {
                        ajoutAmi(client, id, 1);
                    }else
                    if (messageAsync.Equals("vote"))
                    {
                        Vote(client, id, "jean");
                    }
                    else
                    if (messageAsync.Equals("leave"))
                    {
                        messageBytesAsync = BitConverter.GetBytes(1);

                        int bytesSentAsync = client.Send(messageBytesAsync);
                        Console.WriteLine("sent {0} bytes to the server", bytesSentAsync);
                    }
                    else
                    if (messageAsync.Equals("create"))
                    {
                        createGame(client, id, "demo", "asgard");
                    }
                    else
                    if (messageAsync.Equals("login"))
                    {
                        login(client, "mahmoud", "jesuisunmotdepasse");
                    }
                    else
                    if (messageAsync.Equals("signin"))
                    {
                        sendInscription(client, "mahmoud", "Jesuisunmotdepasse0@", "moumouh.atm@gmail.com");
                    }
                    else
                    if (messageAsync.Equals("join"))
                    {
                        join(client, id, 0, username);
                    }
                    else if (messageAsync.Equals("love"))
                    {
                        ChooseLovers(client, 0, 1);
                    }
                    else
                    {
                        sendchatMessage(client, messageAsync);
                    }
                }
            });


            string message;
            while (a)
            {
                answer rep;
                rep = recvMessage(client);
            }
        }
        catch (Exception e)
        {
            Console.Write(e.Message);
            a = false;
        }


    }
    public static void addChatMessage(string message)
    {
        Console.WriteLine(message);
    }
    public static void vote(int vote, int player)
    {
        Console.WriteLine($"player {vote} votes for {player}");
    }
    public static void setGameInfo(string name, int[] idPlayers, string[] playerNames)
    {
        Console.WriteLine($"game name = {name}");
        for (int i = 0; i < idPlayers.Length; i++)
            Console.Write($"player  {idPlayers[i]} is {playerNames[i]} ");

    }
    public static void SetCurrentGame(int[] nbPlayers, int[] gameId, string[] name)
    {
        for (int i = 0; i < nbPlayers.Length; i++)
        {
            Console.WriteLine($"the game {name[i]} is created by {gameId[i]} with {nbPlayers[i]} current players");
        }
    }
    public static int decode(byte[] message, int[] size)
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
        int dataSize = decode(message, size);
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
    public static void setInformationCompte(bool answer, int id, string username)
    {
        Console.WriteLine($"demande de connexion {answer} with id {id} and username{username}");
    }
    public static Socket LinkToServer()
    {
        var ia = "127.0.0.1";
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), 8000);
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(iep);
        return client;
    }

    public static int login(Socket server, string username, string password)
    {
        int userSize = username.Length;
        int pwSize = password.Length;
        int msgSize = 1 + 2 * sizeof(int) + userSize + pwSize;
        byte[] message = new byte[msgSize];

        message[0] = 105;
        int[] size = new int[1] { 1 };
        encode(message, username, size);
        encode(message, password, size);

        return SendMessageToServer(server, message);
    }

    public static int logout(Socket server)
    {
        try
        {
            byte[] message = new byte[1] { 100 };
            server.Send(message, 1, SocketFlags.None);
            return 0;
        }
        catch (SocketException)
        {
            return -1;
        }
    }

    public static answer recvMessage(Socket server)
    {
        int[] size = new int[1];
        int dataSize, tableSize;
        string name;
        int[] idPlayers;
        string[] playerNames;
        byte[] message = new byte[5000];
        int recvSize = server.Receive(message), idPlayer, idp;
        if (message.Length == 0)
        {
            return new answer(true, 0, 0, null);
        }
        else
        {
            switch (message[0])
            {
                case 0:
                    size[0] += 1;
                    addChatMessage(Encoding.ASCII.GetString(message, size[0], message.Length - size[0]));
                    return new answer(false, 0, 0, null);
                case 1:
                    vote(BitConverter.ToInt32(message, 1), BitConverter.ToInt32(message, 1 + sizeof(int)));
                    return new answer(false, 0, 0, null);
                case 5:
                    size[0] = 1;
                    bool day = decodeBool(message, size);
                    if (day)
                    {
                        Console.WriteLine("C'est le jour ");
                    }
                    else
                    {
                        Console.WriteLine("C'est la nuit");
                    }

                    return new answer(false, 0, 0, null);
                case 6:
                    size[0] = 1;
                    idPlayer = decode(message, size);
                    idp = decode(message, size);
                    Console.WriteLine("vous etes amoureux avec {0} et son role est {1}", idPlayer, idp);
                    return new answer(false, 0, 0, null);
                case 8:
                    Console.WriteLine("afficher le mort pour la sorciere");
                    return new answer(false, 0, 0, null);
                case 101:
                    size[0] = 1;
                    name = decodeString(message, size);
                    tableSize = decode(message, size);
                    idPlayers = new int[tableSize];
                    playerNames = new string[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    for (int i = 0; i < tableSize; i++)
                    {
                        playerNames[i] = decodeString(message, size);

                    }
                    setGameInfo(name, idPlayers, playerNames);
                    return new answer(false, 0, 0, null);
                case 102:
                    size[0] = 1;
                    bool answer = decodeBool(message, size);
                    if (answer == false)
                    {
                        setInformationCompte(false, 0, "");
                        return new answer(true, 1, 0, null);
                    }
                    else
                    {
                        id = decode(message, size);
                        string temp = decodeString(message, size);
                        setInformationCompte(true, id, temp);
                        return new answer(false, 0, 0, null);
                    }
                case 103:
                    size[0] = 1;
                    tableSize = decode(message, size);
                    int[] nbPlayers = new int[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        nbPlayers[i] = decode(message, size);
                    }
                    tableSize = decode(message, size);
                    int[] gameId = new int[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        gameId[i] = decode(message, size);
                    }
                    string[] gameName = new string[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        gameName[i] = decodeString(message, size);
                    }
                    SetCurrentGame(nbPlayers, gameId, gameName);
                    return new answer();
                case 105:
                    size[0] = 1;
                    if (decodeBool(message, size))
                    {
                        id = decode(message, size);
                        username = decodeString(message, size);

                        Console.WriteLine($"l'utilisateur {username} avec l'id {pid} s'est connecté  ");
                    }
                    else
                    {
                        Console.WriteLine("non connecté");
                    }
                    break;
                case 104:
                    size[0] = 1;
                    if (decodeBool(message, size))
                    {
                        Console.WriteLine($"l'utilisateur  s'est inscrit");
                    }
                    else
                    {
                        Console.WriteLine("non connecté");
                    }
                    break;

                case 110:
                    dataSize = decode(message, size);
                    idPlayers = new int[dataSize];
                    for (int i = 0; i < dataSize; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    dataSize = decode(message, size);
                    int[] role = new int[dataSize];
                    for (int i = 0; i < dataSize; i++)
                    {
                        role[i] = decode(message, size);
                    }
                    return new answer();
                case 153:
                    Console.WriteLine("ajout");
                    break;
                case 154:
                    Console.WriteLine("supprimer");
                    break;
                case 155:
                    Console.WriteLine("reponse");
                    break;
                case 255:
                    Console.WriteLine("une erreur est survenue");
                    break;

            }
        }
        answer answeranswer = new answer();
        return answeranswer;
    }
    public static int SendMessageToServer(Socket server, byte[] message)
    {

        Console.WriteLine("msgsize=" + message.Length);
        return server.Send(message, message.Length, SocketFlags.None);
    }

    public static int sendInscription(Socket server, string username, string password, string email)
    {
        int usernameSize = username.Length;
        int pwSize = password.Length;
        int emailSize = email.Length;
        int messageSize = 1 + 4 * sizeof(int) + usernameSize + pwSize + emailSize;
        byte[] message = new byte[messageSize];

        message[0] = 104;
        int[] size = new int[1] { 1 };
        encode(message, username, size);
        encode(message, password, size);
        encode(message, email, size);

        return SendMessageToServer(server, message);
    }

    public static int Vote(Socket server, int idUser, int idVote)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        message[0] = 1;
        int[] size = new int[1] { 1 };
        encode(message, idUser, size);
        encode(message, idVote, size);

        return SendMessageToServer(server, message);
    }
    public static int sendStartKickVote(Socket server, int idPlayer, int voted)
    {

        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 200;
        encode(message, idPlayer, size);
        encode(message, voted, size);

        return SendMessageToServer(server, message);
    }
    public static int sendKickVote(Socket server, int idPlayer, int voted)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        int[] size = new int[1] { 1 };
        message[0] = 201;
        encode(message, idPlayer, size);
        encode(message, voted, size);
        return SendMessageToServer(server, message);
    }
    public static int sendFetchGameRequest(Socket server)
    {
        byte[] message = new byte[1];
        message[0] = 2;

        return SendMessageToServer(server, message);
    }

    public static int sendchatMessage(Socket server, string message)
    {
        byte[] msg = new byte[1 + sizeof(int) + message.Length];
        Console.WriteLine("lllllll {0}", msg.Length);
        msg[0] = 0;
        int[] size = new int[1] { 1 };
        encode(msg, message, size);
        SendMessageToServer(server, msg);
        return 0;
    }
    public static int createGame(Socket server, int id, string username, string name)
    {
        byte[] message = new byte[1 + sizeof(int) * 3 + username.Length + name.Length];
        int[] size = new int[1] { 1 };
        message[0] = 3;
        encode(message, id, size);
        encode(message, username, size);
        encode(message, name, size);

        return SendMessageToServer(server, message);

    }
    public static int join(Socket server, int gameId, int id, string username)
    {
        byte[] message = new byte[1 + sizeof(int) * 3 + username.Length];
        int[] size = new int[1] { 1 };
        message[0] = 4;
        encode(message, gameId, size);
        encode(message, id, size);
        encode(message, username, size);

        return SendMessageToServer(server, message);

    }
    public static int ajoutAmi(Socket server, int idUser, string username)
    {
        byte[] message = new byte[1 + sizeof(int) * 2 + username.Length];
        int[] size = new int[1] { 1 };
        message[0] = 153;
        encode(message, idUser, size);
        encode(message, username, size);

        return SendMessageToServer(server, message);
    }
    public static int supprimerAmi(Socket server, int idUser, int id)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 153;
        encode(message, idUser, size);
        encode(message, id, size);

        return SendMessageToServer(server, message);
    }
    public static int reponseAmi(Socket server, int idUser, int id, bool answer)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 153;
        encode(message, idUser, size);
        encode(message, id, size);
        encode(message, answer, size);
        return SendMessageToServer(server, message);
    }
    public static int ChooseLovers(Socket server, int id1, int id2)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        message[0] = 6;
        int[] index = new int[1] { 1 };
        encode(message, id1, index);
        encode(message, id2, index);
        return SendMessageToServer(server, message);
    }
}