using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;


class Amis
{

    public int get_id(MySqlConnection conn, string pseudo)
    {
        //Requete pour recuperer identifiant du joueur
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_joueur
        int id_joueur = conn.QueryFirstOrDefault<int>(query, new { Pseudo = pseudo });
        return id_joueur;
    }


    public void send_friend_request(MySqlConnection conn, string sender, string receiver)
    {//envoyer une demande

        //Requete pour recuperer identifiant du joueur qui envoie la demande
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_sender
        int id_sender = conn.QueryFirstOrDefault<int>(query, new { Pseudo = sender });
        //Requete pour recuperer identifiant du joueur qui a reçoit la demande
        query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_receiver
        int id_receiver = conn.QueryFirstOrDefault<int>(query, new { Pseudo = receiver });
        using (MySqlCommand command = new MySqlCommand())
        {
            query = "SELECT count(*) FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            int rowcount = conn.QueryFirstOrDefault<int>(query, new { IDS = id_receiver, IDR = id_sender });
            Console.WriteLine(rowcount);
            if (rowcount > 0) return;
            command.Connection = conn;
            command.CommandText = "INSERT INTO Amis (idUsers1,idUsers2,status_ami,date_amis) VALUES (@IDS,@IDR,@STA,@DA)";
            command.Parameters.AddWithValue("@IDS", id_sender);
            command.Parameters.AddWithValue("@IDR", id_receiver);
            command.Parameters.AddWithValue("@STA", false);
            command.Parameters.AddWithValue("@DA", DateTime.Now);
            rowcount = command.ExecuteNonQuery();
            if (rowcount == 0) Console.WriteLine("Failed while inserting");
            else Console.WriteLine("Success");
        }
    }


    public void accept_friend_request(MySqlConnection conn, string playeraccepte, string playersend)
    {//accepter la demande
        //Requete pour recuperer identifiant du joueur qui envoie la demande
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_playeraccepte
        int id_playeraccepte = conn.QueryFirstOrDefault<int>(query, new { Pseudo = playeraccepte });
        //Requete pour recuperer identifiant du joueur qui a reçoit la demande
        query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_playersend
        int id_playersend = conn.QueryFirstOrDefault<int>(query, new { Pseudo = playersend });
        Console.WriteLine(playeraccepte + " idplayeraccepte=" + id_playeraccepte + " " + playersend + " idplayersend " + id_playersend);
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


    public void refuse_friend_request(MySqlConnection conn, string playerrefuse, string playersender)
    {//refuser la demande ou supprimer
        //Requete pour recuperer identifiant du joueur qui envoie la demande
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_playerrefuse
        int id_playerrefuse = conn.QueryFirstOrDefault<int>(query, new { Pseudo = playerrefuse });
        //Requete pour recuperer identifiant du joueur qui a reçoit la demande
        query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_playersender
        int id_playersender = conn.QueryFirstOrDefault<int>(query, new { Pseudo = playersender });
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


    public Dictionary<Tuple<string, string>, DateTime> get_liste_amis(MySqlConnection conn, string player, bool trie_date)
    {
        //Requete pour recuperer identifiant du joueur qui envoie la demande
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_sender
        int player_id = conn.QueryFirstOrDefault<int>(query, new { Pseudo = player });
        Dictionary<Tuple<string, string>, DateTime> affic = new Dictionary<Tuple<string, string>, DateTime>();
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
            if (stat_ami)
            {
                affic.Add(Tuple.Create(player, player_name_rec), timestamp);
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
                affic.Add(Tuple.Create(player, player_name_rec), timestamp);
            }
        }
        return affic;
    }


    public Dictionary<Tuple<string, string>, DateTime> get_liste_amis_enattente(MySqlConnection conn, string player, bool trie_date)
    {
        //Requete pour recuperer identifiant du joueur qui envoie la demande
        string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_sender
        int player_id = conn.QueryFirstOrDefault<int>(query, new { Pseudo = player });
        Dictionary<Tuple<string, string>, DateTime> affic = new Dictionary<Tuple<string, string>, DateTime>();
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
                affic.Add(Tuple.Create(player, player_name_rec), timestamp);
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
            if (stat_ami == false)
            {
                affic.Add(Tuple.Create(player, player_name_rec), timestamp);
            }
        }
        return affic;
    }


    public int get_friend_count(MySqlConnection conn, string pseudo)
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
