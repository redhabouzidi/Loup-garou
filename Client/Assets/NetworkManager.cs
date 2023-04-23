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
using System.Security.Cryptography;
public class NetworkManager : MonoBehaviour
{

    public static int nbplayeres, time;
    public static bool prog = false;
    public static List<byte[]> rep;
    public static Socket client;
    public static int id, tour;
    public static string username;
    public static GameManager gm;
    public static GameObject sp, ho, canvas, gmo, wso, cpo, lo, gmao;
    public static WaitingScreen ws;
    public static GameManagerApp gma;
    public static WPlayer[] players;
    public static Task task;

    public static Aes aes;
    public static RSA rsa;
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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void reseau(string email)
    {
        if (!prog)
        {
            prog = true;
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
                Debug.Log("socket created");
                rsa = Crypto.RecvCertificate(client);
                Debug.Log("RSA received");
                aes = Crypto.SendAes(client, rsa);
                Debug.Log("AES sent");

            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                prog = false;
            }
        }

        ResetPasswReq(email);
    }

    public static void reseau(string pseudo, string password, string email)
    {
        if (!prog)
        {
            prog = true;
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
                Debug.Log("socket created");
                rsa = Crypto.RecvCertificate(client);
                Debug.Log("RSA received");
                aes = Crypto.SendAes(client, rsa);
                Debug.Log("AES sent");
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                // Console.Write(e.Message);
                prog = false;
            }
        }
        sendInscription(pseudo, password, email);
    }


    public static void reseau(string email, string password)
    {
        if (!prog)
        {

            prog = true;
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
                rsa = Crypto.RecvCertificate(client);
                Debug.Log("RSA received");
                aes = Crypto.SendAes(client, rsa);
                Debug.Log("AES sent");
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Console.Write(e.Message);
                prog = false;
            }
        }
        login(email, password);


        while (prog)
        {
            recvMessage(client);
        }
    }
    public static void listener()
    {
        while (rep != null && rep.Count != 0)
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
        foreach (Transform child in gma.containerGame.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GameManagerApp.listGame.Clear();
        for (int i = 0; i < nbPlayers.Length; i++)
        {
            gma.AddGame(gameId[i], name[i], nbPlayers[i]);
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

    public static int login(string username, string password)
    {
        int userSize = username.Length;
        int pwSize = password.Length;
        int msgSize = 1 + 2 * sizeof(int) + userSize + pwSize;
        byte[] message = new byte[msgSize];

        message[0] = 105;
        int[] size = new int[1] { 1 };
        encode(message, username, size);
        encode(message, password, size);

        return SendMessageToServer(client, message);
    }

    public static int logout(Socket server)
    {
        try
        {
            byte[] message = new byte[1] { 100 };
            server.Send(message, 1, SocketFlags.None);
            rep.Add(new byte[1] { 100 });
            return 0;
        }
        catch (SocketException)
        {
            return -1;
        }
    }
    public static void sendQuitLobbyMessage()
    {
        byte[] message = new byte[] { 106 };
        SendMessageToServer(client, message);
    }


    public static void recvMessage(Socket server)
    {

        byte[] message = new byte[5000];
        int recvSize;
        List<Socket> read = new List<Socket>();
        read.Add(server);
        Socket.Select(read, null, null, 500000);
        if (read.Count != 0)
        {
            if (server.Available == 0)
            {
                prog = false;
                rep.Add(new byte[1] { 100 });
                return;
            }

            recvSize = server.Receive(message);

            Debug.Log("recv =" + message[0]);
            byte[] newMessage = Crypto.DecryptMessage(message, aes, recvSize);
            rep.Add(newMessage);

        }
        return;

    }
    public static void sendReady()
    {
        int[] size = new int[1] { 1 };
        int byteSize = 1;
        byte[] message = new byte[byteSize];
        //ajouter le code du packet
        message[0] = 108;
        SendMessageToServer(client, message);
    }
    public static void treatMessage(byte[] message)
    {
        Debug.Log(message == null);
        Dictionary<int, int> dictJoueur;
        bool read = true;
        int[] idPlayers, ids, roles, nbPlayers, gameId;
        string[] playerNames, gameName;
        int dataSize, tableSize, idPlayer, idp, val, role, idP, win;
        string name, usernameP;
        int[] size = new int[1] { 0 };
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
                    int vote = decode(message, size);
                    int voted = decode(message, size);
                    int indice = gm.chercheIndiceJoueurId(vote);
                    gm.listPlayer[indice].SetVote(voted);
                    gm.UpdateVote();
                    break;
                case 5:
                    GameManager.turn = 1;
                    bool ra = decodeBool(message, size);
                    GameManager.isNight = !ra;
                    if (!ra)
                    {
                        GameManager.tour++;
                    }
                    break;
                case 6:
                    idPlayer = decode(message, size);
                    idp = decode(message, size);

                    gm.setAmoureux(idPlayer, id);

                    gm.lover1_id = gm.p.GetId();
                    string msg = "vous etes amoureux avec " + gm.listPlayer[gm.chercheIndiceJoueurId(idPlayer)].GetPseudo() + " et son role est ";
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
                    gm.updateImage(idPlayer, idp);
                    gm.MiseAJourAffichage();
                    break;
                case 7:
                    idPlayer = decode(message, size);
                    role = decode(message, size);
                    gm.updateImage(idPlayer, role);
                    gm.affiche_text_role(idPlayer, role);

                    break;
                case 8:
                    gm.GO_tourRoles.SetActive(false);
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
                    players = new WPlayer[ws.players_waiting.Count];
                    for (int i = 0; i < val; i++)
                    {

                        ws.players_waiting[i].SetRole(dictJoueur[ws.players_waiting[i].GetId()]);
                    }

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
                    gm.updateImage(val, role);
                    gm.LITTERALLYDIE();
                    gm.MiseAJourAffichage();
                    break;
                case 11:
                    GameManager.turn = decode(message, size);
                    break;
                case 12:

                    time = decode(message, size);
                    break;
                case 13:
                    idPlayers = new int[decode(message, size)];
                    for (int i = 0; i < idPlayers.Length; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    int[] score = new int[decode(message, size)];
                    for (int i = 0; i < score.Length; i++)
                    {
                        score[i] = decode(message, size);
                    }
                    //afficher le score
                    break;
                case 100:
                    client.Close();
                    id = -1;
                    username = "";
                    LoadScene("Jeu");
                    break;
                case 101:
                    sp.SetActive(false);
                    wso.SetActive(true);
                    nbplayeres = decode(message, size);
                    int nbLoup = decode(message, size);
                    bool sorciere = decodeBool(message, size);
                    bool voyante = decodeBool(message, size);
                    bool cupidon = decodeBool(message, size);
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
                    ws.newGame = true;
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
                    lo.SetActive(true);
                    SetCurrentGame(nbPlayers, gameId, gameName);
                    break;
                case 104:
                    if (decodeBool(message, size) == false)
                    {
                        gma.AfficheError("Inscription went wrong");
                    }
                    else
                    {
                        gma.loginPage.SetActive(true);
                        gma.registrationPage.SetActive(false);
                    }
                    break;


                case 105:
                    Debug.Log("hey");
                    if (decodeBool(message, size))
                    {
                        id = decode(message, size);
                        username = decodeString(message, size);
                        int friendsSize = decode(message, size);
                        int[] friends = new int[friendsSize];
                        string[] names = new string[friendsSize];
                        int[] status = new int[friendsSize];
                        for (int i = 0; i < friendsSize; i++)
                        {
                            friends[i] = decode(message, size);
                        }
                        for (int i = 0; i < friendsSize; i++)
                        {
                            names[i] = decodeString(message, size);
                        }
                        for (int i = 0; i < friendsSize; i++)
                        {
                            status[i] = decode(message, size);
                        }
                        cpo.SetActive(false);
                        ho.SetActive(true);
                        int j = 0;
                        for (; j < friends.Length; j++)
                        {
                            if (friends[j] == -1)
                            {
                                break;
                            }
                            Debug.Log("id = " + j + " real id = " + friends[j] + " name = " + names[j] + " status = " + status[j]);
                            gma.addFriend(names[j], status[j], friends[j]);
                        }
                        j++;
                        for (; j < friends.Length; j++)
                        {
                            if (friends[j] == -1)
                            {
                                break;
                            }
                            gma.addFriendWait(names[j], friends[j]);
                        }
                        j++;
                        for (; j < friends.Length; j++)
                        {
                            if (friends[j] == -1)
                            {
                                break;
                            }
                            gma.addFriendRequest(names[j], friends[j]);
                        }

                    }
                    else
                    {
                        // prog = false;
                        gma.AfficheError("Error: Email/Pseudo or password is invalide");
                    }
                    break;
                case 106:
                    int idQuitter = decode(message, size);
                    if (idQuitter != id)
                    {
                        ws.quitplayer(idQuitter);
                        Debug.Log("le joueur quitte");

                    }
                    else
                    {
                        wso.SetActive(false);
                        ho.SetActive(true);
                    }

                    break;
                case 107:
                    idPlayer = decode(message, size);
                    int idStatus = decode(message, size);

                    //CHANGER LE STATUS DU JOUEUR (INFORMATION EN PLUS ????)
                    break;
                case 108:
                    idPlayer = decode(message, size);
                    bool ready = decodeBool(message, size);
                    //METTRE LE JOUEUR A PRET 
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
                    gm.isVillageWin = win;
                    gm.gameover = true;

                    break;
                case 153:

                    bool answer = decodeBool(message, size);
                    size[0] = 1;
                    answer = decodeBool(message, size);
                    int idSender = decode(message, size);
                    string pseudo = decodeString(message, size);
                    int idFriend = decode(message, size);
                    string pseudoFriend = decodeString(message, size);
                    if (id == idSender)
                    {
                        Debug.Log("je suis celui qui envoie" + idSender + " " + idFriend);
                        gma.addFriendWait(pseudoFriend, idFriend);

                    }
                    else if (id == idFriend)
                    {
                        Debug.Log("je suis celui qui recoit" + idFriend + " " + idSender);
                        gma.addFriendRequest(pseudoFriend, idFriend);
                    }
                    else
                    {
                        Debug.Log("je ne suis pas sens� recevoir �a ");
                    }
                    //NOUVELLE DEMANDE D'AMIS
                    break;
                case 154:
                    answer = decodeBool(message, size);
                    idSender = decode(message, size);
                    idFriend = decode(message, size);
                    if (idSender == id)
                    {
                        //supprimer idFriend
                    }
                    else
                        if (idFriend == id)
                    {
                        //supprimer idSender
                    }
                    else
                    {

                    }
                    //SUPPRESSION D'UN AMIS
                    break;
                case 155:
                    answer = decodeBool(message, size);
                    idSender = decode(message, size);
                    idFriend = decode(message, size);
                    if (idSender == id)
                    {
                        //Je susi celui qui a répondu
                    }
                    else
                    if (idFriend == id)
                    {
                        //Je suis ceuli a qui on a répondu
                    }
                    else
                    {
                        Debug.Log("je ne suis pas sense recevoir ca " + id);
                    }
                    //REPONSE DEMANDE D'AMIS
                    break;
                case 156:
                    if (!decodeBool(message, size))
                    {
                        gma.AfficheError("Il y a eu une erreur veuiller reesayer");
                    }
                    break;
                case 157:
                    if (decodeBool(message, size) == false)
                    {
                        gma.AfficheError("Il y a eu une erreur veuiller reesayer ((((((((((((((((((((()))))))))))))))))))))");
                    }
                    break;
                case 158:
                    int idSize = decode(message, size);
                    idPlayers = new int[idSize];
                    for (int i = 0; i < idSize; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    idSize = decode(message, size);
                    playerNames = new string[idSize];
                    for (int i = 0; i < idSize; i++)
                    {
                        playerNames[i] = decodeString(message, size);
                    }
                    if (idPlayers.Length == playerNames.Length)
                    {
                        for (int i = 0; i < idPlayers.Length; i++)
                        {
                            gma.addFriendAdd(playerNames[i], idPlayers[i]);

                        }
                    }
                    else
                    {
                        Debug.Log("shouldn't happen");
                    }
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
        byte[] msg = Crypto.EncryptMessage(message, aes);
        Debug.Log("message crypté");
        Debug.Log("msgsize=" + message.Length);
        return server.Send(msg, msg.Length, SocketFlags.None);
    }

    public static int sendInscription(string username, string password, string email)
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

        return SendMessageToServer(client, message);
    }
    public static void sendRequestGames()
    {
        Debug.Log(((IPEndPoint)(NetworkManager.client.RemoteEndPoint)).Address.ToString());

        byte[] message = new byte[1];
        message[0] = 103;
        SendMessageToServer(client, message);
    }
    public static int Vote(int idUser, int idVote)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        message[0] = 1;
        int[] size = new int[1] { 1 };
        encode(message, idUser, size);
        encode(message, idVote, size);

        return SendMessageToServer(client, message);
    }
    public static int sendStartKickVote(int idPlayer, int voted)
    {

        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 200;
        encode(message, idPlayer, size);
        encode(message, voted, size);

        return SendMessageToServer(client, message);
    }
    public static int sendKickVote(int idPlayer, int voted)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        int[] size = new int[1] { 1 };
        message[0] = 201;
        encode(message, idPlayer, size);
        encode(message, voted, size);
        return SendMessageToServer(client, message);
    }
    public static int sendFetchGameRequest()
    {
        byte[] message = new byte[1];
        message[0] = 2;

        return SendMessageToServer(client, message);
    }

    public static int sendchatMessage(string message)
    {
        byte[] msg = new byte[1 + sizeof(int) + message.Length];
        Console.WriteLine("lllllll {0}", msg.Length);
        msg[0] = 0;
        int[] size = new int[1] { 1 };
        encode(msg, message, size);
        SendMessageToServer(client, msg);
        return 0;
    }

    public static int createGame(int id, string username, string name, int nbPlayers, int nbLoups, bool sorciere, bool voyante, bool cupidon, bool hunter, bool guardian, bool dictator)
    {
        byte[] message = new byte[1 + sizeof(int) * 5 + sizeof(bool) * 6 + username.Length + name.Length];
        int[] size = new int[1] { 1 };
        message[0] = 3;
        encode(message, id, size);
        encode(message, username, size);
        encode(message, name, size);
        encode(message, nbPlayers, size);
        encode(message, nbLoups, size);
        encode(message, sorciere, size);
        encode(message, voyante, size);
        encode(message, cupidon, size);
        encode(message, hunter, size);
        encode(message, guardian, size);
        encode(message, dictator, size);

        return SendMessageToServer(client, message);
    }

    public static int join(int gameId, int id, string username)
    {
        byte[] message = new byte[1 + sizeof(int) * 3 + username.Length];
        int[] size = new int[1] { 1 };
        message[0] = 4;
        encode(message, gameId, size);
        encode(message, id, size);
        encode(message, username, size);

        return SendMessageToServer(client, message);
    }

    public static int ajoutAmi(int idUser, int id)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 153;
        encode(message, idUser, size);
        encode(message, id, size);

        return SendMessageToServer(client, message);
    }

    public static int supprimerAmi(int idUser, int id)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 154;
        encode(message, idUser, size);
        encode(message, id, size);

        return SendMessageToServer(client, message);
    }

    public static int reponseAmi(int idUser, int id, bool answer)
    {
        byte[] message = new byte[1 + sizeof(bool) + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 155;
        encode(message, answer, size);
        encode(message, idUser, size);
        encode(message, id, size);
        return SendMessageToServer(client, message);
    }

    public static int ChooseLovers(int id, int id1, int id2)
    {
        byte[] message = new byte[1 + 3 * sizeof(int)];
        message[0] = 6;
        int[] index = new int[1] { 1 };
        encode(message, id, index);
        encode(message, id1, index);
        encode(message, id2, index);
        return SendMessageToServer(client, message);
    }

    public static int ResetPasswReq(string email)
    {
        byte[] message = new byte[1 + sizeof(int) + email.Length];
        message[0] = 156;
        int[] index = new int[1] { 1 };
        encode(message, email, index);
        return SendMessageToServer(client, message);
    }
    public static int ResetPassw(string email, string oldPassw, string newPassw)
    {
        byte[] message = new byte[1 + 3 * sizeof(int) + email.Length + newPassw.Length + oldPassw.Length];
        message[0] = 157;
        int[] index = new int[1] { 1 };
        encode(message, email, index);
        encode(message, oldPassw, index);
        encode(message, newPassw, index);
        return SendMessageToServer(client, message);
    }
    public static int sendSearchRequest(int id, string pseudo)
    {
        byte[] message = new byte[1 + sizeof(int) * 2 + pseudo.Length];
        message[0] = 158;
        int[] size = new int[1] { 1 };
        encode(message, id, size);
        encode(message, pseudo, size);
        return SendMessageToServer(client, message);

    }

}
