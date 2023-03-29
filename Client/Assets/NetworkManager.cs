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
    public static int nbplayeres = 2, time;
    public static bool prog = true, inst = false;
    public static List<byte[]> rep;
    public static Socket client;
    public static int id, tour;
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
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        rep = new List<byte[]>();
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
            if (rep[0] == null)
            {
                rep.RemoveAt(0);
                GameManagerApp.exitGame();
            }
            else
            {
                treatMessage(rep[0]);
            }
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

        if (!connected)
        {
            try
            {

                int port = 18000;
                string ia = "185.155.93.105";
                // int port = 10000;
                // string ia = "127.0.0.1";
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.Connect(iep);

                Console.Write("Connected to the server\n");
                connected = true;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                prog = false;
            }
        }



        while (prog)
        {
            recvMessage(client);
        }

        client.Close();
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
        players = new WPlayer[idPlayers.Length];
        for (int i = 0; i < idPlayers.Length; i++)
        {
            players[i] = new WPlayer(playerNames[i], idPlayers[i]);
        }
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
        Debug.Log("data =" + dataSize);
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



    public static void recvMessage(Socket server)
    {

        byte[] message = new byte[5000];
        int recvSize;
        server.Poll(-1, SelectMode.SelectRead);
        if (server.Available == 0)
        {
            prog = false;
            rep.Add(null);
            return;
        }
        recvSize = server.Receive(message);
        Debug.Log("recv =" + message[0]);
        byte[] newMessage = new byte[recvSize];
        Array.Copy(message, 0, newMessage, 0, recvSize);
        rep.Add(newMessage);
        return;

    }
    public static void treatMessage(byte[] message)
    {
        Debug.Log(message == null);
        Dictionary<int, int> dictJoueur;
        bool read = true;
        int[] idPlayers, ids, roles, nbPlayers, gameId;
        string[] playerNames, gameName;
        int dataSize, tableSize, idPlayer, idp, msgSize = 0, val, role, idP, win;
        string name, usernameP;
        int[] size = new int[1] { 0 };
        string result = "";
        Debug.Log(BitConverter.ToString(message));
        while (read)
        {
            Debug.Log("code == " + message[size[0]]);

            switch (message[size[0]++])
            {
                case 0:
                    Debug.Log("I'm here");
                    gm.SendMessageToChat(decodeString(message, size), Message.MsgType.player);
                    Debug.Log("I am over here");
                    break;
                case 1:
                    size[0] = sizeof(int) * 2 + 1;
                    vote(BitConverter.ToInt32(message, 1), BitConverter.ToInt32(message, 1 + sizeof(int)));
                    break;
                case 5:
                    bool ra = decodeBool(message, size);
                    GameManager.isNight = !ra;
                    if (!ra)
                    {
                        gm.tour++;
                    }
                    tour = 1;
                    break;
                case 6:
                    idPlayer = decode(message, size);
                    idp = decode(message, size);
                    gm.lover1_id = gm.p.GetId();
                    gm.lover2_id = idPlayer;
                    string msg = "vous etes amoureux avec " + gm.listPlayer[idPlayer].GetPseudo() + " et son role est ";
                    switch (idp)
                    {
                        case 1:
                            msg += "Villageois";
                            break;
                        case 2:
                            msg += "Cupidon";
                            break;
                        case 3:
                            msg += "Voyante";
                            break;
                        case 4:
                            msg += "Loup-garou";
                            break;
                        case 5:
                            msg += "Sorciere";
                            break;
                    }
                    gm.SendMessageToChat(msg, Message.MsgType.system);
                    break;
                case 7:
                    idPlayer = decode(message, size);
                    role = decode(message, size);
                    gm.affiche_text_role(idPlayer, role);
                    break;
                case 8:
                    idPlayer = decode(message, size);
                    gm.ActionSorciere(idPlayer);
                    break;
                case 9:
                    val = decode(message, size);

                    ids = new int[val];
                    for (int i = 0; i < val; i++)
                    {
                        ids[i] = decode(message, size);
                    }
                    val = decode(message, size);
                    roles = new int[val];
                    dictJoueur = new Dictionary<int, int>();
                    for (int i = 0; i < val; i++)
                    {
                        roles[i] = decode(message, size);
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
                case 10:
                    val = decode(message, size);
                    role = decode(message, size);
                    foreach (Player p in gm.listPlayer)
                    {
                        if (p.GetId() == val)
                        {
                            if (p.GetId() == gm.p.GetId())
                            {
                                Debug.Log("mort");
                                gm.p.SetIsAlive(false);
                            }
                            p.SetRole(role);
                            p.SetIsAlive(false);
                        }

                        Debug.Log(p.GetIsAlive());
                    }
                    gm.MiseAJourAffichage();
                    break;
                case 11:
                    tour = decode(message, size);

                    if (tour == 2 && gm.p.GetRoleId() == 2)
                    {
                        gm.actionCupidon();
                    }


                    break;
                case 12:

                    time = decode(message, size);
                    Debug.Log("time=" + time);
                    break;
                case 101:
                    sp.SetActive(false);
                    wso.SetActive(true);
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
                    break;
                case 102:
                    idP = decode(message, size);
                    usernameP = decodeString(message, size);
                    addGameInfo(idP, usernameP);
                    break;
                case 103:
                    tableSize = decode(message, size);
                    nbPlayers = new int[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        nbPlayers[i] = decode(message, size);
                    }
                    tableSize = decode(message, size);
                    gameId = new int[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        gameId[i] = decode(message, size);
                    }
                    gameName = new string[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        gameName[i] = decodeString(message, size);
                    }
                    SetCurrentGame(nbPlayers, gameId, gameName);
                    break;
                case 104:
                    if (decodeBool(message, size))
                    {
                    }
                    else
                    {
                    }
                    decode(message, size);
                    break;


                case 105:
                    Debug.Log("hey");
                    if (decodeBool(message, size))
                    {
                        id = decode(message, size);
                        username = decodeString(message, size);
                        join(client, 0, id, username);

                    }
                    else
                    {
                        decode(message, size);
                        decodeString(message, size);
                    }

                    break;
                case 110:
                    win = decode(message, size);
                    dataSize = decode(message, size);
                    idPlayers = new int[dataSize];
                    for (int i = 0; i < dataSize; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    dataSize = decode(message, size);
                    roles = new int[dataSize];
                    for (int i = 0; i < dataSize; i++)
                    {
                        roles[i] = decode(message, size);
                    }
                    if (win == 1)
                    {
                        gm.isVillageWin = true;
                    }
                    else if (win == 2)
                    {
                        gm.isVillageWin = false;
                    }
                    gm.gameover = true;

                    break;

                default:
                    Debug.Log("problem message");
                    break;



            }

            Debug.Log("message = " + message[0] + "and " + size[0] + " == " + message.Length);
            if (message.Length == size[0])
            {
                read = false;
            }

        }
        rep.RemoveAt(0);

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

    public static int ChooseLovers(Socket server, int id, int id1, int id2)
    {
        byte[] message = new byte[1 + 3 * sizeof(int)];
        message[0] = 6;
        int[] index = new int[1] { 1 };
        encode(message, id, index);
        encode(message, id1, index);
        encode(message, id2, index);
        return SendMessageToServer(server, message);
    }


}
