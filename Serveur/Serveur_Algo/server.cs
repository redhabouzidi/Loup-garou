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
    public class Amis
    {
        private Socket sock;
        private int status;
        private string username;
        private List<int> friendList = new List<int>();
        public Amis(Socket sock, int status)
        {
            this.sock = sock;
            this.status = status;
        }
        public void AddFriend(int id)
        {
            friendList.Add(id);
        }
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
        public void SetStatus(int key, int status)
        {
            this.status = status;
            foreach (int i in friendList)
            {
                if (server.userData.ContainsKey(i))
                {
                    if (server.userData[i].GetSocket() != null && server.userData[i].GetSocket().Connected)
                        server.sendStatus(server.userData[i].GetSocket(), key, status);
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
        public static Dictionary<int, Game> players = new Dictionary<int, Game>();
        public static Dictionary<Socket, int> connected = new Dictionary<Socket, int>();
        public static Dictionary<int, Game> games = new Dictionary<int, Game>();
        public static Dictionary<int, Amis> userData = new Dictionary<int, Amis>();
        public static Dictionary<Socket, Aes> client_keys = new Dictionary<Socket, Aes>();
        public static Socket wakeUpMain;
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
            int port = 10004;
            bool reading = true, a = true;
            Socket server = setupSocketClient(port), serverbdd = setupSocketBdd(10005), bdd;
            byte[] message = new byte[1024];
            // try
            // {
            games.Add(0, new Game());
            bdd = serverbdd.Accept();

            List<Socket> list = new List<Socket> { server, bdd };
            List<Socket> fds = new List<Socket>();
            wakeUpMain = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            wakeUpMain.Connect(server.LocalEndPoint);
            list.Add(server.Accept());
            Queue queue = new Queue();
            int i = 0;
            Crypto crypto = new Crypto();
            while (a)
            {

                i++;
                fds.Clear();
                foreach (Socket fd in list)
                {
                    fds.Add(fd);
                }
                foreach (KeyValuePair<Socket, int> input in connected)
                {
                    fds.Add(input.Key);
                }

                Socket.Select(fds, null, null, -1);
                Console.WriteLine("on recoit un truc");
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
                    acceptConnexions(list, server, crypto);
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

                    Console.WriteLine("KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK "+ list.Contains(fd));
                    // Console.WriteLine("le ie point du waikupmain is "+wakeUpMain.RemoteEndPoint.ToString());
                    if (fd.Available == 0)
                    {
                        disconnectPlayer(list, fd);
                    }
                    else if (client_keys.ContainsKey(fd) || fd.Equals(wakeUpMain))
                    {
                        recvMessage(fd, bdd, list, connected, queue, players);
                    }
                    else
                    {
                        client_keys.TryAdd(fd, crypto.RecvAes(fd));
                    }
                }
            }


        }
        public static void disconnectPlayer(List<Socket> list, Socket fd)
        {
            list.Remove(fd);
            if (connected.ContainsKey(fd))
            {
                userData[connected[fd]].SetStatus(connected[fd], -1);
                userData.Remove(connected[fd]);
                disconnectFromLobby(fd);
                connected.Remove(fd);
            }
            client_keys.Remove(fd);
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
        public static Socket setupSocketGame()
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 2001);
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
            return;
        }
        //fonction qui envoie un message a un socket donne en parametre
        public static void sendMessage(Socket client, byte[] message)
        {
            if (client_keys.ContainsKey(client))
            {
                Console.WriteLine("Le premier byte non crtypté est est {0}", message[0]);
                byte[] cryptedMessage = Crypto.EncryptMessage(message, client_keys[client]);
                Console.WriteLine("la taille du message crypté est de {0} et la taille normale {1}", cryptedMessage.Length, message.Length);
                Console.WriteLine("Le premier byte crypté est {0}", cryptedMessage[4]);

                client.Send(cryptedMessage, cryptedMessage.Length, SocketFlags.None);
            }
            else
            {
                client.Send(message, message.Length, SocketFlags.None);
            }


        }
        public static void sendMessage(Socket client, byte[] message, int recvSize)
        {
            foreach (byte b in message)
            {
                Console.Write(b + " ");
            }
            Console.WriteLine("");
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
                client.Send(message, recvSize, SocketFlags.None);
            }
        }

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
        public static void joinGame(Socket client, int gameId, int idj)
        {
            if (games.ContainsKey(gameId))
            {

                if (connected.ContainsKey(client))
                {

                    if (!players.ContainsKey(connected[client]))
                    {
                        if (games[gameId].GetJoueurManquant() != 0)
                        {
                            Console.WriteLine($"joins game {gameId}");
                            Console.WriteLine("id joueur : " + idj);
                            games[gameId].Join(new Client(idj, client, userData[idj].GetUsername()));
                            players[idj] = games[gameId];
                            userData[idj].SetStatus(idj, 2);
                        }
                        else
                        {
                            sendMessage(client, new byte[] { 255 });
                        }

                    }
                    else
                    {
                        Console.WriteLine("already Playing");
                        Game g = players[connected[client]];
                        foreach (Joueur j in g.GetJoueurs())
                        {
                            if (j.GetId() == idj)
                            {
                                Console.WriteLine("envoie d'info et mises a jjour");
                                j.SetSocket(client);
                                g.sendGameInfo(client);
                                g.sendRoles(j);

                                //envoyer les information déjà connue
                            }
                        }
                        foreach (Joueur j in g.GetJoueurs())
                        {
                            if (!j.GetEnVie())
                            {
                                annonceMort(client, j.GetId(), j.GetRole().GetIdRole());
                            }
                        }
                        connected.Remove(client);
                        g.vide.Send(new byte[1] { 0 });

                    }
                }
                else
                {
                    Console.WriteLine("Not connected(shouldn't be possible)");
                }




            }
            else
            {
                sendMessage(client, new byte[] { 255 });
            }
        }
        //fonction qui sert a envoyer les informztion d'une game a un client
        public static void sendGameInfo(Socket client, int nbPlayers, int nbLoup, bool sorciere, bool voyante, bool cupidon, string name, int[] idPlayers, string[] playerNames)
        {
            //variable d'index pour ne pas se perdre dans le tableau de bytes
            int[] size = new int[1] { 0 };
            //recuperer le nombre total de caracteres
            int pnsize = getStringLength(playerNames);
            //declarer un tableau de bytes avec le bon nombre de bytes
            byte[] message = new byte[1 + sizeof(int) + sizeof(int) + sizeof(bool) + sizeof(bool) + sizeof(bool) + sizeof(int) + name.Length + sizeof(int) + idPlayers.Length * sizeof(int) + sizeof(int) * playerNames.Length + pnsize];
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

        public static void SendCurrentGame(Socket client, int[] nbPlayers, int[] gameId, string[] name)
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
        public static void etatGame(Socket client, bool day)
        {
            int msgSize = 1 + sizeof(bool);
            byte[] message = new byte[msgSize];
            message[0] = 5;
            int[] size = new int[1] { 1 };
            encode(message, day, size);
            sendMessage(client, message);
        }
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
        public static void leaveLobby(Socket client, List<Socket> clients, int id)
        {
            int msgSize = 1 + sizeof(int);
            byte[] message = new byte[msgSize];
            int[] size = new int[1] { 1 };
            message[0] = 13;
            encode(message, id, size);
            foreach (Socket sock in clients)
            {
                sendMessage(sock, message);
            }
        }
        public static void ready(Socket client, List<Socket> clients, int id, bool status)
        {
            int msgSize = 1 + sizeof(int) + sizeof(bool);
            byte[] message = new byte[msgSize];
            int[] size = new int[1] { 1 };
            message[0] = 14;
            encode(message, id, size);
            encode(message, status, size);
            foreach (Socket sock in clients)
            {
                sendMessage(sock, message);
            }
        }


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
        public static void recvMessageGame(List<Socket> list, byte[] message, int receivedBytes)
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
                        sendMessage(s, message, receivedBytes);
                    }
                    break;
                case 16:
                    size[0] = 2;
                    idPlayer = decodeInt(message, size);
                    sendSystemMessage(list, message[1], userData[idPlayer].GetUsername());
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
                    Console.WriteLine("on est la et le code est "+message[0]);
                    break;
            }
        }
        public static void recvMessage(Socket client, Socket bdd, List<Socket> list, Dictionary<Socket, int> connected, Queue queue, Dictionary<int, Game> players)
        {
            int[] size = new int[1];
            int dataSize, tableSize, id;
            byte[] message;
            if (client.Equals(wakeUpMain))
            {
                message = new byte[2048];
                int receivedBytes = client.Receive(message);
                Console.WriteLine("le message dont la socket n'est pas dans le dictionnaire est " + message[0] + " received bytes =" + receivedBytes);
            }
            else
            {
                byte[] encryptedMessage = new byte[2048];
                int receivedBytes = client.Receive(encryptedMessage);
                message = Crypto.DecryptMessage(encryptedMessage, client_keys[client], receivedBytes);
                receivedBytes = message.Length;
            }
            string username, password, chat;
            int idPlayer, vote;
            switch (message[0])
            {
                case 2:
                    size[0] = 1;
                    int friendId = decodeInt(message, size);
                    int idj = decodeInt(message, size);
                    if (players.ContainsKey(friendId))
                    {
                        Game g = players[friendId];
                        Console.WriteLine("gameid=" + g.GetGameId());
                        joinGame(client, g.GetGameId(), idj);

                    }
                    break;
                case 3:
                    size[0] = 1;

                    id = decodeInt(message, size);
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

                    if (connected.ContainsKey(client))
                    {

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
                            sendMessage(client, new byte[] { 255 });
                        }

                    }
                    else
                    {
                        Console.WriteLine("non connecté aled");
                    }
                    break;
                case 4:
                    size[0] = 1;
                    int gameId = decodeInt(message, size);
                    idj = decodeInt(message, size);
                    Console.WriteLine("gameid=" + gameId);
                    joinGame(client, gameId, idj);

                    break;
                case 100://disconnects
                    disconnectPlayer(list, client);

                    break;
                case 103://informations des lobby
                    if (connected.ContainsKey(client))
                    {
                        int[] idGames = new int[games.Count];
                        int[] nbPlayer = new int[games.Count];
                        string[] gameNames = new string[games.Count];
                        int i = 0;
                        //charger toutes les information des parties dans les variables
                        foreach (KeyValuePair<int, Game> data in games)
                        {
                            if (!data.Value.GetStart())
                            {
                                idGames[i] = data.Key; ;
                                nbPlayer[i] = data.Value._nbrJoueurs;
                                gameNames[i] = data.Value.name;
                                i++;
                            }
                        }
                        int[] idGamesSend = new int[i];
                        int[] nbPlayerSend = new int[i];
                        string[] gameNamesSend = new string[i];
                        for (int j = 0; j < i; j++)
                        {
                            idGamesSend[j] = idGames[j]; ;
                            nbPlayerSend[j] = nbPlayer[j];
                            gameNamesSend[j] = gameNames[j];
                        }
                        //envoie
                        SendCurrentGame(client, nbPlayerSend, idGamesSend, gameNamesSend);
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
                case 106:
                    if (connected.ContainsKey(client))
                        disconnectFromLobby(client);
                    else
                        Console.WriteLine("nope pas co");


                    break;
                case 108:
                    if (connected.ContainsKey(client))
                    {
                        if (players.ContainsKey(connected[client]))
                        {
                            if (!players[connected[client]].GetStart())
                            {
                                idj = connected[client];
                                players[connected[client]].ToggleReady(idj);
                            }

                        }

                    }
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
                    break;
                case 156:
                    redirect(bdd, queue.addVal(client), message);
                    break;
                case 157:
                    redirect(bdd, queue.addVal(client), message);
                    break;
                case 158:
                    if (connected.ContainsKey(client))
                        redirect(bdd, queue.addVal(client), message);
                    break;
                case 255:
                    return;
                default:
                    break;
            }
        }
        public static void recvBddMessage(Socket bdd, Queue queue, List<Socket> list, Dictionary<Socket, int> connected)
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

                    if (answer)
                    {
                        int idPlayer = decodeInt(message, size);
                        string username = decodeString(message, size);
                        userData[idPlayer] = new Amis(queue.queue[queueId], 0);
                        if (!alreadyConnected(connected, idPlayer))
                        {
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
                            if (players.ContainsKey(idPlayer))
                            {
                                userData[idPlayer].SetStatus(idPlayer, 3);

                                //Player reconnect function

                            }
                            else
                            {
                                userData[idPlayer].SetStatus(idPlayer, 1);

                            }
                            userData[idPlayer].SetUsername(username);
                            //recuperer la liste d'amis ( a faire )
                            list.Remove(queue.queue[queueId]);
                            connected.Add(queue.queue[queueId], idPlayer);


                        }
                    }
                    Socket client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, size[0]);
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
                    answer = decodeBool(message, size);
                    decodeInt(message, size);
                    decodeString(message, size);
                    int idFriend = decodeInt(message, size);
                    decodeString(message, size);
                    if (answer && userData.ContainsKey(idFriend))
                    {
                        redirect(userData[idFriend].GetSocket(), message, recvSize);
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                case 154:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    answer = decodeBool(message, size);
                    decodeInt(message, size);
                    idFriend = decodeInt(message, size);
                    if (answer && userData.ContainsKey(idFriend))
                    {
                        redirect(userData[idFriend].GetSocket(), message, recvSize);
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
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
                            userData[id].AddFriend(idFriend);
                        }
                        if (userData.ContainsKey(idFriend))
                        {
                            userData[id].AddFriend(id);
                            redirect(userData[idFriend].GetSocket(), message, recvSize);
                        }
                    }
                    else
                    {
                        if (userData.ContainsKey(idFriend))
                        {
                            redirect(userData[idFriend].GetSocket(), message, recvSize);
                        }
                    }
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);

                    break;
                case 156:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                case 157:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;
                case 158:
                    size[0] = 1;
                    queueId = decodeInt(message, size);
                    client = queue.queue[queueId];
                    queue.queue.Remove(queueId);
                    redirect(client, message, recvSize);
                    break;

            }
        }

        public static void setLovers(Socket player1, Socket player2, int id1, int id2, int role1, int role2)
        {
            byte[] message = new byte[1 + 2 * sizeof(int)];
            message[0] = 6;
            int[] index = new int[1] { 1 };
            Console.WriteLine("je suis la ");
            encode(message, id1, index);
            encode(message, role1, index);
            if (player2 != null && player2.Connected)
                sendMessage(player2, message);
            index[0] = 1;
            encode(message, id2, index);
            encode(message, role2, index);
            if (player1 != null && player1.Connected)
                sendMessage(player1, message);
        }
        public static void EnvoieInformation(Socket client, int id)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 8;
            int[] size = new int[1] { 1 };
            encode(message, id, size);
            Console.WriteLine("information envoyé a la sorciere");
            sendMessage(client, message);

        }
        public static void sendTurn(Socket client, int roleId)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 11;
            int[] size = new int[1] { 1 };
            encode(message, roleId, size);
            sendMessage(client, message);

        }
        public static void sendTime(Socket client, int time)
        {
            byte[] message = new byte[1 + sizeof(int)];
            message[0] = 12;
            int[] size = new int[1] { 1 };
            encode(message, time, size);
            sendMessage(client, message);


        }
        public static void WakeUpMain()
        {
            wakeUpMain.Send(new byte[1] { 255 });
        }
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
