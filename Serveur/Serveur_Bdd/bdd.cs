﻿using System;
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
                    case 156:
                        ResetPasswdReq(bdd, message);
                        break;
                    case 157:
                        //ResetPassw(bdd, message);
                        break;
                    case 158:
                        searchForPlayer(bdd, message);
                        break;
                    case 159:
                        saveGame(message);
                        break;
                    case 160:
                        sendHistory(bdd,message);
                        break;
                    case 161:
                        //ENVOYER DONNE DE LA PARTIE DEMANDE
                        break;
                    case 162:
                        //ENVOYER STATISTIQUE
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
        if (val==0)
        {
            inscriptionAnswer(bdd, queueId, true);
        }
        else
        {
            inscriptionAnswer(bdd, queueId, false);
        }

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
            int[] ids,idAmis,idAttente;string[] names,nameAmis,nameAttente;DateTime[] date,dateAmis,dateAttente;
            (idAmis,nameAmis,dateAmis) = Amis.get_liste_amis(conn, id);
            (idAttente,nameAttente,dateAttente) = Amis.get_liste_amis_enattente(conn, id);
            ids = new int[idAmis.Length + idAttente.Length + 1];
            names = new string[nameAmis.Length + nameAttente.Length + 1];
            int i = 0;
            for (;i< idAmis.Length;i++)
            {
                ids[i] = idAmis[i];
                names[i] = nameAmis[i];
            }
            ids[i] = -1;
            names[i] = "";
            i++;
            int j=0;
            for (; j < idAttente.Length; j++)
            {
                ids[i+j] = idAttente[j];
                names[i+j] = nameAttente[j];
            }
            connexionAnswer(bdd, queueId, true, id, username, ids, names);
        }
        else
            connexionAnswer(bdd, queueId, false, 0, username, new int[0], new string[0]);
    }
    public static void saveGame(byte[] message){
        int[] size = new int[1]{1};
        string name=decodeString(message,size);
        Console.WriteLine("name ="+name);
        string action=decodeString(message,size);
        Console.WriteLine("action  ="+action );

        int tableSize=decodeInt(message,size);
        int[] ids=new int [tableSize];
        for(int i=0;i<ids.Length;i++){
            ids[i]=decodeInt(message,size);
        }
        tableSize=decodeInt(message,size);
        int[] score=new int [tableSize];
        for(int i=0;i<score.Length;i++){
            score[i]=decodeInt(message,size);
        }
        
        int idPartie=Partie.create_partie(conn,name);
        Partie.write_action(conn,action,idPartie);
        for(int i=0;i<ids.Length;i++){

            Partie.init_sauvegardepartie(conn,idPartie,ids[i],score[i]);
        }

    }
    public static void sendHistory(Socket bdd, byte[] message){
        int[] size = new int[1]{1};
        int queueId=decodeInt(message,size);
        Console.WriteLine("queue="+queueId);
        int id=decodeInt(message,size);
        int idUser = decodeInt(message,size);
        int[] ids;
        string[] names;
        (ids,names) = Partie.get_partie(conn,idUser);
        int pname = getStringLength(names);
        byte[] newMessage = new byte[1+sizeof(int)*2+sizeof(int)*ids.Length+sizeof(int)+sizeof(int)*names.Length+pname];
        newMessage[0]=160;
        size[0]=1;
        encode(newMessage,queueId,size);
        encode(newMessage,ids.Length,size);
        for(int i=0;i<ids.Length;i++){
            encode(newMessage,ids[i],size);
        }
        encode(newMessage,names.Length,size);
        for(int i=0;i<names.Length;i++){
            encode(newMessage,names[i],size);
        }
        sendMessage(bdd, newMessage);
    }
    public static int ajoutAmi(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        int idFriend = decodeInt(message, size);
        Amis.send_friend_request(conn, id, idFriend);
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
        string email = decodeString(message, size);
        size[0] = 1;
        bool answer = dataBase.Resetmdp.ResetPassword(conn, email);
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
/*
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
*/
    public static void searchForPlayer(Socket bdd, byte[] message)
    {
        int[] size = new int[1] { 1 };
        int queueId = decodeInt(message, size);
        int id = decodeInt(message, size);
        string username = decodeString(message, size);
        int[] ids; string[] usernames;
        (ids, usernames) = Amis.search_for_player(conn,username);
        sendSearchPlayer(bdd, queueId, ids, usernames);

    }
    public static void sendSearchPlayer(Socket bdd,int queueId,int[] id,string[] username)
    {
        int pname = getStringLength(username);
        byte[] message = new byte[1 + sizeof(int) * 3 + sizeof(int) * id.Length + sizeof(int)*username.Length + pname];
        int[] size = new int[1] { 1 };
        message[0] = 158;
        encode(message, queueId, size);
        encode(message, id.Length, size);
        foreach(int i in id)
        {
            encode(message, i, size);
        }
        encode(message, username.Length, size);
        foreach(string name in username)
        {
            encode(message, name, size);
        }
        sendMessage(bdd, message);
    }
}