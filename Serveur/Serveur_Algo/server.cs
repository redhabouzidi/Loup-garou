using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LGproject;
using System.Collections;
using System.Security.Cryptography;
using System.Threading.Tasks;



namespace Server
{
    //contient toutes les fonctions concernant la gestion des amis 
    public class Amis
    {
        //Socket du client
        private Socket sock;
        //Status du client
        private int status;
        //Nom du client
        private string username="";
        //Amis du client
        private List<int> friendList = new List<int>();

        public Amis(Socket sock, int status)
        {
            this.sock = sock;
            this.status = status;
        }
        //Permet d'ajouter un amis au client
        public void AddFriend(int id)
        {
            friendList.Add(id);
        }
        //Permet de supprimer un amis au client
        public void RemoveFriend(int id)
        {
            friendList.Remove(id);
        }

        public Socket GetSocket()
        {
            return sock;
        }

        public void SetSock(Socket sock)
        {
            this.sock = sock;
        }

        public int GetStatus()
        {
            return status;
        }
        //La fonction change le status du client , et envoie a tout ses amis connecté le changement de status
        public void SetStatus(int key, int status)
        {
            this.status = status;
            foreach (int i in friendList)
            {
                //Test si l'amis est connecté
                if (Messages.userData.ContainsKey(i))
                {
                    //Test pour eviter l'envoie de données a un client non connecté
                    if (Messages.userData[i].GetSocket() != null && Messages.userData[i].GetSocket().Connected)

                        Messages.sendStatus(Messages.userData[i].GetSocket(), key, status);
                }
            }
        }

        public void SetUsername(string username)
        {
            this.username = username;
        }

        public string GetUsername()
        {
            return username;
        }
    }
    //La classes contient la queue faite pour ne pas attendre la réponse de la base de donnée
    public class Queue
    {
        //La liste des Socket en attente
        public Dictionary<int, Socket> queue;
        //Le compteur de message en attente
        public int count;

        public Queue()
        {
            queue = new Dictionary<int, Socket>();
            count = 0;
        }
        //ajout un client dans la queue et incremente le compteur
        public int addVal(Socket client)
        {
            queue.Add(count, client);
            return count++;
        }

    }
    //La classe principale du serveur , Contient toutes les fonctions d'envoie et d'écoute du serveur
    public class Messages
    {
        //Sauvegarde la presence d'un joueur dans un lobby ou une game
        public static Dictionary<int, Game> players = new Dictionary<int, Game>();
        //Sauvegarde le status de connexion d'un joueur a un compte
        public static Dictionary<Socket, int> connected = new Dictionary<Socket, int>();
        //Sauvegarde la presence de parties en cours
        public static Dictionary<int, Game> games = new Dictionary<int, Game>();
        //Sauvegarde les données et la liste d'amis d'un joueur
        public static Dictionary<int, Amis> userData = new Dictionary<int, Amis>();
        //Sauvegarde les clefs d'un joueur
        public static Dictionary<Socket, Aes> client_keys = new Dictionary<Socket, Aes>();
        //Declaration de la socket bdd et d'une socket utilisé pour reveiller le main
        public static  Socket? wakeUpMain,bdd;
        //Sauvegarde les utilisateurs dont on attends la clef
        public static List<Socket> waitingKeys = new List<Socket>();
        
        //écoute principale du serveur , ne marche que aprés la connexion de la base de données
        public static void Main(string[] args)
        {
            int port = 10000;
            bool  a = true;
            //Parametrage des sockets
            Socket server = setupSocketClient(port), serverbdd = setupSocketBdd(10001) ;
 
            byte[] message = new byte[1024];
            //Attente de la connexion de la bdd
            bdd = serverbdd.Accept();

            List<Socket> list = new List<Socket> { server, bdd };
            List<Socket> fds = new List<Socket>();
            //Setup de la socket pour actualiser le main
            wakeUpMain = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(server.LocalEndPoint!=null){
            wakeUpMain.Connect(server.LocalEndPoint);
            }else{
                return;
            }
            list.Add(server.Accept());
            //Initialisation de la queue pour la bdd et la crypto
            Queue queue = new Queue();
            Crypto crypto = new Crypto();
            while (a)
            {
                //On vide la liste
                fds.Clear();
                //et on mets tous les utilisateurs actuels non connecté
                foreach (Socket fd in list)
                {
                        fds.Add(fd);
                    

                }
                //On rajoute les utilisateurs connecté
                foreach (KeyValuePair<Socket, int> input in connected)
                {
                        fds.Add(input.Key);
                }
                //On attends de recevoir des messages
                Socket.Select(fds, null, null, -1);
                
                //Si on recoit un message du serveur
                if (fds.Contains(server))
                {
                    //On rajoute le client
                    acceptConnexions(list, server, crypto);
                    fds.Remove(server);
                }
                //Si c'ets la bdd qui envoie un message
                if (fds.Contains(bdd))
                {
                    //On intercepte le message
                    try{

                    recvBddMessage(bdd, queue, list, connected);
                    }catch(SocketException e){
                        Environment.Exit(0);
                    }
                    fds.Remove(bdd);
                }
                //Si c'est le Wakeupmain qui parle on ne fait rien
                if(fds.Contains(wakeUpMain)){
                    fds.Remove(wakeUpMain);
                }
                //on écoute toutes les sockets restant comme des joueurs
                foreach (Socket fd in fds)
                {
                    //Si la socket n'est pas conneccté on deconnecte le joueur
                    if (!fd.Connected)
                    {
                        disconnectPlayer(list, fd);
                        
                    }
                    //si le joueur est dans la liste d'attente des clefs , on le rajoute dans le dictionnaire de clefs
                    else if (waitingKeys.Contains(fd))
                    {
                        client_keys.Add(fd, crypto.RecvAes(fd));
                        waitingKeys.Remove(fd);
                    }
                    else
                    {
                        //Sinon on écoute le joueur
                        try{

                        recvMessage(fd, bdd, list, connected, queue, players);
                        }catch(SocketException e){
                            disconnectPlayer(list,fd);
                            Console.WriteLine(e.ToString());
                            
                        }

                    }
                }
            }


        }
        //Fonction qui permet la deconnexion du joueur , gérant tous les cas (joueur en partie , hors partie , en lobby ect)
        public static void disconnectPlayer(List<Socket> list, Socket fd)
        {
            list.Remove(fd);
            if(client_keys.ContainsKey(fd)){
                client_keys.Remove(fd);
            }
            if(waitingKeys.Contains(fd)){
                client_keys.Remove(fd);
            }
            if (connected.ContainsKey(fd))
            {
                userData[connected[fd]].SetStatus(connected[fd], -1);
                userData.Remove(connected[fd]);
                disconnectFromLobby(fd);
                connected.Remove(fd);
            }
            
            fd.Close();
        }
        //fonction qui cree le socket serveur sur le port donné en parametre et qui fait un listen sur ce socket
        public static Socket setupSocketClient(int port)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(10);
            Console.WriteLine("Starting to listen to clients");
            return server;
        }
        //fontion qui cree le socket qui va ecouter sur la bdd
        public static Socket setupSocketBdd(int port)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, port);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(1);
            Console.WriteLine("Starting to listen to dataBase");
            return server;
        }
        //setup les Sockets utilisé pour avoir une notion de timer dans le jeu
        public static Socket setupSocketGame()
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 2000);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(100);
            return server;
        }
        //fonction qui accepte une nouvelle connexion d'un client au serveur et qui l'ajoite a la liste des client qui sont sur le serveur
        public static void acceptConnexions(List<Socket> clients, Socket server, Crypto crp)
        {

            Socket new_client = server.Accept();
            crp.SendCertificateToClient(new_client);
            clients.Add(new_client);
            waitingKeys.Add(new_client);
            return;
        }
        //Fonction d'envoie de message basique , utilise le cryptage pour crypter les données
        public static void sendMessage(Socket client, byte[] message)
        {
            //si la socket n'est pas valide on fait rien
            if(client!=null && client.Connected){
                //si le client posséde une clef on envoie un message crypté
            if (client_keys.ContainsKey(client))
            {
                Console.WriteLine("Le premier byte non crtypté est est {0}", message[0]);
                byte[] cryptedMessage = Crypto.EncryptMessage(message, client_keys[client]);
                Console.WriteLine("la taille du message crypté est de {0} et la taille normale {1}", cryptedMessage.Length, message.Length);
                Console.WriteLine("Le premier byte crypté est {0}", cryptedMessage[4]);
                Console.WriteLine("\non va envoyer un message au client ",client.RemoteEndPoint.ToString());
                Console.WriteLine(BitConverter.ToString(cryptedMessage));
                client.Send(cryptedMessage, cryptedMessage.Length, SocketFlags.None);
            }
            else
            {
                //sinon on envoie un message normal
                client.Send(message, message.Length, SocketFlags.None);
            }

            }


        }
        //Fonction d'envoie de message basique , utilise le cryptage pour crypter les données
        public static void sendMessage(Socket client, byte[] message, int recvSize)
        {
            //si la socket n'est pas valide on fait rien
            if(client!=null && client.Connected){
                //si le client posséde une clef on envoie un message crypté
            if (client_keys.ContainsKey(client))
            {
                Console.WriteLine("Le premier byte est {0}", message[0]);
                byte[] cryptedMessage = Crypto.EncryptMessage(message, client_keys[client], recvSize);
                Console.WriteLine("la taille du message crypté est de {0} et le non crypté {1}", cryptedMessage.Length, message.Length);
                Console.WriteLine("Le premier byte crypté est {0}", cryptedMessage[0]);

                client.Send(cryptedMessage, cryptedMessage.Length, SocketFlags.None);
            }
            else
            {
                //sinon on envoie un message normal
                client.Send(message, recvSize, SocketFlags.None);
            }
            }
        }
        //envoie un message basique a la bdd
        public static int sendMessagebdd(Socket bdd, byte[] message)
        {
            return bdd.Send(message, message.Length, SocketFlags.None);
        }
        //fonction qui renvoie le nombre de caractere total dans un tableau de chaine de caractere
        public static int getStringLength(string[] tab)
        {
            int size = 0;
            foreach (string s in tab)
            {
                size += s.Length;
            }
            return size;
        }

        //dans les fonction encode et decode le size[] represente un tableau d'une seule valeur qui represente l'index dans le tableau de byte a partir de lequel il faut mettre la valeur il sera ensuite incrementé poue ne pas se perdre par la suite
        //fonction qui met des bytes en entier
        public static int decodeInt(byte[] message, int[] size)
        {
            int result = BitConverter.ToInt32(message, size[0]);
            size[0] += sizeof(int);
            return result;
        }


        //fonction qui met des bytes en booleen
        public static bool decodeBool(byte[] message, int[] size)
        {
            bool result = BitConverter.ToBoolean(message, size[0]);
            size[0] += sizeof(bool);
            return result;
        }
        //fonction qui met des bytes en une chaine de caractere
        public static string decodeString(byte[] message, int[] size)
        {
            int dataSize = decodeInt(message, size);
            string name = Encoding.ASCII.GetString(message, size[0], dataSize);
            size[0] += dataSize;

            return name;
        }

        //fonction qui met un entier en bytes
        public static void encode(byte[] message, int val, int[] size)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(int));
            size[0] += sizeof(int);
        }
        //fonction qui met un booleen en bytes
        public static void encode(byte[] message, bool val, int[] size)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(bool));
            size[0] += sizeof(bool);
        }
        //fonction qui met une chaine de caracteres en bytes
        public static void encode(byte[] message, string val, int[] size)
        {
            encode(message, val.Length, size);
            Console.WriteLine(size[0]);
            Array.Copy(Encoding.ASCII.GetBytes(val), 0, message, size[0], val.Length);
            size[0] += val.Length;

        }
        //fonction permettant a charger et envoyer les bon message pour un joueur qui rejoin une game
        public static void joinGame(Socket client, int gameId, int idj)
        {
            //si la game existe
            if (games.ContainsKey(gameId))
            {
                //si le joueur est connecté
                if (connected.ContainsKey(client))
                {
                    //si le joueur n'est pas en jeu
                    if (!players.ContainsKey(connected[client]))
                    {
                        //si la partie n'est pas pleine
                        if (games[gameId].GetJoueurManquant() != 0)
                        {
                            //on fait rejoindre le joueur
                            games[gameId].Join(new Client(idj, client, userData[idj].GetUsername()));
                            players[idj] = games[gameId];
                            //et on mets sont status a 2
                            userData[idj].SetStatus(idj, 2);
                        }
                        else
                        {
                            //sinon on envoie un message d'erreur , game compléte
                            sendMessage(client, new byte[] { 255 ,4,2});
                        }

                    }
                    else if(players[connected[client]].GetStart())
                    {
                        //si le joueur est en jeu on lui renvoie les données
                        Console.WriteLine("already Playing");
                        Game g = players[connected[client]];
                        //recherche du joueur
                        foreach (Joueur j in g.GetJoueurs())
                        {
                            if (j.GetId() == idj)
                            {
                                Console.WriteLine("envoie d'info et mises a jjour");
                                j.SetSocket(client);
                                g.sendGameInfo(client);
                                g.sendRoles(j);
                                if(j.GetRole().GetIdRole()==5){
                                    //envoie de l'utilisation des potions si le joueur était sorciere
                                    if(((Sorciere)j.GetRole()).GetPotionKill()==0){
                                        sendUseItem(client,1);
                                    }
                                    if(((Sorciere)j.GetRole()).GetPotionSoin()==0){
                                        sendUseItem(client,0);
                                    }
                                }
                                //envoyer les information déjà connue
                            }
                        }
                        //envoie du status des autres joueurs
                        foreach (Joueur j in g.GetJoueurs())
                        {
                            if (!j.GetEnVie())
                            {
                                annonceMort(client, j.GetId(), j.GetRole().GetIdRole());
                            }
                            if(j.GetEstMaire()){
                                sendMaire(client,j.GetId());
                            }
                        }
                        //envoie du temps restant
                        TimeSpan timeSpent = DateTime.Now - g.t;
                        int timeToSend = g.currentTime- (int)timeSpent.TotalSeconds;
                        sendTime(client,timeToSend);
                        connected.Remove(client);
                        //reveille du jeu pour qu'il écoute le joueur
                        if(g.vide!=null){
                        g.vide.Send(new byte[1] { 0 });
                        }

                    }else{
                        //si la partie a déjà commencé on envoie un message d'erreur
                        sendMessage(client, new byte[] { 255,4,3 });
                    }
                }
                else
                {
                    //envoie message d'erreur , joueur non connecté
                    sendMessage(client, new byte[] { 255,4,1 });
                }




            }
            else
            {
                //envoie message erreur game inexistante
                sendMessage(client, new byte[] { 255,4,0 });
            }
        }
        //fonction qui sert a envoyer les informztion d'une game a un client
        public static void sendGameInfo(Socket client, int nbPlayers, int nbLoup, bool sorciere, bool voyante, bool cupidon, bool chasseur, bool guardien, bool dictateur, string name, int[] idPlayers, string[] playerNames,bool[] ready)
        {
            //variable d'index pour ne pas se perdre dans le tableau de bytes
            int[] size = new int[1] { 0 };
            //recuperer le nombre total de caracteres
            int pnsize = getStringLength(playerNames);
            //declarer un tableau de bytes avec le bon nombre de bytes
            byte[] message = new byte[1 + sizeof(int) + sizeof(int) + sizeof(bool) + sizeof(bool) + sizeof(bool) + sizeof(bool) + sizeof(bool) + sizeof(bool) + sizeof(int) + name.Length + sizeof(int) + idPlayers.Length * sizeof(int) + sizeof(int) * playerNames.Length + pnsize+sizeof(bool)*ready.Length];
            //mettre le code du message dans le premier byte
            message[0] = 101;
            //inrementer l'index
            size[0] += 1;
            //ajouter les parametres de la game dans le tableau de byte
            encode(message, nbPlayers, size);
            encode(message, nbLoup, size);
            encode(message, sorciere, size);
            encode(message, voyante, size);
            encode(message, cupidon, size);
            encode(message, chasseur, size);
            encode(message, guardien, size);
            encode(message, dictateur, size);
            encode(message, name, size);
            //ajouter le nombre d'elements du tableau idPlayers dans le message pour savoir combien il faut lire a la reception du packet
            encode(message, idPlayers.Length, size);
            //ajouter tous les ids de joueurs
            for (int i = 0; i < idPlayers.Length; i++)
            {
                encode(message, idPlayers[i], size);
            }

            //ajouter tous les noms des joueurs
            for (int i = 0; i < playerNames.Length; i++)
            {
                encode(message, playerNames[i], size);
            }
            for(int i=0;i<ready.Length;i++){
                encode(message, ready[i], size);
            }
            //A RAJOUTER (PARAMETRES DE LA PARTIE)
            //envoyer le packet au client
            sendMessage(client, message);
        }

        //fonction qui envoie une reponse au client qui a essayé de s'authantifier apres la reponse de la bdd
        public static void loginAnswer(Socket client, bool answer, int idPlayer, string username)
        {
            //declearer le tableau a envoyer
            byte[] message = new byte[1 + sizeof(bool) + sizeof(int) * 2 + username.Length];
            int[] size = new int[1] { 1 };
            //le code du packet
            message[0] = 105;
            //ajouer la reponse
            encode(message, answer, size);
            //ajouter l'id
            encode(message, idPlayer, size);
            //ajouter le username
            encode(message, username, size);
            //envoyer le packet au client
            sendMessage(client, message);
        }

        //fonction qui envoie les information du compte
        public static void SendAccountInfo(Socket client, int id, string username)
        {
            int sizeUsername = username.Length;
            int[] size = new int[1] { 0 };
            int byteSize = 1 + 2 * sizeof(int) + sizeUsername;
            byte[] message = new byte[byteSize];

            message[0] = 102;

            //l'index du tableau pour ne pas se perdre
            size[0] += 1;

            //changer la valeur de size
            //copier l'id
            encode(message, id, size);

            //copier d'abord la taille de la chaine de carateres username ensuit copier cette derniere
            encode(message, username, size);
            foreach (byte b in message)
                Console.WriteLine(b);
            sendMessage(client, message);
        }
        //envoie les parametres de parties aux joueurs
        public static void sendRoles(Socket client, int[] id, int[] roles)
        {
            int[] size = new int[1] { 0 };
            int byteSize = 1 + sizeof(int) * id.Length + sizeof(int) + sizeof(int) * roles.Length + sizeof(int);
            byte[] message = new byte[byteSize];

            message[0] = 9;

            size[0] += 1;

            encode(message, id.Length, size);
            foreach (int i in id)
            {
                encode(message, i, size);
            }
            encode(message, roles.Length, size);
            foreach (int i in roles)
            {
                encode(message, i, size);
            }
            sendMessage(client, message);
        }
        //envoie les games disponibles
        public static void SendCurrentGame(Socket client, int[] nbPlayers, int[] gameId, string[] name,int[] actualPlayers,List<int[]> roles)
        {
            int[] size = new int[1] { 0 };
            int pnsize = getStringLength(name);
            int listSize=0;
            foreach(int[] list in roles){
                listSize+=sizeof(int)*list.Length;
            }
            int byteSize = 1 + sizeof(int) + nbPlayers.Length * sizeof(int) + sizeof(int) + gameId.Length * sizeof(int) + sizeof(int) * name.Length + pnsize+sizeof(int)*actualPlayers.Length+sizeof(int)*roles.Count+listSize;
            byte[] message = new byte[byteSize];
            message[0] = 103;
            size[0] += 1;
            encode(message, nbPlayers.Length, size);

            for (int i = 0; i < nbPlayers.Length; i++)
            {
                Console.WriteLine("nb players = " + nbPlayers[i]);
                encode(message, nbPlayers[i], size);
            }
            encode(message, gameId.Length, size);
            for (int i = 0; i < gameId.Length; i++)
            {
                encode(message, gameId[i], size);
            }
            for (int i = 0; i < name.Length; i++)
            {
                encode(message, name[i], size);
            }
            for(int i=0;i<actualPlayers.Length;i++){
                encode(message,actualPlayers[i],size);
            }
            foreach(int[] list in roles){
                encode(message,list.Length,size);
                for(int i=0;i<list.Length;i++){
                    encode(message,list[i],size);
                }
            }
            sendMessage(client, message);

        }

        //fonction qui envoie un message au chat
        public static void sendChatMessage(List<Socket> clients, string chat)
        {
            //l'index pour ne pas se perdre
            int[] size = new int[1] { 0 };
            //recuperer la tailles totales des bytes necessaires
            int byteSize = 1 + sizeof(int) + chat.Length;
            //declarer le tableau a envoyer
            byte[] message = new byte[byteSize];
            //mettre le code du packet en premier
            message[0] = 0;
            //incrementer l'index
            size[0] += 1;
            //ajouter la taille
            encode(message, chat.Length, size);
            //ajouter le message
            encode(message, chat, size);
            //envoyer le packet a tous les clients
            foreach (Socket socket in clients)
            {
                sendMessage(socket, message);
            }
        }
        //envoie a la bdd les données de la partie
        public static void sendMatch(Socket bdd,string nom,string recit,string recit_ang,int[] ids,int[] score,bool[] victoire){
            byte [] message = new byte[1+sizeof(int)+nom.Length+sizeof(int)+recit.Length+sizeof(int)+recit_ang.Length+sizeof(int)+sizeof(int)*ids.Length+sizeof(int)+sizeof(int)*score.Length+sizeof(int)+sizeof(bool)*victoire.Length];
            int [] size = new int[1]{1};
            message[0]=159;
            Console.WriteLine("name ="+nom);
            Console.WriteLine("recit ="+recit);
            
            encode(message,nom,size);
            encode(message,recit,size);
            encode(message,recit_ang,size);
            encode(message,ids.Length,size);
            foreach(int id in ids){
                encode(message,id,size);
            }
            encode(message,score.Length,size);
            foreach(int id in score){
                encode(message,id,size);
            }
            encode(message,victoire.Length,size);
            foreach(bool id in victoire){
                encode(message,id,size);
            }
            sendMessage(bdd,message);
        }
        //envoie les utilisations d'objet
        public static void sendUseItem(Socket client,int item){
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 19;
            //ajouter l'id de l'objet
            encode(message, item, size);
            //envoyer a tous les joueurs
            sendMessage(client, message);
        }
        //focntion qui envoie aux joueurs le choix du vote d'un joueurs
        public static void sendVote(Socket client, int id, int cible)
        {
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int) + sizeof(int);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 1;
            //ajouter l'id de celui qui a voté
            encode(message, id, size);
            //ajouter le choix de vote
            encode(message, cible, size);
            //envoyer a tous les joueurs
            sendMessage(client, message);
        }
        //envoie l'information du nouveau maire
        public static void sendMaire(Socket client,int id){
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 18;
            //ajouter l'id de celui qui a voté
            encode(message, id, size);
            //envoyer a tous les joueurs
            sendMessage(client, message);
        }
        //fonctions qui envoie les roles de tous les joueurs a la fin de la partie
        public static void sendEndState(List<Socket> clients, int win, int[] idJoueur, int[] role)
        {
            int[] size = new int[1] { 0 };
            int byteSize = 1 + sizeof(int) * 2 + idJoueur.Length * sizeof(int) + sizeof(int) + role.Length * sizeof(int);
            byte[] message = new byte[byteSize];
            message[0] = 110;
            size[0] += 1;
            encode(message, win, size);
            encode(message, idJoueur.Length, size);
            for (int i = 0; i < idJoueur.Length; i++)
            {
                Console.WriteLine(idJoueur[i]);
                encode(message, idJoueur[i], size);
            }
            encode(message, role.Length, size);
            for (int i = 0; i < role.Length; i++)
            {
                encode(message, role[i], size);
            }
            foreach (Socket socket in clients)
            {
                sendMessage(socket, message);
            }
        }

        //fonction qui en envoie des donnes a la base de données dans le cas d'une connexion ou une incription et on attend la reponse de la bdd dans la queue
        public static void redirect(Socket bdd, int queueId, byte[] msg)
        {
            int dataLength = msg.Length;
            int msgSize = sizeof(int) + dataLength;
            byte[] message = new byte[msgSize];
            message[0] = msg[0];
            int[] size = new int[1] { 1 };
            encode(message, queueId, size);
            Array.Copy(msg, 1, message, size[0], dataLength - 1);
            if (bdd != null && bdd.Connected)
                sendMessagebdd(bdd, message);
        }
        //fonction qui en envoie la reponse de la base de donnée au client
        public static void redirect(Socket sock, byte[] message, int recvSize)
        {
            byte[] newMessage = new byte[recvSize - sizeof(int)];
            newMessage[0] = message[0];
            Array.Copy(message, 1 + sizeof(int), newMessage, 1, recvSize - sizeof(int) - 1);
            if (sock != null && sock.Connected)
                sendMessage(sock, newMessage);
        }
        //fonction qui interroge la base de données pour les paties disponibles
        public static void getCurrentLobbies(Socket bdd, int queueId)
        {
            byte[] message = new byte[1 + sizeof(int)];
            int[] size = new int[1] { 1 };
            message[0] = 150;
            encode(message, queueId, size);
            sendMessage(bdd, message);
        }

        //fonction qui interroge la base de données pour les info des joueurs
        public static void getClientInfo(Socket bdd, int queueId, int[] id)
        {
            int msgSize = 1 + sizeof(int) * 2 + id.Length;
            byte[] message = new byte[msgSize];
            message[0] = 151;
            int[] size = new int[1] { 1 };
            encode(message, queueId, size);
            encode(message, id.Length, size);
            for (int i = 0; i < id.Length; i++)
                encode(message, id[i], size);

            sendMessage(bdd, message);
        }

        //fonction qui met a jour les données d'un compte et qui envoie a la bdd
        public static void updateAccountData(Socket bdd, int queueId, int idUser, string username, string password, string email)
        {
            int msgSize = 1 + sizeof(int) * 5 + username.Length + password.Length + email.Length;
            byte[] message = new byte[msgSize];
            message[0] = 152;
            int[] size = new int[1] { 1 };
            encode(message, queueId, size);
            encode(message, idUser, size);
            encode(message, username, size);
            encode(message, password, size);
            encode(message, email, size);
            sendMessage(bdd, message);
        }
        //envoie le status du jour
        public static void etatGame(Socket client, bool day)
        {
            int msgSize = 1 + sizeof(bool);
            byte[] message = new byte[msgSize];
            message[0] = 5;
            int[] size = new int[1] { 1 };
            encode(message, day, size);
            sendMessage(client, message);
        }
        //informe la mort
        public static void annonceMort(Socket client, int id, int role)
        {
            int msgSize = 1 + sizeof(int) * 2;
            byte[] message = new byte[msgSize];
            message[0] = 10;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            encode(message, role, size);
            sendMessage(client, message);
        }
        //envoie une revelation de role
        public static void revelerRole(Socket client, int id, int role)
        {
            int msgSize = 1 + sizeof(int) * 2;
            byte[] message = new byte[msgSize];
            message[0] = 7;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            encode(message, role, size);
            sendMessage(client, message);
        }

        //renvoie si un joueur est connecté
        public static bool alreadyConnected(Dictionary<Socket, int> connected, int idPlayer)
        {
            foreach (KeyValuePair<Socket, int> input in connected)
            {
                if (input.Value == idPlayer)
                {
                    return true;
                }
            }
            return false;
        }
        //informe le depart d'un joueur
        public static void sendQuitMessage(Socket client, int id)
        {
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 106;
            //ajouter l'id de celui qui a voté
            encode(message, id, size);
            sendMessage(client, message);
        }
        envoie le nouveau status d'un joueur
        public static void sendStatus(Socket client, int id, int status)
        {
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int) + sizeof(int);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 107;
            //ajouter l'id 
            encode(message, id, size);
            //ajout du status
            encode(message, status, size);
            sendMessage(client, message);
        }
        //envoie l'information de preparation
        public static void sendReady(Socket client, int id, bool ready)
        {
            int[] size = new int[1] { 1 };
            int byteSize = 1 + sizeof(int) + sizeof(bool);
            byte[] message = new byte[byteSize];
            //ajouter le code du packet
            message[0] = 108;
            //ajouter l'id 
            encode(message, id, size);
            //ajout du status
            encode(message, ready, size);
            sendMessage(client, message);
        }
        //permet de deconnecter un joueur du lobby
        public static void disconnectFromLobby(Socket client)
        {
            if (players.ContainsKey(connected[client]))
            {
                if (!players[connected[client]].GetStart())
                {
                    Console.WriteLine(("on supprime bien le joueur"));
                    Game g = players[connected[client]];
                    g.RemovePlayer(client);

                    if (g._nbrJoueurs == g.GetJoueurManquant())
                    {
                        games.Remove(g.GetGameId());
                    }
                    else
                    if (games.ContainsKey(connected[client]))
                    {
                        int newId = g.GetJoueurs()[0].GetId();
                        games[newId] = g;
                        games.Remove(connected[client]);
                    }
                    players.Remove(connected[client]);
                }
            }
        }
        //envoie un message systéme
        public static void sendSystemMessage(List<Socket> clients, byte val, string username)
        {
            byte[] message = new byte[1 + 1 + sizeof(int) + username.Length];
            int[] size = new int[1] { 2 };
            message[0] = 16;
            message[1] = val;
            encode(message, username, size);
            foreach (Socket sock in clients)
            {
                sendMessage(sock, message);

            }
        }
        //fonction de reception pour les fonctionalitées secondaire en jeu
        public static void recvMessageGame(List<Socket> list, byte[] message, int receivedBytes)
        {
            int[] size = new int[1];
            int idPlayer, vote;
            switch (message[0])
            {

                case 0://chat message
                    Console.WriteLine("chat marche");
                    size[0] = 1;
                    if(receivedBytes<=257){
                    foreach (Socket s in list)
                    {
                        sendMessage(s, message, receivedBytes);
                    }

                    }
                    break;
                    //message systéme
                case 16:
                    size[0] = 2;
                    idPlayer = decodeInt(message, size);
                    sendSystemMessage(list, message[1], userData[idPlayer].GetUsername());
                    break;
                    //chat loup
                case 20:
                    Console.WriteLine("chat loup marche");
                    size[0] = 1;
                    if(receivedBytes<=257){
                    foreach (Socket s in list)
                    {
                        sendMessage(s, message, receivedBytes);
                    }
                    }
                    break;
                    //permet de kick un joueur ( non implementé)
                case 200://begin kicking
                    size[0] = 1;
                    idPlayer = decodeInt(message, size);
                    vote = decodeInt(message, size);
                    break;
                    //permet de voter pour le kick d'un joueur ( non implementé )
                case 201://votes for kick
                    size[0] = 1;
                    idPlayer = decodeInt(message, size);
                    vote = decodeInt(message, size);
                    break;
                    //message non reconnu
                default:
                    break;
            }
        }
        //reception des clients non en jeu
        public static void recvMessage(Socket client, Socket bdd, List<Socket> list, Dictionary<Socket, int> connected, Queue queue, Dictionary<int, Game> players)
        {
            int[] size = new int[1];
            int   id;
            byte[] message;
            //si le joueur n'as pas de clef
            if (!client_keys.ContainsKey(client))
            {
                //on receptionne le message
                message = new byte[2048];
                int receivedBytes = client.Receive(message);
                //si le message n'est pas valide on deconnecte le joueur
                if(receivedBytes<=0){
                    throw new SocketException();
                    return;
                }
                Console.WriteLine("le message dont la socket n'est pas dans le dictionnaire est " + message[0] + " received bytes =" + receivedBytes);
            }
            //sinon on decode sont message
            else
            {
                byte[] encryptedMessage = new byte[2048];
                int receivedBytes = client.Receive(encryptedMessage);
                if(receivedBytes<=0){
                    throw new SocketException();
                    return;
                }
                message = Crypto.DecryptMessage(encryptedMessage, client_keys[client], receivedBytes);
                receivedBytes = message.Length;
            }
            string username;
            int  idj;
            switch (message[0])
            {
                //rejoin la partie d'un amis
                case 2:
                    size[0] = 1;
                    int friendId = decodeInt(message, size);
                    if(connected.ContainsKey(client)){
                        if (players.ContainsKey(friendId))
                        {
                            Game g = players[friendId];
                            Console.WriteLine("gameid=" + g.GetGameId());
                            joinGame(client, g.GetGameId(), connected[client]);

                        }

                    }
                    break;
                    //crée une partie
                case 3:
                    size[0] = 1;

                    username = decodeString(message, size);
                    string name = decodeString(message, size);
                    int nbPlayers = decodeInt(message, size);
                    int nbLoups = decodeInt(message, size);
                    bool sorciere = decodeBool(message, size);
                    bool voyante = decodeBool(message, size);
                    bool cupidon = decodeBool(message, size);
                    bool chasseur = decodeBool(message, size);
                    bool guardien = decodeBool(message, size);
                    bool dictateur = decodeBool(message, size);
                    //si le joueur est connecté
                    if(nbPlayers <4){
                        sendMessage(client,new byte[]{255,3,0});
                    }else
                    if (connected.ContainsKey(client))
                    {
                        
                        id = connected[client];
                        //si la game existe et que le joueur n'est pas en jeu
                        if (!games.ContainsKey(id) && !players.ContainsKey(id))
                        {
                            Console.WriteLine("game created");
                            Game g = new Game(new Client(id, client, username), name, nbPlayers, nbLoups, sorciere, voyante, cupidon, chasseur, guardien, dictateur);

                            if (g._nbrJoueurs == 0)
                            {
                                Console.WriteLine("problem");
                                //gameerror message
                            }
                            else
                            {
                                games.Add(id, g);
                                players.Add(id, games[id]);
                                userData[id].SetStatus(id, 2);

                            }
                        }
                        else
                        {
                            Console.WriteLine("game not created");
                            sendMessage(client, new byte[] { 255,3,1 });
                        }

                    }
                    break;
                    //le joueur veut rejoindre une game
                case 4:
                    size[0] = 1;
                    int gameId = decodeInt(message, size);
                    Console.WriteLine("gameid=" + gameId);
                    joinGame(client, gameId, connected[client]);

                    break;
                case 100://disconnects
                    disconnectPlayer(list, client);

                    break;
                case 103://informations des lobby
                    if (connected.ContainsKey(client))
                    {
                        int[] idGames = new int[games.Count];
                        int[] nbPlayer = new int[games.Count];
                        int[] nbActualPlayer = new int[games.Count];
                        string[] gameNames = new string[games.Count];
                        List<int[]> roles = new List<int[]>();
                        int i = 0;
                        //charger toutes les information des parties dans les variables
                        foreach (KeyValuePair<int, Game> data in games)
                        {
                            if (!data.Value.GetStart())
                            {
                                idGames[i] = data.Key; ;
                                nbPlayer[i] = data.Value._nbrJoueurs;
                                gameNames[i] = data.Value.name;
                                nbActualPlayer[i] = data.Value.GetJoueurs().Count;
                                roles.Add(data.Value.GetRolesJoueurs());
                                i++;
                            }
                        }
                        int[] idGamesSend = new int[i];
                        int[] nbPlayerSend = new int[i];
                        int[] nbActualPlayerSend = new int[i];
                        string[] gameNamesSend = new string[i];
                        for (int j = 0; j < i; j++)
                        {
                            idGamesSend[j] = idGames[j]; ;
                            nbPlayerSend[j] = nbPlayer[j];
                            gameNamesSend[j] = gameNames[j];
                            nbActualPlayerSend[j] = nbActualPlayer[j];
                        }
                        //envoie
                        SendCurrentGame(client, nbPlayerSend, idGamesSend, gameNamesSend,nbActualPlayerSend,roles);
                    }

                    break;
                case 104://sign in
                         //envoyer les informations a la base de données
                    redirect(bdd, queue.addVal(client), message);
                    //faire conncté directement apres l'inscription
                    // SendAccountInfo(client, true, 1, username);

                    break;
                case 105://log in
                    if (!connected.ContainsKey(client))
                        redirect(bdd, queue.addVal(client), message);
                    else
                        sendMessage(client, new byte[] { 255,105 });

                    //check avec la base de donnees

                    //reponse au client
                    // SendAccountInfo(client, true, 1, username);
                    break;
                //le joueur quitte le lobby
                case 106:
                    if (connected.ContainsKey(client))
                        disconnectFromLobby(client);
                    else
                        Console.WriteLine("nope pas co");
                    break;
                case 108:
                    //si le joueur est connecté
                    if (connected.ContainsKey(client))
                    {
                        //si le joueur est en lobby
                        if (players.ContainsKey(connected[client]))
                        {
                            //si la partie n'as pas commencé
                            if (!players[connected[client]].GetStart())
                            {
                                idj = connected[client];
                                //on met le joueur a prêt
                                players[connected[client]].ToggleReady(idj);
                            }

                        }

                    }
                    break;
                    //demande d'amis
                case 153:
                //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    break;
                    //suppression d'amis
                case 154:
                //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    break;
                    //réponse amis
                case 155:
                //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        bool answer=decodeBool(message,size);
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    break;
                    //reinitialisation mdp
                case 156:
                    //on redirige le message vers la bdd
                    redirect(bdd, queue.addVal(client), message);
                    break;
                    //changement mdp
                case 157:
                    //on redirige le message vers la bdd
                    redirect(bdd, queue.addVal(client), message);
                    break;
                    //recherche d'amis
                case 158:
                    //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    break;
                    //demande l'historique de partie 
                case 160:
                    //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    
                    break;
                    //demande information partie
                case 161:
                    //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                    break;
                    //demande les statistiques
                case 162:
                    //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                break;
                //demande les ranks
                case 163:
                    //on redirige le message vers la bdd tant qu'il a donné le bon id et qu'il est connecté
                    size[0]=1;
                    if (connected.ContainsKey(client)){
                        id=decodeInt(message,size);
                        if(connected[client]==id){
                            redirect(bdd, queue.addVal(client), message);

                        }
                        else{
                            sendMessage(client,new byte[2]{255,message[0]});

                        }
                    }
                break;
                //recoit un message d'erreur 
                case 255:
                    return;
                default:
                    break;
            }
        }
        //envoie les personens qui doivent être choisis pour être tué
        public static void SendVictime(Socket client, int[] id)
        {
            byte[] message = new byte[1 + sizeof(int) + sizeof(int) * id.Length];
            int[] size = new int[1] { 1 };
            message[0]=17;
            encode(message,id.Length,size);
            for (int i = 0; i < id.Length; i++)
            {
                encode(message,id[i],size);
            }
            sendMessage(client,message);
        }
        //recv de la bdd
        public static void recvBddMessage(Socket bdd, Queue queue, List<Socket> list, Dictionary<Socket, int> connected)
        {
            int[] size = new int[1] { 1 };
            byte[] message = new byte[4096];
            int recvSize = bdd.Receive(message);
            if(recvSize <=0){
                throw new SocketException();
            }
            switch (message[0])
            {
                //charge l'information des joueurs 
                case 105:
                    size = new int[1] { 1 };
                    int queueId = decodeInt(message, size);
                    bool answer = decodeBool(message, size);
                    Socket client = queue.queue[queueId];
                    //si le joueur s'est bien connecté
                    if (answer)
                    {

                        int idPlayer = decodeInt(message, size);
                        string username = decodeString(message, size);
                        //si le joueur n'est pas connecté 
                        if (!connected.ContainsKey(client)&&!userData.ContainsKey(idPlayer))
                        {
                            //on initialise le dictionnaire d'amis
                            userData[idPlayer] = new Amis(client, 0);
                            userData[idPlayer].SetUsername(username);
                            //on enleve le joueur de la liste d'écoute des joueurs non connecté
                            list.Remove(client);
                            //on ajoute le joueur dans la liste des joueurs connecté
                            connected.Add(client, idPlayer);
                            //on initialise la liste d'amis
                            int friendsSize = decodeInt(message, size);
                            int[] friendList = new int[friendsSize];

                            for (int i = 0; i < friendsSize; i++)
                            {
                                friendList[i] = decodeInt(message, size);
                            }
                            for (int i = 0; i < friendsSize; i++)
                            {
                                decodeString(message, size);
                            }
                            bool friends = true;
                            foreach (int i in friendList)
                            {
                                if (i == -1)
                                {
                                    friends = false;
                                    encode(message, -1, size);
                                }
                                else
                                if (friends)
                                {
                                    userData[idPlayer].AddFriend(i);
                                    if (players.ContainsKey(i) && players[i].GetStart())
                                    {
                                        encode(message, 3, size);
                                    }
                                    else
                                    if (players.ContainsKey(i))
                                    {
                                        encode(message, 2, size);
                                    }
                                    else
                                    if (userData.ContainsKey(i))
                                    {
                                        encode(message, 1, size);
                                    }
                                    else
                                    {
                                        encode(message, -1, size);
                                    }

                                }
                                else
                                {
                                    encode(message, 0, size);
                                }
                            }
                            redirect(client, message, size[0]);
                            //si le joueur est en jeu
                            if (players.ContainsKey(idPlayer))
                            {
                                //on mes sont status a 3
                                userData[idPlayer].SetStatus(idPlayer, 3);
                                //on appelle la fonction de join
                                joinGame(client,players[idPlayer].GetGameId(),idPlayer);
                            }
                            else
                            {
                                //sinon on mets le status a 1
                                userData[idPlayer].SetStatus(idPlayer, 1);

                            }
                            


                        }else{
                            //si le joueur est déjà connecté on envoie un message d'erreur
                            sendMessage(client,new byte[] {255,105});
                        }
                        
                    }
                    else
                    {
                        size[0]=1;
                        message[0]=105;
                        encode(message,false,size);
                        sendMessage(client,message,1+sizeof(bool));
                    }
                    queue.queue.Remove(queueId);
                    
                    break;
                    //inscription du joueur
                case 104:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                    //retour demande amis
                case 153:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    answer = decodeBool(message, size);
                    int idAdd=decodeInt(message, size);
                    decodeString(message, size);
                    int idFriend = decodeInt(message, size);
                    decodeString(message, size);
                    if (userData.ContainsKey(idFriend))
                    {
                        //si l'amis est connecté on lui envoie l'information
                        redirect(userData[idFriend].GetSocket(), message, recvSize);
                    }
                    if(userData.ContainsKey(idAdd)){
                        //si le joueur est connecté on lui envoie l'information
                        redirect(userData[idAdd].GetSocket(), message, recvSize);
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);

                    break;
                    //retour suppression amis
                case 154:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    answer = decodeBool(message, size);
                    idAdd=decodeInt(message, size);
                    idFriend = decodeInt(message, size);
                    if (userData.ContainsKey(idFriend))
                    {
                        //si le joueur supprimé est connecté on lui envoie le messasge
                        userData[idFriend].RemoveFriend(idAdd);
                        redirect(userData[idFriend].GetSocket(), message, recvSize);
                    }
                    if (userData.ContainsKey(idAdd))
                    {
                        //si le joueur qui supprime est connecté on lui envoie le messasge
                        userData[idAdd].RemoveFriend(idFriend);
                        redirect(userData[idAdd].GetSocket(), message, recvSize);
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);

                    break;
                    //retour réponse demande d'amis
                case 155:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    answer = decodeBool(message, size);
                    int id = decodeInt(message, size);
                    idFriend = decodeInt(message, size);
                    if (answer)
                    {
                        if (userData.ContainsKey(id))
                        {
                            //si le joueur est connecté on lui envoie l'information
                            userData[id].AddFriend(idFriend);//et on l'ajoute dans la liste d'amis
                            redirect(userData[id].GetSocket(), message, recvSize);

                        }
                        if (userData.ContainsKey(idFriend))
                        {
                            //si l'amis est connecté on lui envoie l'information
                            userData[idFriend].AddFriend(id);//et on l'ajoute dans la liste d'amis
                            redirect(userData[idFriend].GetSocket(), message, recvSize);

                        }
                        if (userData.ContainsKey(idFriend) && userData.ContainsKey(id))
                        {
                            //si les deux sont connecté on leurs envoie le status de chaque joueurs
                            sendStatus(userData[id].GetSocket(), idFriend, userData[idFriend].GetStatus());
                            sendStatus(userData[idFriend].GetSocket(), id, userData[id].GetStatus());
                        }
                    }
                    else
                    {
                        if (userData.ContainsKey(idFriend))
                        {
                            //si l'amis est connecté on lui envoie l'information
                            redirect(userData[idFriend].GetSocket(), message, recvSize);
                        }
                        if (userData.ContainsKey(id))
                        {
                            //si le joueur est connecté on lui envoie l'information
                            redirect(userData[id].GetSocket(), message, recvSize);
                        }
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);

                    break;
                    //retour de reinitialisation de mdp
                case 156:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de changement de mdp
                case 157:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de recherche d'amis
                case 158:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de la demande d'historique
                case 160:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de la demande d'information partie
                case 161:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de la demande de statistique
                case 162:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                    //retour de la demande de rank
                case 163:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;

            }
        }
        //envoie les amoureux aux joueurs
        public static void setLovers(Socket player1, Socket player2, int id1, int id2, int role1, int role2)
        {
            byte[] message = new byte[1 + 2 * sizeof(int)];
            message[0] = 6;
            int[] index = new int[1] { 1 };
            Console.WriteLine("je suis la ");
            encode(message, id1, index);
            encode(message, role1, index);
            //envoie le message au premier amoureux
            if (player2 != null && player2.Connected)
                sendMessage(player2, message);
            index[0] = 1;
            encode(message, id2, index);
            encode(message, role2, index);
            //envoie le message au deuxieme amoureux
            if (player1 != null && player1.Connected)
                sendMessage(player1, message);
        }
        //envoie la demande de resurection de la sorciere
        public static void EnvoieInformation(Socket client, int id)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 8;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            Console.WriteLine("information envoyé a la sorciere");
            sendMessage(client, message);

        }
        //envoie le tour au joueur
        public static void sendTurn(Socket client, int roleId)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 11;
            int[] size = new int[1] { 1 };
            encode(message, roleId, size);
            sendMessage(client, message);

        }
        //envoie le temp au joueur
        public static void sendTime(Socket client, int time)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 12;
            int[] size = new int[1] { 1 };
            encode(message, time, size);
            sendMessage(client, message);


        }
        //reveille le main pour initialiser les nouvelles valeurs
        public static void WakeUpMain()
        {
            if(wakeUpMain!=null){
                wakeUpMain.Send(new byte[1] { 255 });
            }
        }
        //envoie le score de fin de partie
        public static void sendScore(Socket client, int[] id, int[] score)
        {
            byte[] message = new byte[1 + sizeof(int) * 2 + sizeof(int) * id.Length + sizeof(int) * score.Length];
            message[0] = 15;
            int[] size = new int[1] { 1 };
            encode(message, id.Length, size);
            foreach (int i in id)
            {
                encode(message, i, size);
            }
            encode(message, score.Length, size);
            foreach (int i in score)
            {
                encode(message, i, size);
            }
            sendMessage(client, message);
        }
    }

}
