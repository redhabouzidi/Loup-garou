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
    public static bool prog = false, ready, inGame;
    public static List<byte[]> rep;
    public static Socket client;
    public static int id, tour;
    public static string username;
    public static GameManager gm;
    public static GameObject sp, ho, canvas, gmo, wso, cpo, lo, gmao, sgo, ro, so, chpwe,chpass,profile,fpo;
    public static Statistiques s;
    public static rank r;
    public static SavedGames sg;
    public static WaitingScreen ws;
    public static GameManagerApp gma;
    public static WPlayer[] players;
    public static Task task;

    public static Aes aes;
    public static RSA rsa;
    public static List<int> rolews = new List<int>(), nbRole;
    public static bool connected = false;

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
    //fonction pour la demande de reinitialisation de mdp
    public static void reseau(string email)
    {
        if (!prog)
        {
            prog = true;
            try
            {
                connected=false;
                int port = 18000;
                string ia = "185.155.93.105";
                // int port = 10000;
                // string ia = "127.0.0.1";
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), port);
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(iep);
                if(!client.Connected){
                    Debug.Log("unable to connect to host");
                    return;
                }
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
        recvMessage(NetworkManager.client);
    }
    //fonction pour la demande d'inscription
    public static void reseau(string pseudo, string password, string email)
    {
        if (!prog)
        {
            prog = true;
            try
            {
                connected=false;
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
        recvMessage(client);
    }

    //fonction pour la demande de connection
    
    public static void reseau(string email, string password)
    {
        if (!prog)
        {
            prog = true;
            try
            {
                connected=false;
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
        //on vide la sockeet de reception
        while (client.Available != 0)
        {
            client.Receive(new byte[4086]);
        }
        //on envoie une demande de connexion
        login(email, password);

        //tant que le programme tourne on recoit les messages
        while (prog)
        {
            recvMessage(client);
        }

        client.Close();
    }
    //permet d'interpreter les messages reçu
    public static void listener()
    {
        while (rep != null && rep.Count != 0)
        {
            //on inteprete le message
                treatMessage(rep[0]);
            


        }
    }
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    

    //fonction qui set les parametres du waiting screen
    public static void setGameInfo(string name, int[] idPlayers, string[] playerNames,bool[] ready)
    {
        ws.name = name;
        players = new WPlayer[idPlayers.Length];
        for (int i = 0; i < idPlayers.Length; i++)
        {
            if(idPlayers[i]==id){
                players[i] = new WPlayer(playerNames[i], idPlayers[i],false);
            }else{
                players[i] = new WPlayer(playerNames[i], idPlayers[i],ready[i]);
            }
        }
    }
    //fonction qui ajoute un joueur au waiting screen
    public static void addGameInfo(int id, string username,bool ready)
    {
        ws.addplayer(username, id,ready);
    }
    //fonction qui remplis le tableau de parties
    public static void SetCurrentGame(int[] nbPlayers, int[] gameId, string[] name,int[] actualPlayers,List<int[]> roles)
    {
        int id=gma.GetIdToggleGameOn();
        foreach (Transform child in gma.containerGame.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        GameManagerApp.listGame.Clear();
        for (int i = 0; i < nbPlayers.Length; i++)
        {
            gma.AddGame(gameId[i], name[i], nbPlayers[i],actualPlayers[i],roles[i]);
        }
        if(nbPlayers.Length == 0){
            gma.setRightToggleSelector(-1);
        }
        gma.setRightToggleSelector(id);
    }
    //dans les fonction encode et decode le size[] represente un tableau d'une seule valeur qui represente l'index dans le tableau de byte a partir de lequel il faut mettre la valeur il sera ensuite incrementé poue ne pas se perdre par la suite
        //fonction qui met des bytes en entier
    public static int decode(byte[] message, int[] size)
    {
        int result = BitConverter.ToInt32(message, size[0]);
        size[0] += sizeof(int);
        return result;
    }
    //fonction qui mets des bytes en double
    public static double decodeDouble(byte[] message, int[] size)
    {
        double result = BitConverter.ToDouble(message, size[0]);
        size[0] += sizeof(double);
        return result;
    }
    //fonction qui mets des bytes en bool

    public static bool decodeBool(byte[] message, int[] size)
    {
        bool result = BitConverter.ToBoolean(message, size[0]);
        size[0] += sizeof(bool);
        return result;
    }
    //fonction qui mets des bytes en string

    public static string decodeString(byte[] message, int[] size)
    {
        int dataSize = decode(message, size);
        Debug.Log("data =" + dataSize);
        string name = Encoding.ASCII.GetString(message, size[0], dataSize);
        size[0] += dataSize;

        return name;
    }
    //fonction qui mets des bytes en datetime

    public static DateTime decodeDate(byte[] message, int[] size)
    {
        DateTime date = DateTime.FromBinary(BitConverter.ToInt64(message, size[0]));
        size[0] += sizeof(long);
        return date;
    }

    //dans les fonction encode et decode le size[] represente un tableau d'une seule valeur qui represente l'index dans le tableau de byte a partir de lequel il faut mettre la valeur il sera ensuite incrementé poue ne pas se perdre par la suite
        //fonction qui met des entier en byte
    public static void encode(byte[] message, int val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(int));
        size[0] += sizeof(int);
        //fonction qui met des double en byte
    }
    public static void encode(byte[] message, double val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(double));
        size[0] += sizeof(int);
    }
        //fonction qui met des bool en byte

    public static void encode(byte[] message, bool val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(bool));
        size[0] += sizeof(bool);
    }
        //fonction qui met des string en byte

    public static void encode(byte[] message, string val, int[] size)
    {
        encode(message, val.Length, size);
        Console.WriteLine(size[0]);
        Array.Copy(Encoding.ASCII.GetBytes(val), 0, message, size[0], val.Length);
        size[0] += val.Length;
    }
        //fonction qui met des dateTime en byte

    public static void encode(byte[] message, DateTime val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val.Ticks), 0, message, size[0], sizeof(long));
        size[0] += sizeof(long);
    }

    //parametrage de la socket serveur
    public static Socket LinkToServer()
    {
        var ia = "185.155.93.105";
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ia), 10000);
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(iep);
        return client;
    }
    //envoie une demande de connexion
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
    //envoie une demande de deconnexion
    public static int logout(Socket server)
    {
        try
        {
            byte[] message = new byte[1] { 100 };
            SendMessageToServer(server, message);
            return 0;
        }
        catch (SocketException)
        {
            return -1;
        }
    }
    //envoie une demande de quitter le lobby
    public static void sendQuitLobbyMessage()
    {
        byte[] message = new byte[] { 106 };
        SendMessageToServer(client, message);
    }

    //fonction d'écoute du serveur 
    public static void recvMessage(Socket server)
    {

        byte[] message = new byte[5000];
        int recvSize;
        bool read = server.Poll(50000,SelectMode.SelectRead);   
        try{
            //on recoit le message
            recvSize = server.Receive(message);
            if(recvSize <=0){
                throw new SocketException();
            }
            Debug.Log("recv =" + message[0]);
            //on decrypte le message
            Crypto.DecryptMessage(message, aes, recvSize);
        }catch(SocketException e){
            //si une erreur survient on ferme la socket ( usuellement la deconnexion du serveur )
            Debug.Log(e.ToString());
            prog = false;
            rep.Add(new byte[1] { 100 });
            return;
        }
        
        return;

    }
    //envoie le status prêt
    public static void sendReady()
    {
        int[] size = new int[1] { 1 };
        int byteSize = 1;
        byte[] message = new byte[byteSize];
        //ajouter le code du packet
        message[0] = 108;
        SendMessageToServer(client, message);
    }
    //permet de traitre les messages
    public static void treatMessage(byte[] message)
    {
        Debug.Log(message == null);
        Dictionary<int, int> dictJoueur;
        int[] idPlayers, ids, roles, nbPlayers, gameId, nbPartie;
        double[] winrates;
        string[] playerNames, gameName;
        int dataSize, tableSize, idPlayer, idp, val, role, idP, win;
        string name, usernameP;
        int[] size = new int[1] { 1 };
        Debug.Log(BitConverter.ToString(message));
        Debug.Log("code== "+message[0]);
        rep.RemoveAt(0);


        switch (message[0])
        {
            //chat 
            case 0:
                Debug.Log("I'm here");
                gm.SendMessageToChat(decodeString(message, size), Message.MsgType.player);
                Debug.Log("I am over here");
                break;
                //vote
            case 1:
                int vote = decode(message, size);
                int voted = decode(message, size);
                int indice = gm.chercheIndiceJoueurId(vote);
                gm.listPlayer[indice].SetVote(voted);
                gm.UpdateVote();
                break;
                //status du jeu
            case 5:
                GameManager.turn = 1;
                bool day = decodeBool(message, size);
                if (day)
                {
                    GameManager.tour++;
                    Debug.Log(GameManager.tour);

                }
                GameManager.isNight = !day;
                
                break;
                //definit les amoureux
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
                //fonction de revelation pour la voyante
            case 7:
                idPlayer = decode(message, size);
                role = decode(message, size);
                gm.updateImage(idPlayer, role);
                gm.affiche_text_role(idPlayer, role);

                break;
                //tour de la sorciere pour recussiter
            case 8:
                gm.GO_tourRoles.SetActive(false);
                idPlayer = decode(message, size);
                gm.ActionSorciere(idPlayer);

                break;
                //debut de la partie
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
                        
                        if(ws.players_waiting!=null){
                            players=new WPlayer[ws.players_waiting.Count];
                            ws.players_waiting.CopyTo(players);
                        }
                    
                    for (int i = 0; i < val; i++)
                    {
                        players[i].SetRole(dictJoueur[players[i].GetId()]);
                    }
                    inGame = true;
                    LoadScene("game_scene");

                    break;
                    //mort d'un joueur
            case 10:
                 val = decode(message, size);
                    role = decode(message, size);
                    if(GameManager.newDead==null){
                        GameManager.newDead= new List<(int,int)>();
                    }
                        GameManager.newDead.Add((val,role));
                    break;
                    //fonction definissant le tour du joueur
            case 11:
                GameManager.turn = decode(message, size);
                break;
                //fonction definissant le temps restant
            case 12:

                time = decode(message, size);
                break;
                //fonction donnant le score
            case 15:
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

                for(int i = 0; i<score.Length; i++) {
                    if(gm.p.GetId() == idPlayers[i])
                        gm.afficheScore(score[i]);
                }
                //afficher le score
                break;
                //fonction permetant d'afficher le message systeme
            case 16:
                size[0] = 2;
                usernameP = decodeString(message, size);
                gm.SendMessageToChat("" + usernameP + " stands for Mayor elections !", Message.MsgType.system);
                if (usernameP == username)
                    gm.sestPresente = true;
                break;
                //fonction definissant qui doit être choisis par le maire
            case 17://
                size[0] = 1;
                tableSize = decode(message, size);
                ids = new int[tableSize];
                for (int i = 0; i < tableSize; i++)
                {
                    ids[i] = decode(message, size);
                }
                if (gm.p.GetIsMaire())
                {
                    gm.affiche_egalite(ids);
                }
                break;
                //fonction definissant qui est le maire
            case 18:
                size[0] = 1;
                idp = decode(message, size);
                if(gm==null){
                    GameManager.maire=idp;
                }else{
                if (idp == id)
                {
                    gm.p.SetIsMaire(true);
                }
                else
                {
                    gm.p.SetIsMaire(false);

                }
                if (!gm.p.GetIsAlive())
                {
                    gm.GO_dead_bg.SetActive(true);
                }
                foreach (Player j in gm.listPlayer)
                {
                    if (j.GetId() == idp)
                    {
                        j.SetIsMaire(true);
                    }
                    else
                    {
                        j.SetIsMaire(false);
                    }
                }
                gm.MiseAJourAffichage();
                }
                
                break;
                //fonction d'utilisation dobjet
                case 19:
                    int item=decode(message,size);
                    if(item==0){
                        //UTILISER POTION DE VIE
                        GameManager.useHeal=true;
                    }else if(item==1){
                        //UTILISER POTION DE MORT
                        GameManager.useKill=true;
                    }
                    break;
                    //chat loup
                case 20:
                    //MESSAGES LOUP
                    gm.SendMessageToChatLG(decodeString(message, size), Message.MsgType.loup);
                    break;
                    //fonction de deconnexion
                case 100:
                    id = -1;
                    connected = false;
                    username = "";
                    GameManagerApp.listFriend.Clear();
                    GameManagerApp.listAdd.Clear();
                    GameManagerApp.listRequest.Clear();
                    GameManagerApp.listWait.Clear();
                    LoadScene("Jeu");
                    break;
                    //fonction d'initialisation de partie
                case 101:
                    sp.SetActive(false);
                    wso.SetActive(true);
                    nbRole = new List<int>();
                    rolews = new List<int>();
                    nbplayeres = decode(message, size);
                    int nbLoup = decode(message, size);
                    bool sorciere = decodeBool(message, size);
                    bool voyante = decodeBool(message, size);
                    bool cupidon = decodeBool(message, size);
                    bool chasseur = decodeBool(message, size);
                    bool garde = decodeBool(message, size);
                    bool dictateur = decodeBool(message, size);
                    int nbVillagers = nbplayeres;
                    
                    if (nbLoup != 0)
                    {
                        rolews.Add(4);
                        nbRole.Add(nbLoup);
                        nbVillagers -= nbLoup;
                    }
                    if (sorciere)
                    {
                        rolews.Add(5);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (voyante)
                    {
                        rolews.Add(3);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (cupidon)
                    {
                        rolews.Add(2);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (chasseur)
                    {
                        rolews.Add(6);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (garde)
                    {
                        rolews.Add(7);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (dictateur)
                    {
                        rolews.Add(8);
                        nbRole.Add(1);
                        nbVillagers--;
                    }
                    if (nbVillagers >0)
                    {
                        rolews.Add(1);
                        nbRole.Add(nbVillagers);
                    }
                    name = decodeString(message, size);
                    tableSize = decode(message, size);
                    idPlayers = new int[tableSize];
                    playerNames = new string[tableSize];
                    bool[] readyState=new bool[tableSize];
                    for (int i = 0; i < tableSize; i++)
                    {
                        idPlayers[i] = decode(message, size);
                    }
                    for (int i = 0; i < tableSize; i++)
                    {
                        playerNames[i] = decodeString(message, size);

                    }
                    for(int i=0;i<tableSize;i++){
                        readyState[i]=decodeBool(message,size);
                    }
                    setGameInfo(name, idPlayers, playerNames,readyState);
                    ws.add_role(rolews.ToArray(), nbRole.ToArray());
                    ws.newGame = true;
                    break;
                    //fonction d'ajout d'un joueur
                case 102:
                    idP = decode(message, size);
                    usernameP = decodeString(message, size);
                    addGameInfo(idP, usernameP,false);
                    break;
                    //fonction qui recupere les partie disponible
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
                    int [] actualPlayers = new int[tableSize];
                    for(int i = 0;i<tableSize;i++){
                        actualPlayers[i]=decode(message,size);
                    }
                    List<int[]> rolesJoueurs = new List<int[]>();
                    for(int i=0;i<tableSize;i++){
                        dataSize=decode(message,size);
                        int[] rolesTemp = new int[dataSize];
                        for(int j=0;j<dataSize;j++){
                            rolesTemp[j]=decode(message,size);
                        }
                        rolesJoueurs.Add(rolesTemp);
                    }
                    lo.SetActive(true);
                    SetCurrentGame(nbPlayers, gameId, gameName,actualPlayers,rolesJoueurs);
                    break;
                    //fonction d'inscription
                case 104:
                if (decodeBool(message, size) == false)
                {
                    int error=decode(message,size);
                    Debug.Log(error);
                    switch(error){
                        case 1:
                            gma.AfficheError("Mot de passe incorrecte");
                        break;
                        case 2:
                            gma.AfficheError("Erreur serveur");
                        break;
                        case 3:
                            gma.AfficheError("Email invalide");
                        break;
                        case 4:
                            gma.AfficheError("Nom d'utilisateur déjà existant");
                        break;
                        case 5:
                            gma.AfficheError("Nom d'utilisateur invalide");

                        break;
                    }
                    
                }
                else
                {
                    gma.loginPage.SetActive(true);
                    gma.registrationPage.SetActive(false);
                }
                break;
                //fonction de connexion
            case 105:
                Debug.Log("hey");
                if (decodeBool(message, size))
                {
                    connected = true;
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
                        gma.addFriend(names[j], friends[j], status[j]);
                    }
                    j++;
                    for (; j < friends.Length; j++)
                    {
                        if (friends[j] == -1)
                        {
                            break;
                        }
                        Debug.Log("id = " + j + " real id = " + friends[j] + " name = " + names[j] + " status = " + status[j]);
                        gma.addFriendWait(names[j], friends[j]);
                    }
                    j++;
                    for (; j < friends.Length; j++)
                    {
                        if (friends[j] == -1)
                        {
                            break;
                        }
                        Debug.Log("id = " + j + " real id = " + friends[j] + " name = " + names[j] + " status = " + status[j]);
                        gma.addFriendRequest(names[j], friends[j]);
                    }
                    gma.AfficheNoObject();
                }
                break;
                //quand un joueur quitte le lobby on le supprime
            case 106:
                int idQuitter = decode(message, size);
                ws.quitplayer(idQuitter);
                if (idQuitter == id)
                {
                    wso.SetActive(false);
                    ho.SetActive(true);
                }

                break;
                //changement de status d'un joueur
            case 107:
                idPlayer = decode(message, size);
                int idStatus = decode(message, size);

                //CHANGER LE STATUS DU JOUEUR
                gma.UpdateStatusFriend(idPlayer, idStatus);
                break;
                //changement de ready d'un joueur
            case 108:
                idPlayer = decode(message, size);
                ready = decodeBool(message, size);
                ws.ChangeReady(idPlayer,ready);

                //METTRE LE JOUEUR A PRET 
                break;
                //envoie de fin de partie
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

                for (int i = 0; i < idPlayers.Length; i++)
                {
                    Debug.Log(idPlayers[i]);
                    if (gm.listPlayer[i].GetId() == idPlayers[i])
                    {
                        gm.listPlayer[i].SetRole(roles[i]);
                    }
                    else
                    {

                        Player p = gm.listPlayer.Find(j => j.GetId() == idPlayers[i]);
                        p.SetRole(roles[i]);

                    }

                }
                gm.isVillageWin = win;
                gm.gameover = true;

                break;
                //ajout d'un joueur en ami
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
                    gma.addFriendRequest(pseudo, idSender);
                }
                else
                {
                    Debug.Log("je ne suis pas sens� recevoir �a ");
                }
                //NOUVELLE DEMANDE D'AMIS
                break;
                //suppression d'un amis
            case 154:
                answer = decodeBool(message, size);
                idSender = decode(message, size);
                idFriend = decode(message, size);
                if (idSender == id)
                {
                    //supprimer idFriend
                    gma.SupprimerAmi(idFriend);
                }
                else
                    if (idFriend == id)
                {
                    //supprimer idSender
                    gma.SupprimerAmi(idSender);
                }
                else
                {

                }
                //SUPPRESSION D'UN AMIS
                break;
                //réponse d'un amis
            case 155:
                answer = decodeBool(message, size);
                idSender = decode(message, size);
                idFriend = decode(message, size);
                if (idSender == id)
                {
                    //Je susi celui qui a répondu
                    gma.ReponseAmi(idFriend, answer);
                }
                else
                if (idFriend == id)
                {
                    //Je suis ceuli a qui on a répondu
                    gma.ReponseAmi(idSender, answer);
                }
                else
                {
                    Debug.Log("je ne suis pas sense recevoir ca " + id);
                }
                //REPONSE DEMANDE D'AMIS
                break;
                //changement de mdp
            case 156:
                if (!decodeBool(message, size))
                {
                    gma.AfficheError("Il y a eu une erreur veuiller reesayer");
                }
                else
                {

                }
                break;
                //reinitialisation du mdp
            case 157:
                if (decodeBool(message, size) == false)
                {
                    gma.AfficheError("Il y a eu une erreur veuiller reesayer ((((((((((((((((((((()))))))))))))))))))))");
                }
                else
                {
                    if (!connected)
                    {
                        Debug.Log("on a réinitialiser avec succes");
                        chpwe.SetActive(false);
                        cpo.SetActive(true);
                        Debug.Log("on a réinitialiser avec succes 2");
                    }
                    else
                    {
                        chpass.SetActive(false);
                        profile.SetActive(true);
                    }
                }
                break;
                //recherche de joueurs
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
                    gma.AfficheNoObject();
                }
                else
                {
                    Debug.Log("shouldn't happen");
                }
                break;
                //demande de l'historique
            case 160:
                //montre l'historique de quelqu'un
                tableSize = decode(message, size);
                ids = new int[tableSize];
                for (int i = 0; i < tableSize; i++)
                {
                    ids[i] = decode(message, size);
                    Debug.Log(ids[i]);
                }
                tableSize = decode(message, size);
                string[] namesPartie = new string[tableSize];
                for (int i = 0; i < namesPartie.Length; i++)
                {
                    namesPartie[i] = decodeString(message, size);
                    Debug.Log(namesPartie[i]);
                }
                tableSize = decode(message, size);
                DateTime[] dates = new DateTime[tableSize];
                for (int i = 0; i < dates.Length; i++)
                {
                    dates[i] = decodeDate(message, size);
                }
                tableSize = decode(message, size);
                score = new int[tableSize];
                for (int i = 0; i < score.Length; i++)
                {
                    score[i] = decode(message, size);
                }
                sg.actualize(namesPartie, ids, dates, score);
                break;
                //demande d'information d'une partie
            case 161:

                string action = decodeString(message, size);
                Debug.Log(action);
                sg.refresh_string(action);
                sg.show_history();
                break;
                //demande de statistiques
            case 162:
                tableSize = decode(message, size);
                score = new int[tableSize];
                for (int i = 0; i < score.Length; i++)
                {
                    score[i] = decode(message, size);
                }
                tableSize = decode(message, size);
                playerNames = new string[tableSize];
                for (int i = 0; i < playerNames.Length; i++)
                {
                    playerNames[i] = decodeString(message, size);
                    Debug.Log("nom = " + playerNames[i]);
                }
                r.refresh_fields(playerNames, score);

                break;
                //demande de rank
            case 163:
                tableSize = decode(message, size);
                playerNames = new string[tableSize];
                for (int i = 0; i < playerNames.Length; i++)
                {
                    playerNames[i] = decodeString(message, size);
                }
                tableSize = decode(message, size);
                ids = new int[tableSize];
                for (int i = 0; i < ids.Length; i++)
                {
                    ids[i] = decode(message, size);
                }
                tableSize = decode(message, size);
                score = new int[tableSize];
                for (int i = 0; i < score.Length; i++)
                {
                    score[i] = decode(message, size);
                }
                tableSize = decode(message, size);
                nbPartie = new int[tableSize];
                for (int i = 0; i < nbPartie.Length; i++)
                {
                    nbPartie[i] = decode(message, size);
                }
                tableSize = decode(message, size);
                winrates = new double[tableSize];
                for (int i = 0; i < winrates.Length; i++)
                {
                    winrates[i] = decodeDouble(message, size);
                    Debug.Log(winrates[i]);
                }
                s.refresh_fields_stat(playerNames, nbPartie, score, winrates);
                break;
                //messages d'erreurs
            case 255:
                //faire les cas d'erreurs
                switch(message[1]){
                    case 3:
                        switch(message[2]){
                        case 0:
                            gma.AfficheError("pas assez de joueur demandé dans la partie");
                        break;
                        case 1:
                            gma.AfficheError("Le joueur est déjà en jeu");
                        break;
                        

                    }
                    break;
                    case 4:
                    switch(message[2]){
                        case 0:
                            gma.AfficheError("la game est inexistante");
                        break;
                        case 1:
                            gma.AfficheError("Le joueur n'est pas connecté");
                        break;
                        case 2:
                            gma.AfficheError("la partie est complete");
                        break;
                        case 3:
                            gma.AfficheError("la partie est déjà lancé");
                        break;
                        

                    }
                    break;
                    case 105:
                    switch(message[2]){
                        case 0:
                            gma.AfficheError("vous êtes déjà connecté a un compte");
                        break;
                        case 1:
                            gma.AfficheError("le compte au quel vous essayez d'acceder est en ligne");
                        break;
                        case 2:
                            gma.AfficheError("Pseudo ou mot de passe invalide");
                        break;
                    }
                    break;
                }
                break;
            default:
                Debug.Log("problem message");
                break;



        }



    }
    //envoie un message au serveur
    public static int SendMessageToServer(Socket server, byte[] message)
    {
        Debug.Log(BitConverter.ToString(message));
        byte[] msg = Crypto.EncryptMessage(message, aes);
        Debug.Log("message crypté");
        Debug.Log("msgsize=" + message.Length);
        return server.Send(msg, msg.Length, SocketFlags.None);
    }
    //envoie une demande d'inscription
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
    //envoie une demande de parties
    public static void sendRequestGames()
    {
        Debug.Log(((IPEndPoint)(NetworkManager.client.RemoteEndPoint)).Address.ToString());

        byte[] message = new byte[1];
        message[0] = 103;
        SendMessageToServer(client, message);
    }
    //permet de voter
    public static int Vote(int idUser, int idVote)
    {
        if (idVote == 0)
        {
            Debug.Log("IL A DIT NON");
        }
        byte[] message = new byte[1 + 2 * sizeof(int)];
        message[0] = 1;
        int[] size = new int[1] { 1 };
        encode(message, idUser, size);
        encode(message, idVote, size);

        return SendMessageToServer(client, message);
    }
    //envoie une demande de rank
    public static void sendRankRequest()
    {
        byte[] message = new byte[1 + sizeof(int)];
        message[0] = 162;
        int[] size = new int[1] { 1 };
        encode(message, id, size);
        SendMessageToServer(client, message);
    }
    //envoie une demande de statistiques
    public static void sendStatRequest(int trier)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        message[0] = 163;
        int[] size = new int[1] { 1 };
        encode(message, id, size);
        encode(message, trier, size);
        SendMessageToServer(client, message);
    }
    //envoie une demande d'historique
    public static void sendHistoryRequest()
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        message[0] = 160;
        int[] size = new int[1] { 1 };
        Debug.Log(id);
        encode(message, id, size);
        encode(message, id, size);

        SendMessageToServer(client, message);
    }
    //envoie une demande d'affichage de partie
    public static void sendMatchRequest(int idMatch)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)+sizeof(bool)];
        message[0] = 161;
        int[] size = new int[1] { 1 };
        encode(message, id, size);
        encode(message, idMatch, size);
        encode(message,Traduction.fr,size);
        SendMessageToServer(client, message);
    }
    //envoie une demande de kick
    public static int sendStartKickVote(int idPlayer, int voted)
    {

        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 200;
        encode(message, idPlayer, size);
        encode(message, voted, size);

        return SendMessageToServer(client, message);
    }
    //envoie un vote pour le kick
    public static int sendKickVote(int idPlayer, int voted)
    {
        byte[] message = new byte[1 + 2 * sizeof(int)];
        int[] size = new int[1] { 1 };
        message[0] = 201;
        encode(message, idPlayer, size);
        encode(message, voted, size);
        return SendMessageToServer(client, message);
    }
    //envoie un message dans le chat
    public static int sendchatMessage(string message)
    {
        byte[] msg = new byte[1 + sizeof(int) + message.Length];
        msg[0] = 0;
        int[] size = new int[1] { 1 };
        encode(msg, message, size);
        SendMessageToServer(client, msg);
        return 0;
    }
    //envoie un message dans le chat loup
    public static int sendchatLGMessage(string message)
    {
        byte[] msg = new byte[1 + sizeof(int) + message.Length];
        msg[0] = 20;
        int[] size = new int[1] { 1 };
        encode(msg, message, size);
        SendMessageToServer(client, msg);
        return 0;
    }
    //envoie une demande de presentation de maire
    public static void sendMayorPresentation()
    {
        byte[] message = new byte[1 + 1 + sizeof(int)];
        int[] size = new int[1] { 2 };
        message[0] = 16;
        message[1] = 0;
        encode(message, id, size);

        SendMessageToServer(client, message);
    }
    //envoie une demande de creation de game
    public static int createGame(string username, string name, int nbPlayers, int nbLoups, bool sorciere, bool voyante, bool cupidon, bool hunter, bool guardian, bool dictator)
    {
        byte[] message = new byte[1 + sizeof(int) * 4 + sizeof(bool) * 6 + username.Length + name.Length];
        int[] size = new int[1] { 1 };
        message[0] = 3;
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
    //envoie une demande de join
    public static int join(int gameId)
    {
        byte[] message = new byte[1 + sizeof(int)];
        int[] size = new int[1] { 1 };
        message[0] = 4;
        encode(message, gameId, size);

        return SendMessageToServer(client, message);
    }
    //envoie une demande de join un amis
    public static int joinFriend(int friendId)
    {
        byte[] message = new byte[1 + sizeof(int)];
        int[] size = new int[1] { 1 };
        message[0] = 2;
        encode(message, friendId, size);

        return SendMessageToServer(client, message);
    }
    //envoie une demande d'ajout d'un ami
    public static int ajoutAmi(int idUser, int id)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 153;
        encode(message, idUser, size);
        encode(message, id, size);

        return SendMessageToServer(client, message);
    }
    //envoie une demande de suppression d'ami
    public static int supprimerAmi(int idUser, int id)
    {
        byte[] message = new byte[1 + sizeof(int) * 2];
        int[] size = new int[1] { 1 };
        message[0] = 154;
        encode(message, idUser, size);
        encode(message, id, size);

        return SendMessageToServer(client, message);
    }
    //envoie une réponse pour un ami
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
    //envoie le choix des amoureux
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
    //envoie une demande de reinitialisation
    public static int ResetPasswReq(string email)
    {
        byte[] message = new byte[1 + sizeof(int) + email.Length];
        message[0] = 156;
        int[] index = new int[1] { 1 };
        encode(message, email, index);
        return SendMessageToServer(client, message);
    }
    //envoie une demande de changement de mdp
    public static int ResetPassw(string pseudo, string oldPassw, string newPassw)
    {
        byte[] message = new byte[1 + 3 * sizeof(int) + pseudo.Length + newPassw.Length + oldPassw.Length];
        message[0] = 157;
        int[] index = new int[1] { 1 };
        encode(message, pseudo, index);
        encode(message, oldPassw, index);
        encode(message, newPassw, index);
        return SendMessageToServer(client, message);
    }
    //envoie une deamnde de recherche de joueurs
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
