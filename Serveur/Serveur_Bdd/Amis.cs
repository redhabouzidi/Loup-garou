using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;


class Amis
{

    public static int get_id(MySqlConnection conn, string pseudo)
    {
        //Requete pour recuperer identifiant du joueur
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_joueur
        int id_joueur = conn.QueryFirstOrDefault<int>(query, new { Pseudo = pseudo });
        return id_joueur;
    }
    public static string get_username(MySqlConnection conn,int id)
    {
        string query = "SELECT pseudo FROM Utilisateurs WHERE idUsers=@IDA";
        //id=id_joueur
        string pseudo_joueur= conn.QueryFirstOrDefault<string>(query, new { IDA = id });
        return pseudo_joueur;
    }


    public static int send_friend_request(MySqlConnection conn, int id_sender, int receiver)
    {//envoyer une demande
        //Requete pour recuperer identifiant du joueur qui a reçoit la demande
        //id=id_receiver
        int id_receiver = receiver;
        using (MySqlCommand command = new MySqlCommand())
        {
            query = "SELECT count(*) FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            int rowcount = conn.QueryFirstOrDefault<int>(query, new { IDS = id_receiver, IDR = id_sender });
            Console.WriteLine(rowcount);
            if (rowcount > 0) return -1;
            command.Connection = conn;
            command.CommandText = "INSERT INTO Amis (idUsers1,idUsers2,status_ami,date_amis) VALUES (@IDS,@IDR,@STA,@DA)";
            command.Parameters.AddWithValue("@IDS", id_sender);
            command.Parameters.AddWithValue("@IDR", id_receiver);
            command.Parameters.AddWithValue("@STA", false);
            command.Parameters.AddWithValue("@DA", DateTime.Now);
            rowcount = command.ExecuteNonQuery();
            if (rowcount == 0) Console.WriteLine("Failed while inserting");
            else Console.WriteLine("Success");
            return id_receiver;
        }
    }


    public static void accept_friend_request(MySqlConnection conn, int id_playeraccepte, int id_playersend)
    {//accepter la demande
        Console.WriteLine("idplayeraccepte=" + id_playeraccepte + "  idplayersend " + id_playersend);
        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = conn;
            command.CommandText = "UPDATE Amis SET status_ami=@STA, date_amis=@DTE WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS", id_playersend);
            command.Parameters.AddWithValue("@IDR", id_playeraccepte);
            command.Parameters.AddWithValue("@STA", true);
            command.Parameters.AddWithValue("@DTE", DateTime.Now);
            int rowcount = command.ExecuteNonQuery();
            if (rowcount == 0) Console.WriteLine("Failed while updating");
            else Console.WriteLine("Success");
        }
    }


    public static void refuse_friend_request(MySqlConnection conn, int id_playerrefuse, int id_playersender)
    {//refuser la demande ou supprimer

        using (MySqlCommand command = new MySqlCommand())
        {
            command.Connection = conn;
            command.CommandText = "DELETE FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS", id_playerrefuse);
            command.Parameters.AddWithValue("@IDR", id_playersender);
            int rowcount = command.ExecuteNonQuery();
            if (rowcount == 0) Console.WriteLine("Failed while deleting");
            else Console.WriteLine("Success");
        }
    }


    public static Dictionary<Tuple<int, string>, DateTime> get_liste_amis(MySqlConnection conn, int player_id, bool trie_date)
    {

        Dictionary<Tuple<int, string>, DateTime> affic = new Dictionary<Tuple<int, string>, DateTime>();
        string query;
        if (trie_date)  query = "SELECT * FROM Amis WHERE idUsers1=@IDF ORDER BY date_amis DESC";
        else query = "SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data = conn.Query(query, new { IDF = player_id });
        foreach (dynamic row in data)
        {
            int id_rec = row.idUsers2;
            if (id_rec == player_id) continue;
            query = "SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec = conn.QueryFirstOrDefault<string>(query, new { ID = id_rec });
            if (player_name_rec == null) continue;
            bool stat_ami = row.status_ami;
            DateTime timestamp = row.date_amis;
            if (stat_ami)
            {
                affic.Add(Tuple.Create(id_rec, player_name_rec), timestamp);
            }
        }
        if (trie_date) query = "SELECT * FROM Amis WHERE idUsers2=@IDS ORDER BY date_amis DESC";
        else query = "SELECT * FROM Amis WHERE idUsers2=@IDS";
        data = conn.Query(query, new { IDS = player_id });
        foreach (dynamic row in data)
        {
            int id_rec = row.idUsers1;
            if (id_rec == player_id) continue;
            query = "SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec = conn.QueryFirstOrDefault<string>(query, new { ID = id_rec });
            if (player_name_rec == null) continue;
            bool stat_ami = row.status_ami;
            DateTime timestamp = row.date_amis;
            if (stat_ami)
            {
                affic.Add(Tuple.Create(id_rec, player_name_rec), timestamp);
            }
        }
        return affic;
    }


    public static Dictionary<Tuple<int, string>, DateTime> get_liste_amis_enattente(MySqlConnection conn, int player_id, bool trie_date)
    {
        string query;
        Dictionary<Tuple<int, string>, DateTime> affic = new Dictionary<Tuple<int, string>, DateTime>();
        if (trie_date) query = "SELECT * FROM Amis WHERE idUsers1=@IDF ORDER BY date_amis DESC";
        else query = "SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data = conn.Query(query, new { IDF = player_id });
        foreach (dynamic row in data)
        {
            int id_rec = row.idUsers2;
            if (id_rec == player_id) continue;
            query = "SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec = conn.QueryFirstOrDefault<string>(query, new { ID = id_rec });
            if (player_name_rec == null) continue;
            bool stat_ami = row.status_ami;
            DateTime timestamp = row.date_amis;
            if (stat_ami == false)
            {
                affic.Add(Tuple.Create(id_rec, player_name_rec), timestamp);
            }
        }
        affic.Add(Tuple.Create(-1, "E"), new DateTime());
        if (trie_date) query = "SELECT * FROM Amis WHERE idUsers2=@IDS ORDER BY date_amis DESC";
        else query = "SELECT * FROM Amis WHERE idUsers2=@IDS";
        data = conn.Query(query, new { IDS = player_id });
        foreach (dynamic row in data)
        {
            int id_rec = row.idUsers1;
            if (id_rec == player_id) continue;
            query = "SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec = conn.QueryFirstOrDefault<string>(query, new { ID = id_rec });
            if (player_name_rec == null) continue;
            bool stat_ami = row.status_ami;
            DateTime timestamp = row.date_amis;
            if (stat_ami == false)
            {
                affic.Add(Tuple.Create(id_rec, player_name_rec), timestamp);
            }
        }
        return affic;
    }


    public static int get_friend_count(MySqlConnection conn, string pseudo)
    {
        int friend_count = 0;
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_sender
        int id_player = conn.QueryFirstOrDefault<int>(query, new { Pseudo = pseudo });

        query = "SELECT * FROM Amis WHERE idUsers1=@IDS OR idUsers2=@IDF";
        IEnumerable<dynamic> data = conn.Query(query, new { IDS = id_player, IDF = id_player });
        foreach (dynamic row in data)
        {
            if (row.status_ami == true) friend_count++;
        }
        return friend_count;
    }
}
