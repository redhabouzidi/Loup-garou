using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public class NetworkManager : MonoBehaviour
{
    public static List<answer> rep;
    public static Socket client;
    public static int id;
    public static string username;
    static bool connected = false;
    public static GameObject canvas;
    public static GameObject gmo;
    public static GameManager gm;
    public static GameObject sp;
    public static GameObject wso;
    public static WaitingScreen ws;
    public static WPlayer[] players;
    public class answer
    {
        public bool error;
        public int errType;
        public int code;
        public byte[] message;
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
    //unity variables

    // Start is called before the first frame update
    void Start()
    {
        rep = new List<answer>();
        canvas = GameObject.Find("Canvas");

        sp = canvas.transform.Find("StartPage").gameObject;
        wso = canvas.transform.Find("WaitingScreen").gameObject;
        ws = wso.GetComponent<WaitingScreen>();
        Task.Run(() =>
        {
            reseau();
        });
    }

    // Update is called once per frame
    void Update()
    {

        while (rep.Count != 0)
        {
            treatMessage(rep[0]);
            rep.RemoveAt(0);
        }

    }

    public static void reseau()
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
        bool a = true;
        if (!connected)
        {
            try
            {

                int port = 18000;
                /*int port = 10000;*/
                /*string ia = "127.0.0.1";*/
                string ia = "185.155.93.105";
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.Connect(iep);

                Console.Write("Connected to the server\n");
                connected = true;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                a = false;
            }
        }
        /*var inputTask = Task.Run(() =>
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
        });*/



        while (a)
        {
            answer r = recvMessage(client);
            rep.Add(r);
            Debug.Log("dans le while " + rep.Count);
        }
    }

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
        for (int i = 0; i < idPlayers.Length; i++)
        {
            Debug.Log("username = " + playerNames[i] + " id = " + idPlayers[i]);
            ws.addplayer(playerNames[i], idPlayers[i]);
        }
        Debug.Log("heyyyyyy");
    }

    public static void addGameInfo(int id, string username)
    {
        ws.addplayer(username, id);
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
        var ia = "185.155.93.105";
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), 10000);
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
        bool error = false;
        int[] size = new int[1], idPlayers;
        byte[] message = new byte[5000];
        int dataSize, tableSize, recvSize, idPlayer, idp, code = 0, errType = 0;
        recvSize = server.Receive(message);
        Debug.Log("recv" + message[0]);
        if (message.Length == 0)
        {
            Application.Quit();
            return new answer(true, 0, 0, null);
        }
        else
        {
            code = message[0];
            switch (message[0])
            {
                case 0:
                    Debug.Log("test");
                    break;
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

                    break;
                case 6:
                    size[0] = 1;
                    idPlayer = decode(message, size);
                    idp = decode(message, size);
                    Console.WriteLine("vous etes amoureux avec {0} et son role est {1}", idPlayer, idp);
                    return new answer(false, 0, 0, null);
                case 8:
                    Console.WriteLine("afficher le mort pour la sorciere");
                    return new answer(false, 0, 0, null);
                case 9:
                    break;
                case 101:
                    break;
                case 102:
                    break;
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
                        NetworkManager.id = decode(message, size);
                        NetworkManager.username = decodeString(message, size);

                    }
                    else
                    {
                        error = true;
                        errType = 1;
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
                        Console.WriteLine("non connect�");
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
            return new answer(error, errType, code, message);
        }
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

    public static void treatMessage(answer r)
    {

        // if (r.code != 0)
        // {
        Debug.Log("code == " + r.code);
        if (r.error)
        {
            if (r.code == 105)
            {
                LoadScene("jeu");

            }
        }
        else
        {
            int[] size = new int[1] { 1 };
            switch (r.code)
            {
                case 0:
                    // addChatMessage(Encoding.ASCII.GetString(message, size[0], message.Length - size[0]));
                    Debug.Log("I'm here");
                    gm.SendMessageToChat(decodeString(r.message, size), Message.MsgType.player);
                    Debug.Log("I am over here");
                    break;
                case 105:

                    join(client, 0, id, username);
                    break;
                case 101:
                    sp.SetActive(false);
                    wso.SetActive(true);

                    string name = decodeString(r.message, size);
                    int tableSize = decode(r.message, size);
                    int[] idPlayers = new int[tableSize];
                    string[] playerNames = new string[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        idPlayers[i] = decode(r.message, size);
                    }
                    for (int i = 0; i < tableSize; i++)
                    {
                        playerNames[i] = decodeString(r.message, size);

                    }
                    setGameInfo(name, idPlayers, playerNames);
                    foreach (WPlayer p in ws.players_waiting)
                        Debug.Log("uname=" + p.GetUsername());
                    break;

                case 102:

                    int idP = decode(r.message, size);
                    string usernameP = decodeString(r.message, size);
                    addGameInfo(idP, usernameP);
                    break;
                case 9:

                    int val = decode(r.message, size);
                    int[] ids = new int[val];
                    for (int i = 0; i < val; i++)
                    {
                        ids[i] = decode(r.message, size);
                    }
                    val = decode(r.message, size);
                    int[] roles = new int[val];
                    Dictionary<int, int> dictJoueur = new Dictionary<int, int>();
                    for (int i = 0; i < val; i++)
                    {
                        roles[i] = decode(r.message, size);
                        dictJoueur[ids[i]] = roles[i];
                    }

                    for (int i = 0; i < ws.players_waiting.Count; i++)
                    {

                        ws.players_waiting[i].SetRole(dictJoueur[ws.players_waiting[i].GetId()]);
                    }
                    players = new WPlayer[ws.players_waiting.Count];
                    ws.players_waiting.CopyTo(players);
                    LoadScene("game_scene");

                    break;
                case 7:

                    val = decode(r.message, size);
                    int role = decode(r.message, size);
                    foreach (Player p in gm.listPlayer)
                    {
                        if (p.GetId() == val)
                        {
                            p.SetRole(role);
                            p.SetIsAlive(false);
                        }
                        Debug.Log(p.GetIsAlive());
                    }
                    break;
            }
        }
    }
}
