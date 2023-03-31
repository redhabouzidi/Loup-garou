using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LGproject;


namespace Server
{
    public class Queue
    {
        public Dictionary<int, Socket> queue;
        public int count;
        public Queue()
        {
            queue = new Dictionary<int, Socket>();
            count = 0;
        }
        public int addVal(Socket client)
        {
            queue.Add(count, client);
            return count++;
        }

    }
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
    public class server
    {
        public static Dictionary<int, Game> games = new Dictionary<int, Game>();

        public static void Main(string[] args)
        {
            /*if (args.Length != 1)
            {
                return;
            }
            int port;
            bool answer = Int32.TryParse(args[0], out port);
            if (answer == false)
            {
                return;
            }*/
            int port = 10000;
            bool reading = true, a = true;
            Socket server = setupSocketClient(port), serverbdd = setupSocketBdd(10001), bdd;
            byte[] message = new byte[1024];
            // try
            // {
            games.Add(0, new Game());
            bdd = serverbdd.Accept();
            List<Socket> clients = new List<Socket>();
            List<Socket> list = new List<Socket> { server, bdd };
            List<Socket> fds = new List<Socket>();
            Dictionary<int, Game> players = new Dictionary<int, Game>();
            Dictionary<Socket,int> connected= new Dictionary<Socket ,int>();
            Queue queue = new Queue();
	        int i=0;
            while (a)
            {

                i++;
                fds.Clear();
                foreach (Socket fd in list)
                {
                    fds.Add(fd);
                }
                foreach (KeyValuePair<Socket,int> input in connected)
                {
                    fds.Add(input.Key);
                }

                Socket.Select(fds, null, null, -1);
                //Testing
                int[] idPlayers = new int[5] { 1, 2, 3, 4, 5 };
                int[] nbPlayers = new int[5] { 4, 5, 3, 9, 8 };
                int[] gameId = new int[5] { 1, 2, 3, 4, 5 };
                string[] name = new string[5] { "artorias", "heaven", "casablanca", "tartarus", "asgard" };
                string[] playerNames = new string[5] { "jean", "mark", "minot", "daniel", "aurore" };
                //SendCurrentGame(server, nbPlayers, gameId, name);
                //sendGameInfo(server, "rocky", idPlayers, playerNames);
                //SendAccountInfo(server, true, 5, "rocky");
                // sendChatMessage(list, "azul fellawen");
                //sendVote(list, 4);
                //sendEndState(list, idPlayers, idPlayers);

                if (fds.Count == 0)
                {
                    Console.WriteLine("nothing happened v2");
                }
                else
                if (fds.Contains(server))
                {
                    acceptConnexions(list, server);

                    clients.Add(list.Last());
                    fds.Remove(server);
                    Console.WriteLine("got a new client ! it's " + list.Last().RemoteEndPoint.ToString());
                }
                if (fds.Contains(bdd))
                {
                    recvBddMessage(bdd, queue, list, connected);
                    fds.Remove(bdd);
                }
                foreach (Socket fd in fds)
                {
                    if (fd.Available == 0)
                    {
                        list.Remove(fd);
                        clients.Remove(fd);
                        connected.Remove(fd);
                        fd.Close();

                    }
                    else
                    {
                        recvMessage(fd, bdd, list, connected, queue, players);
                    }
                }
	    }


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
        public static Socket setupSocketGame()
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 10007);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(iep);
            server.Listen(100);
            return server;
        }
        //fonction qui accepte une nouvelle connexion d'un client au serveur et qui l'ajoite a la liste des client qui sont sur le serveur
        public static void acceptConnexions(List<Socket> clients, Socket server)
        {
            clients.Add(server.Accept());
            return;
        }
        //fonction qui envoie un message a un socket donne en parametre
        public static int sendMessage(Socket client, byte[] message)
        {
		foreach (byte b in message)
		{
		Console.Write(b+" ");
		}
		Console.WriteLine("");
		
            return client.Send(message, message.Length, SocketFlags.None);

        }
	public static int sendMessage(Socket client,byte [] message , int recvSize)
	{
		foreach(byte b in message)
		{
			Console.Write(b+" ");
		}
		Console.WriteLine("");
		return client.Send(message,recvSize,SocketFlags.None);
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

        //fonction qui sert a envoyer les informztion d'une game a un client
        public static int sendGameInfo(Socket client, string name, int[] idPlayers, string[] playerNames)
        {
            //variable d'index pour ne pas se perdre dans le tableau de bytes
            int[] size = new int[1] { 0 };
            //recuperer le nombre total de caracteres
            int pnsize = getStringLength(playerNames);
            //declarer un tableau de bytes avec le bon nombre de bytes
            byte[] message = new byte[1 + sizeof(int) + name.Length + sizeof(int) + idPlayers.Length * sizeof(int) + sizeof(int) * playerNames.Length + pnsize];
            //mettre le code du message dans le premier byte
            message[0] = 101;
            //inrementer l'index
            size[0] += 1;
            //ajouter le nom de la game dans le tableau de byte
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
            //envoyer le packet au client
            return sendMessage(client, message);
        }

        //fonction qui envoie une reponse au client qui a essayé de s'authantifier apres la reponse de la bdd
        public static int loginAnswer(Socket client, bool answer, int idPlayer, string username)
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
            return sendMessage(client, message);
        }

        //fonction qui envoie les information du compte
        public static int SendAccountInfo(Socket client, int id, string username)
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
            return sendMessage(client, message);
        }
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
        //fonction qui va generer un thread qui va lancer la partie
        public static void createGame(int id, string username, string name)
        {
            Console.WriteLine($"user {id} with name {username} create the game {name}");
        }

        public static int SendCurrentGame(Socket client, int[] nbPlayers, int[] gameId, string[] name)
        {
            int[] size = new int[1] { 0 };
            int pnsize = getStringLength(name);
            int byteSize = 1 + sizeof(int) + nbPlayers.Length * sizeof(int) + sizeof(int) + gameId.Length * sizeof(int) + sizeof(int) * name.Length + pnsize;
            byte[] message = new byte[byteSize];
            message[0] = 103;
            size[0] += 1;
            encode(message, nbPlayers.Length, size);
            for (int i = 0; i < nbPlayers.Length; i++)
            {
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
            return sendMessage(client, message);

        }

        //fonction qui envoie un message au chat
        public static int sendChatMessage(List<Socket> clients, string chat)
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
            return 0;
        }

        //focntion qui envoie aux joueurs le choix du vote d'un joueurs
        public static int sendVote(List<Socket> clients, int id, int cible)
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
            foreach (Socket socket in clients)
            {
                sendMessage(socket, message);
            }
            return 0;
        }

        //fonctions qui envoie les roles de tous les joueurs a la fin de la partie
        public static int sendEndState(List<Socket> clients,int win, int[] idJoueur, int[] role)
        {
            int[] size = new int[1] { 0 };
            int byteSize = 1 + sizeof(int)*2 + idJoueur.Length * sizeof(int) + sizeof(int) + role.Length * sizeof(int);
            byte[] message = new byte[byteSize];
            message[0] = 110;
            size[0] += 1;
	    encode(message,win,size);
            encode(message, idJoueur.Length, size);
            for (int i = 0; i < idJoueur.Length; i++)
            {
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
            return 0;
        }

        //fonction qui en envoie des donnes a la base de données dans le cas d'une connexion ou une incription et on attend la reponse de la bdd dans la queue
        public static int redirect(Socket bdd, int queueId, byte[] msg)
        {
            int dataLength = msg.Length;
            int msgSize = sizeof(int) + dataLength;
            byte[] message = new byte[msgSize];
            message[0] = msg[0];
            int[] size = new int[1] { 1 };
            encode(message, queueId, size);
            Array.Copy(msg, 1, message, size[0], dataLength - 1);
            return sendMessage(bdd, message);
        }
        //fonction qui en envoie la reponse de la base de donnée au client
        public static int redirect(Socket sock, byte[] message, int recvSize)
        {
            byte[] newMessage = new byte[recvSize - sizeof(int)];
            newMessage[0] = message[0];
            Array.Copy(message, 1 + sizeof(int), newMessage, 1, recvSize - sizeof(int) - 1);
            return sendMessage(sock, newMessage);
        }
        //fonction qui interroge la base de données pour les paties disponibles
        public static int getCurrentLobbies(Socket bdd, int queueId)
        {
            byte[] message = new byte[1 + sizeof(int)];
            int[] size = new int[1] { 1 };
            message[0] = 150;
            encode(message, queueId, size);
            return sendMessage(bdd, message);
        }

        //fonction qui interroge la base de données pour les info des joueurs
        public static int getClientInfo(Socket bdd, int queueId, int[] id)
        {
            int msgSize = 1 + sizeof(int) * 2 + id.Length;
            byte[] message = new byte[msgSize];
            message[0] = 151;
            int[] size = new int[1] { 1 };
            encode(message, queueId, size);
            encode(message, id.Length, size);
            for (int i = 0; i < id.Length; i++)
                encode(message, id[i], size);

            return sendMessage(bdd, message);
        }

        //fonction qui met a jour les données d'un compte et qui envoie a la bdd
        public static int updateAccountData(Socket bdd, int queueId, int idUser, string username, string password, string email)
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
            return sendMessage(bdd, message);
        }
        public static int etatGame(Socket client, bool day)
        {
            int msgSize = 1 + sizeof(bool);
            byte[] message = new byte[msgSize];
            message[0] = 5;
            int[] size = new int[1] { 1 };
            encode(message, day, size);
            client.Send(message);
            return 0;
        }
        public static int annonceMort(Socket client, int id, int role)
        {
            int msgSize = 1 + sizeof(int) * 2;
            byte[] message = new byte[msgSize];
            message[0] = 10;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            encode(message, role, size);
            return sendMessage(client, message);
        }
        public static int revelerRole(Socket client, int id, int role)
        {
            int msgSize = 1 + sizeof(int) * 2;
            byte[] message = new byte[msgSize];
            message[0] = 7;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            encode(message, role, size);
            return sendMessage(client, message);
        }

        public static answer recvMessageGame(List<Socket> list,byte[] message , int receivedBytes)
        {
            int[] size = new int[1];
            string chat;
            int idPlayer, vote, idUser;
            switch (message[0])
            {
                case 0://chat message
        		Console.WriteLine("chat marche");	
	    		size[0] = 1;
                    foreach (Socket s in list)
                    {
                        sendMessage(s, message,receivedBytes);
                    }
                    break;
                case 1://voter
                    size[0] = 1;
                    idUser = decodeInt(message, size);
                    vote = decodeInt(message, size);
                    break;


                case 101://information de la partie

                    break;

                case 200://begin kicking
                    size[0] = 1;
                    idPlayer = decodeInt(message, size);
                    vote = decodeInt(message, size);
                    break;
                case 201://votes for kick
                    size[0] = 1;
                    idPlayer = decodeInt(message, size);
                    vote = decodeInt(message, size);
                    break;
                default:
                    break;
            }
            return new answer();
        }
        public static bool alreadyConnected(Dictionary<Socket,int> connected,int idPlayer)
        {
            foreach(KeyValuePair<Socket,int> input in connected)
            {
                if(input.Value == idPlayer)
                {
                    return true;
                }
            }
            return false;
        }
        public static answer recvMessage(Socket client, Socket bdd, List<Socket> list, Dictionary<Socket,int> connected, Queue queue,Dictionary<int,Game> players)
        {
            int[] size = new int[1];
            int dataSize, tableSize, id;

            byte[] message = new byte[2048];
            int receivedBytes = client.Receive(message);
            string username, password, chat;
            int idPlayer, vote;
            switch (message[0])
            {

                case 3:
                    size[0] = 1;

                    id = decodeInt(message, size);
                    username = decodeString(message, size);
                    string name = decodeString(message, size);

                    if (!games.ContainsKey(id))
                    {
                        Console.WriteLine("game created");
                        games.Add(id, new Game());
                    }
                    else
                    {
                        Console.WriteLine("game created");
                        sendMessage(client, new byte[] { 255 });
                    }

                    break;
                case 4:
                    size[0] = 1;
                    int gameId = decodeInt(message, size);
                    int idj = decodeInt(message, size);
                    username = decodeString(message, size);
                    Console.WriteLine("gameid=" + gameId);
                    if (games.ContainsKey(gameId))
                    {
                        if (games[gameId].GetJoueurManquant() != 0)
                        {
                            if (connected.ContainsKey(client))
                            {

                            if (!players.ContainsKey(connected[client]))
                            {
                                Console.WriteLine($"joins game {gameId}");
                                Console.WriteLine("id joueur : " + idj);
                                games[gameId].Join(new Client(idj, client, username));

                            }
                            else
                            {
                                Console.WriteLine("already Playing");
                            }
                            }
                            else
                            {
                                Console.WriteLine("Not connected");
                            }
                        }
                        else
                        {
                            sendMessage(client, new byte[] { 255 });
                        }


                    }
                    else
                    {
                        sendMessage(client, new byte[] { 255 });
                    }
                    return new answer(false, 0, 0, null);
                case 6:
                    size[0] = 1;
                    setLovers(list[3], list[4], decodeInt(message, size), decodeInt(message, size), 10, 14);
                    return new answer(false, 0, 0, null);
                case 100://disconnects
                    break;
                case 103://informations des lobby
                    if (connected.ContainsKey(client))
                    {
                        getCurrentLobbies(bdd, queue.addVal(client));
                        return new answer(false, 0, 0, null);
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
                        sendMessage(client, new byte[] { 255 });

                    //check avec la base de donnees

                    //reponse au client
                    // SendAccountInfo(client, true, 1, username);
                    break;
                case 153:
                    if (connected.ContainsKey(client))
                        redirect(bdd, queue.addVal(client), message);
                    break;
                case 154:
                    if (connected.ContainsKey(client))
                        redirect(bdd, queue.addVal(client), message);
                    break;
                case 155:
                    if (connected.ContainsKey(client))
                        redirect(bdd, queue.addVal(client), message);
                    break;
                default:
                    break;
            }
            return new answer();
        }
        public static int recvBddMessage(Socket bdd, Queue queue, List<Socket> list, Dictionary<Socket,int> connected)
        {
            int[] size = new int[1] { 1 };
            byte[] message = new byte[4096];
            if (bdd.Available == 0)
            {
                throw new SocketException();
            }

            int recvSize = bdd.Receive(message);
            switch (message[0])
            {
                case 105:
                    size = new int[1] { 1 };
                    int queueId = decodeInt(message, size);
                    bool answer = decodeBool(message, size);
                    int idPlayer = decodeInt(message, size);
                    string username = decodeString(message, size);
                    if (answer)
                    {
                        if (!alreadyConnected(connected,idPlayer))
                        {
                            list.Remove(queue.queue[queueId]);
                            connected.Add(queue.queue[queueId],idPlayer);

                        }
                    }
                    Socket client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                case 104:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                case 103:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                case 153:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                case 154:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                case 155:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
            }
            return 0;
        }

        public static int setLovers(Socket player1, Socket player2, int id1, int id2, int role1, int role2)
        {
            byte[] message = new byte[1 + 2 * sizeof(int)];
            message[0] = 6;
            int[] index = new int[1] { 1 };
            Console.WriteLine("je suis la ");
            encode(message, id1, index);

            encode(message, role1, index);
            sendMessage(player2, message);
            index[0] = 1;
            encode(message, id2, index);
            encode(message, role2, index);
            sendMessage(player1, message);
            return 0;
        }
        public static int EnvoieInformation(Socket client, int id)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 8;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
		Console.WriteLine("information envoyé a la sorciere");
            return sendMessage(client, message);
            
        }
        public static int sendTurn(Socket client, int roleId)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 11;
            int[] size = new int[1] { 1 };
            encode(message, roleId, size);
            return sendMessage(client, message);

            
        }
        public static int sendTime(Socket client, int time)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 12;
            int[] size = new int[1] { 1 };
            encode(message, time, size);
            return sendMessage(client, message);

            
        }
        // public (int, int) gameVote(List<Joueur> listJoueurs)
        // {
        //     Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
        //     List<Socket> role = new List<Socket>();
        //     List<Socket> sockets = new List<Socket>();
        //     foreach (Joueur j in listJoueurs)
        //     {
        //         sockets.Add(j.getSocket());
        //         if (j.GetRole().GetidRole() == this.GetidRole())
        //         {
        //             dictJoueur[j.getSocket()] = j;
        //             role.Add(j.getSocket());
        //         }

        //     }
        //     while (true)
        //     {
        //         Socket.Select(sockets, null, null, 5000000);
        //         foreach (Socket sock in sockets)
        //         {
        //             int[] size = new int[1] { 1 };
        //             byte[] message = new byte[4096];
        //             if (role.Contains(sock))
        //             {
        //                 if (sock.Available == 0)
        //                 {
        //                     //gestion de deconnexion ???
        //                 }
        //                 else
        //                 {
        //                     sock.Receive(message);
        //                     if (message[0] == 1)
        //                     {
        //                         int idVoter = server.decodeInt(message, size);
        //                         int idVoted = server.decodeInt(message, size);
        //                         if (idVoter == dictJoueur[sock].getId())
        //                         {
        //                             return (idVoter, idVoted);
        //                         }
        //                     }

        //                 }
        //                 while (true)
        //                 {
        //                     Socket.Select(sockets, null, null, 5000000);
        //                     foreach (Socket sock in sockets)
        //                     {
        //                         int[] size = new int[1] { 1 };
        //                         byte[] message = new byte[4096];
        //                         if (role.Contains(sock))
        //                         {
        //                             if (sock.Available == 0)
        //                             {
        //                                 //gestion de deconnexion ???
        //                             }
        //                             else
        //                             {
        //                                 sock.Receive(message);
        //                                 if (message[0] == 1)
        //                                 {
        //                                     int idVoter = server.decodeInt(message, size);
        //                                     int idVoted = server.decodeInt(message, size);
        //                                     if (idVoter == dictJoueur[sock].getId())
        //                                     {
        //                                         return (idVoter, idVoted);
        //                                     }



        //                                 }
        //                             }
        //                         }
        //                         else
        //                         {
        //                             server.recvMessageGame(sock, sockets);

        //                         }
        //                     }
        //                 }
        //                 return (-1, -1);
        //             }
        //         }
        //     }
        // }



        //TO ADD TO THE ALGORITHMIC SIDE


        /* using System.Net.Sockets;

 namespace LGproject;

     // on part du principe que la partie se lance à 6 joueurs

     public class Game
     {
         private List<Joueur> _joueurs;
         private List<Role> _roles;
         private int _nbrJoueursManquants;
         public static Socket listener = Server.server.setupSocketGame();*/






        /*Socket reveille = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reveille.Connect(Game.listener.LocalEndPoint);
        Socket vide;
        vide = Game.listener.Accept();
        Task t = Task.Run(() =>
        {
            Thread.Sleep(GetDelaiAlarme() * 1000);
            vide.Send(new byte[5] { 1, 2, 3, 4, 5 });
            boucle = false;
        });*/





        /*public (int, int) gameVote(List<Joueur> listJoueurs, Socket reveille)
        {
            Dictionary<Socket, Joueur> dictJoueur = new Dictionary<Socket, Joueur>();
            List<Socket> role = new List<Socket>();
            List<Socket> sockets = new List<Socket>(), read = new List<Socket>();
            sockets.Add(reveille);

            foreach (Joueur j in listJoueurs)
            {
                sockets.Add(j.GetSocket());
                Console.WriteLine("idjoueur=" + j.GetId());
                if (j.GetRole().GetIdRole() == this.GetIdRole())
                {
                    dictJoueur[j.GetSocket()] = j;
                    role.Add(j.GetSocket());
                }

            }
            while (true)
            {
                foreach (Socket socket in sockets)
                {
                    read.Add(socket);
                }
                Socket.Select(read, null, null, -1);
                if (read.Contains(reveille))
                {
                    return (-1, -1);
                }
                else
                {
                    foreach (Socket sock in read)
                    {
                        int[] size = new int[1] { 1 };
                        byte[] message = new byte[4096];
                        if (role.Contains(sock))
                        {
                            if (sock.Available == 0)
                            {
                                //gestion de deconnexion ???
                            }
                            else
                            {
                                sock.Receive(message);
                                if (message[0] == 1)
                                {

                                    int idVoter = server.decodeInt(message, size);
                                    int idVoted = server.decodeInt(message, size);
                                    Console.WriteLine("avant" + dictJoueur[sock].GetId());
                                    if (idVoter == dictJoueur[sock].GetId())
                                    {
                                        Console.WriteLine("apres");
                                        return (idVoter, idVoted);
                                    }

                                }
                                else
                                {

                                }
                            }
                        }
                        else
                        {
                            server.recvMessageGame(sock, sockets);

                        }
                    }
                }
            }
        }*/

    }

}
