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
    public static double decodeDouble(byte[] message, int[] size)
    {
        double result = BitConverter.ToDouble(message, size[0]);
        size[0] += sizeof(double);
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
    public static DateTime decodeDate(byte[] message, int[] size)
    {
        DateTime date = DateTime.FromBinary(BitConverter.ToInt64(message, size[0]));
        size[0] += sizeof(long);
        return date;
    }

    public static void encode(byte[] message, int val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(int));
        size[0] += sizeof(int);
    }
    public static void encode(byte[] message, double val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val), 0, message, size[0], sizeof(double));
        size[0] += sizeof(double);
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
    public static void encode(byte[] message, DateTime val, int[] size)
    {
        Array.Copy(BitConverter.GetBytes(val.Ticks), 0, message, size[0], sizeof(long));
        size[0] += sizeof(long);
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
                try{
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
                    case 156:
                        ResetPasswdReq(bdd, message);
                        break;
                    case 157:
                        ResetPassw(bdd, message);
                        break;
                    case 158:
                        searchForPlayer(bdd, message);
                        break;
                    case 159:
                        saveGame(message);
                        break;
                    case 160:
                        sendHistory(bdd, message);
                        break;
                    case 161:
                        sendAction(bdd, message);
                        break;
                    case 162:
                        sendRank(bdd, message);
                        break;
                    case 163:
                        sendStats(bdd, message);
                        break;
                }
                }catch(Exception e){
                        Console.WriteLine(e.ToString());
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
        if (val == 0)
        {
            inscriptionAnswer(bdd, queueId, true);
        }
        else
        {
            inscriptionAnswer(bdd, queueId, false);
        }

    }
    public static void sendAction(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idPartie = decodeInt(message, size);
        string action = Partie.get_action(conn, id, idPartie);
        byte[] newMessage;
        if (action == null)
        {
            newMessage = new byte[2] { 255, 161 };
            return;
        }
        newMessage = new byte[1 + sizeof(int) + sizeof(int) + action.Length];
        newMessage[0] = 161;
        size[0] = 1;
        encode(newMessage, queueId, size);
        encode(newMessage, action, size);
        sendMessage(bdd, newMessage);
    }
    public static void sendStats(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int trier = decodeInt(message, size);
        (string[] pseudo, int[] ids, int[] score, int[] nbPartie, double[] winrate) = Statistique.Get_all(conn, id, trier);
        int pname = getStringLength(pseudo);
        byte[] newMessage = new byte[1 + sizeof(int) + sizeof(int) + sizeof(int) * pseudo.Length + pname + sizeof(int) + sizeof(int) * ids.Length + sizeof(int) + sizeof(int) * score.Length + sizeof(int) + sizeof(int) * nbPartie.Length + sizeof(int) + sizeof(double) * winrate.Length];
        size[0] = 1;
        newMessage[0] = 163;
        encode(newMessage, queueId, size);
        encode(newMessage, pseudo.Length, size);
        for (int i = 0; i < pseudo.Length; i++)
        {
            encode(newMessage, pseudo[i], size);
        }
        encode(newMessage, ids.Length, size);
        for (int i = 0; i < ids.Length; i++)
        {
            encode(newMessage, ids[i], size);
        }
        encode(newMessage, score.Length, size);
        for (int i = 0; i < score.Length; i++)
        {
            encode(newMessage, score[i], size);
        }
        encode(newMessage, nbPartie.Length, size);
        for (int i = 0; i < nbPartie.Length; i++)
        {
            encode(newMessage, nbPartie[i], size);
        }
        encode(newMessage, winrate.Length, size);
        for (int i = 0; i < winrate.Length; i++)
        {
            encode(newMessage, winrate[i], size);
        }
        sendMessage(bdd, newMessage);
    }
    public static void sendRank(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        (int[] score, string[] names) = Statistique.Trierpar_score(conn);
        int pname = getStringLength(names);
        byte[] newMessage = new byte[1 + sizeof(int) * 2 + sizeof(int) * score.Length + sizeof(int) + sizeof(int) * names.Length + pname];
        newMessage[0] = 162;
        size[0] = 1;
        encode(newMessage, queueId, size);
        encode(newMessage, score.Length, size);
        for (int i = 0; i < score.Length; i++)
        {
            encode(newMessage, score[i], size);
        }
        encode(newMessage, names.Length, size);
        for (int i = 0; i < score.Length; i++)
        {
            encode(newMessage, names[i], size);
        }
        sendMessage(bdd, newMessage);


    }
    public static int inscriptionAnswer(Socket bdd, int queueId, bool answer)
    {
        int msgSize = 1 + sizeof(int) + sizeof(bool);
        byte[] message = new byte[msgSize];
        int[] size = new int[1] { 1 };
        message[0] = 104;
        encode(message, queueId, size);
        encode(message, answer, size);
        bdd.Send(message);
        return 0;
    }

    public static int connexionAnswer(Socket bdd, int queueId, bool answer, int idPlayer, string username, int[] friends, string[] names)
    {
        int psize = getStringLength(names);
        int msgSize = 1 + sizeof(bool) + sizeof(int);
        if (answer)
        {
            msgSize += sizeof(int) * 2 + username.Length + sizeof(int) + friends.Length * sizeof(int) + psize + sizeof(int) * names.Length;
        }
        byte[] message = new byte[msgSize];
        int[] size = new int[1] { 1 };
        message[0] = 105;
        encode(message, queueId, size);
        encode(message, answer, size);
        if (answer)
        {
            encode(message, idPlayer, size);
            encode(message, username, size);
            encode(message, friends.Length, size);
            foreach (int i in friends)
            {
                encode(message, i, size);
            }
            foreach (string name in names)
            {
                encode(message, name, size);
            }

        }
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
            int id = Login.get_id(conn, username);
            //recuperer les amis
            int[] ids, idAmis, idAttente; string[] names, nameAmis, nameAttente; DateTime[] date, dateAmis, dateAttente;
            (idAmis, nameAmis, dateAmis) = Amis.get_liste_amis(conn, id);
            (idAttente, nameAttente, dateAttente) = Amis.get_liste_amis_enattente(conn, id);
            ids = new int[idAmis.Length + idAttente.Length + 1];
            names = new string[nameAmis.Length + nameAttente.Length + 1];
            int i = 0;
            for (; i < idAmis.Length; i++)
            {
                ids[i] = idAmis[i];
                names[i] = nameAmis[i];
            }
            ids[i] = -1;
            names[i] = "";
            i++;
            int j = 0;
            for (; j < idAttente.Length; j++)
            {
                ids[i + j] = idAttente[j];
                names[i + j] = nameAttente[j];
            }
            connexionAnswer(bdd, queueId, true, id, username, ids, names);
        }
        else
            connexionAnswer(bdd, queueId, false, 0, username, new int[0], new string[0]);
    }
    public static void saveGame(byte[] message)
    {

        int[] size = new int[1] { 1 };
        string name = decodeString(message, size);
        string action = decodeString(message, size);
        string actionAng = decodeString(message,size);

        int tableSize = decodeInt(message, size);
        int[] ids = new int[tableSize];
        for (int i = 0; i < ids.Length; i++)
        {
            ids[i] = decodeInt(message, size);
        }
        tableSize = decodeInt(message, size);
        int[] score = new int[tableSize];
        for (int i = 0; i < score.Length; i++)
        {
            score[i] = decodeInt(message, size);
        }
        tableSize = decodeInt(message, size);
        bool[] win = new bool[tableSize];
        for (int i = 0; i < score.Length; i++)
        {
            win[i] = decodeBool(message, size);
        }
        int idPartie = Partie.create_partie(conn, name);
        Partie.write_action(conn, action, idPartie);
        for (int i = 0; i < ids.Length; i++)
        {

            Partie.init_sauvegardepartie(conn, idPartie, ids[i], score[i]);
        }
        Statistique.nouveau_jeu(conn, ids);
        Statistique.score_gain(conn, ids, score);
        Statistique.jeu_gagne(conn, ids, win);

    }
    public static void sendHistory(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idUser = decodeInt(message, size);
        int[] ids;
        string[] names;
        DateTime[] dates;
        int[] score;
        (ids, names, dates, score) = Partie.get_partie(conn, idUser);
        int pname = getStringLength(names);
        byte[] newMessage = new byte[1 + sizeof(int) * 2 + sizeof(int) * ids.Length + sizeof(int) + sizeof(int) * names.Length + pname + sizeof(int) + sizeof(long) * dates.Length + sizeof(int) + sizeof(int) * score.Length];
        newMessage[0] = 160;
        size[0] = 1;
        encode(newMessage, queueId, size);
        encode(newMessage, ids.Length, size);
        for (int i = 0; i < ids.Length; i++)
        {
            encode(newMessage, ids[i], size);
        }
        encode(newMessage, names.Length, size);
        for (int i = 0; i < names.Length; i++)
        {
            encode(newMessage, names[i], size);
        }
        encode(newMessage, dates.Length, size);
        for (int i = 0; i < dates.Length; i++)
        {
            encode(newMessage, dates[i], size);
        }
        encode(newMessage, score.Length, size);
        for (int i = 0; i < score.Length; i++)
        {
            encode(newMessage, score[i], size);
        }
        sendMessage(bdd, newMessage);
    }
    public static int ajoutAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idFriend = decodeInt(message, size);
        Console.WriteLine("return = " + Amis.send_friend_request(conn, id, idFriend));
        string pseudoJoueur = Amis.get_pseudo(conn, id);
        string username = Amis.get_pseudo(conn, idFriend);
        Console.WriteLine($"l'utilisateur {id} rajoute la personne avec le nom {username}");
        byte[] newMessage = new byte[1 + sizeof(int) + sizeof(bool) + sizeof(int) + sizeof(int) + pseudoJoueur.Length + sizeof(int) + sizeof(int) + username.Length];
        size[0] = 1;
        newMessage[0] = 153;
        encode(newMessage, queueId, size);
        encode(newMessage, true, size);
        encode(newMessage, id, size);
        encode(newMessage, pseudoJoueur, size);
        encode(newMessage, idFriend, size);
        encode(newMessage, username, size);
        return sendMessage(bdd, newMessage);

    }
    public static int supprimerAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idDelete = decodeInt(message, size);
        Amis.refuse_friend_request(conn, id, idDelete);
        Console.WriteLine($"l'utilisateur {id} supprime la personne avec le nom {idDelete}");
        byte[] newMessage = new byte[1 + sizeof(int) * 3 + sizeof(bool)];
        size[0] = 1;
        newMessage[0] = 154;
        encode(newMessage, queueId, size);
        encode(newMessage, true, size);
        encode(newMessage, id, size);
        encode(newMessage, idDelete, size);
        return sendMessage(bdd, newMessage);
    }
    public static int reponseAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        bool answer = decodeBool(message, size);
        int id = decodeInt(message, size);
        int idReponse = decodeInt(message, size);
        if (answer)
        {
            answer = true;
            Amis.accept_friend_request(conn, id, idReponse);
        }
        else
        {
            answer = false;
            Amis.refuse_friend_request(conn, id, idReponse);
        }
        Console.WriteLine($"l'utilisateur {id} repond a l'invitation de {idReponse} avec {answer}");
        byte[] newMessage = new byte[1 + sizeof(int) * 3 + sizeof(bool)];
        size[0] = 1;
        newMessage[0] = 155;
        encode(newMessage, queueId, size);
        encode(newMessage, answer, size);
        encode(newMessage, id, size);
        encode(newMessage, idReponse, size);
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

    public static void ResetPasswdReq(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        string pseudo = decodeString(message, size);
        size[0] = 1;
        bool answer = dataBase.Resetmdp.ResetPassword(conn, pseudo);
        EmailSent(bdd, queueId, answer);
    }
    public static int EmailSent(Socket bdd, int queueId, bool answer)
    {
        int messageSize = 1 + sizeof(bool) + sizeof(int);
        byte[] msg = new byte[messageSize];
        int[] index = new int[1] { 1 };
        msg[0] = 156;
        encode(msg, queueId, index);
        encode(msg, answer, index);
        return bdd.Send(msg);

    }

    public static void ResetPassw(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queue = decodeInt(message, size);
        string email = decodeString(message, size);
        string oldpass = decodeString(message, size);
        string newpass = decodeString(message, size);
        byte[] msg = new byte[1 + sizeof(bool) + sizeof(int)];
        size[0] = 1;
        msg[0] = 157;
        encode(msg, queue, size);
        if (Resetmdp.changement_mdp(conn, email, oldpass, newpass) == 0)
        {
            encode(msg, true, size);
        }
        else
        {
            encode(msg, false, size);
        }
        bdd.Send(msg);
    }

    public static void searchForPlayer(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        string username = decodeString(message, size);
        int[] ids; string[] usernames;
        (ids, usernames) = Amis.search_for_player(conn, id, username);
        sendSearchPlayer(bdd, queueId, ids, usernames);

    }
    public static void sendSearchPlayer(Socket bdd, int queueId, int[] id, string[] username)
    {
        int pname = getStringLength(username);
        byte[] message = new byte[1 + sizeof(int) * 3 + sizeof(int) * id.Length + sizeof(int) * username.Length + pname];
        int[] size = new int[1] { 1 };
        message[0] = 158;
        encode(message, queueId, size);
        encode(message, id.Length, size);
        foreach (int i in id)
        {
            encode(message, i, size);
        }
        encode(message, username.Length, size);
        foreach (string name in username)
        {
            encode(message, name, size);
        }
        sendMessage(bdd, message);
    }
}