using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;


class Amis{
    public int get_id(MySqlConnection conn,string pseudo){
        //Requete pour recuperer identifiant du joueur
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_joueur
        int id_joueur= conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        return id_joueur;
    }
    public string get_pseudo(MySqlConnection conn,int idplayer){
        //requete pour recuperer le pseudo d'un joueur
        string query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@IDJ";
        //pseudo_joueur
        string pseudo_joueur= conn.QueryFirstOrDefault<string>(query, new { IDJ=idplayer});
        return pseudo_joueur;
    }

    public void send_friend_request(MySqlConnection conn,int sender,int receiver){//envoyer une demande
        using(MySqlCommand command=new MySqlCommand()){
            string query="SELECT count(*) FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            int rowcount=conn.QueryFirstOrDefault<int>(query,new{IDS=receiver,IDR=sender});
            if(rowcount>0)return;
            command.Connection=conn;
            command.CommandText="INSERT INTO Amis (idUsers1,idUsers2,status_ami,date_amis) VALUES (@IDS,@IDR,@STA,@DA)";
            command.Parameters.AddWithValue("@IDS",sender);
            command.Parameters.AddWithValue("@IDR",receiver);
            command.Parameters.AddWithValue("@STA",false);
            command.Parameters.AddWithValue("@DA",DateTime.Now);
            rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while inserting");
            else Console.WriteLine("Success");
        }
    }
    public void accept_friend_request(MySqlConnection conn,int playeraccepte,int playersend){//accepter la demande
        using(MySqlCommand command=new MySqlCommand()){
            command.Connection=conn;
            command.CommandText="UPDATE Amis SET status_ami=@STA, date_amis=@DTE WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS",playersend);
            command.Parameters.AddWithValue("@IDR",playeraccepte);
            command.Parameters.AddWithValue("@STA",true);
            command.Parameters.AddWithValue("@DTE",DateTime.Now);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while updating");
            else Console.WriteLine("Success");
        }
    }
    public void refuse_friend_request(MySqlConnection conn,int playerrefuse,int playersender){//refuser la demande ou supprimer
        using(MySqlCommand command=new MySqlCommand()){
            command.Connection=conn;
            command.CommandText="DELETE FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS",playersender);
            command.Parameters.AddWithValue("@IDR",playerrefuse);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while deleting");
            else Console.WriteLine("Success");
        }
    }

public Dictionary<Tuple<int,string>,DateTime> get_liste_amis(MySqlConnection conn,int idPlayer){
        Dictionary<Tuple<int,string>,DateTime> affic= new Dictionary<Tuple<int,string>,DateTime>();
        string query="SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDF=idPlayer});
        foreach(dynamic row in data){
            int id_rec=row.idUsers2;
            if(id_rec==idPlayer)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID=id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami){
                affic.Add(Tuple.Create(id_rec,player_name_rec),timestamp);
            }
        }
        query="SELECT * FROM Amis WHERE idUsers2=@IDS";
        data=conn.Query(query,new{IDS=idPlayer});
        foreach(dynamic row in data){
            int id_rec=row.idUsers1;
            if(id_rec==idPlayer)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID= id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami){
                affic.Add(Tuple.Create(id_rec,player_name_rec),timestamp);
            }
        }
        return affic;
    }


public Dictionary<Tuple<int,string>,DateTime> get_liste_amis_enattente(MySqlConnection conn,int idPlayer){
        Dictionary<Tuple<int,string>,DateTime> affic= new Dictionary<Tuple<int,string>,DateTime>();
        string query="SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDF=idPlayer});
        foreach(dynamic row in data){
            int id_rec=row.idUsers2;
            if(id_rec==idPlayer)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID=id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami==false)
            {
                affic.Add(Tuple.Create(id_rec,player_name_rec),timestamp);
            }
        }
        query="SELECT * FROM Amis WHERE idUsers2=@IDS";
        data=conn.Query(query,new{IDS=idPlayer});
        foreach(dynamic row in data){
            int id_rec=row.idUsers1;
            if(id_rec==idPlayer)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID= id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami==false){
                affic.Add(Tuple.Create(id_rec,player_name_rec),timestamp);
            }
        }
        return affic;
    }
    public int get_friend_count(MySqlConnection conn,int id_player){
        int friend_count=0;
        //id=id_player
        string query="SELECT * FROM Amis WHERE idUsers1=@IDS OR idUsers2=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDS=id_player,IDF=id_player});
        foreach(dynamic row in data){
            if(row.status_ami==true)friend_count++;
        }
        return friend_count;
    }
}

/*
    public int get_id(MySqlConnection conn,string pseudo){
        //Requete pour recuperer identifiant du joueur
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //id=id_joueur
        int id_joueur= conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        return id_joueur;
    }
    public void send_friend_request(MySqlConnection conn,string sender,string receiver){//envoyer une demande
        //recuperer id_sender
        int id_sender=get_id(conn,sender);
        //recuperer id_reciever
        int id_receiver=get_id(conn,receiver);
        using(MySqlCommand command=new MySqlCommand()){
            string query="SELECT count(*) FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            int rowcount=conn.QueryFirstOrDefault<int>(query,new{IDS=id_receiver,IDR=id_sender});
            Console.WriteLine(rowcount);
            if(rowcount>0)return;
            command.Connection=conn;
            command.CommandText="INSERT INTO Amis (idUsers1,idUsers2,status_ami,date_amis) VALUES (@IDS,@IDR,@STA,@DA)";
            command.Parameters.AddWithValue("@IDS",id_sender);
            command.Parameters.AddWithValue("@IDR",id_receiver);
            command.Parameters.AddWithValue("@STA",false);
            command.Parameters.AddWithValue("@DA",DateTime.Now);
            rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while inserting");
            else Console.WriteLine("Success");
        }
    }
    public void accept_friend_request(MySqlConnection conn,string playeraccepte,string playersend){//accepter la demande
        //recuperer id_accepte
        int id_playeraccepte= get_id(conn,playeraccepte);
        //recuperer id du joueur qui envoie la demande
        int id_playersend=get_id(conn,playersend);
        Console.WriteLine(playeraccepte+" idplayeraccepte="+id_playeraccepte+" "+playersend+" idplayersend "+id_playersend);
        using(MySqlCommand command=new MySqlCommand()){
            command.Connection=conn;
            command.CommandText="UPDATE Amis SET status_ami=@STA, date_amis=@DTE WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS",id_playersend);
            command.Parameters.AddWithValue("@IDR",id_playeraccepte);
            command.Parameters.AddWithValue("@STA",true);
            command.Parameters.AddWithValue("@DTE",DateTime.Now);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while updating");
            else Console.WriteLine("Success");
        }
    }
    public void refuse_friend_request(MySqlConnection conn,string playerrefuse,string playersender){//refuser la demande ou supprimer
        //id=id_playerrefuse
        int id_playerrefuse=get_id(conn,playersender);
        //id=id_playersender
        int id_playersender=get_id(conn,playersender);
        using(MySqlCommand command=new MySqlCommand()){
            command.Connection=conn;
            command.CommandText="DELETE FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
            command.Parameters.AddWithValue("@IDS",id_playerrefuse);
            command.Parameters.AddWithValue("@IDR",id_playersender);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("Failed while deleting");
            else Console.WriteLine("Success");
        }
    }

    public Dictionary<Tuple<string,string>,DateTime> get_liste_amis(MySqlConnection conn,string player,bool trie_date){
        //id=id_sender
        int player_id= get_id(conn,player);
        Dictionary<Tuple<string,string>,DateTime> affic= new Dictionary<Tuple<string,string>,DateTime>();
        string query="";
        if(trie_date)query="SELECT * FROM Amis WHERE idUsers1=@IDF ORDER BY date_amis DESC";
        else query="SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDF=player_id});
        foreach(dynamic row in data){
            int id_rec=row.idUsers2;
            if(id_rec==player_id)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID=id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami){
                affic.Add(Tuple.Create(player,player_name_rec),timestamp);
            }
        }
        if(trie_date)query="SELECT * FROM Amis WHERE idUsers2=@IDS ORDER BY date_amis DESC";
        else query="SELECT * FROM Amis WHERE idUsers2=@IDS";
        data=conn.Query(query,new{IDS=player_id});
        foreach(dynamic row in data){
            int id_rec=row.idUsers1;
            if(id_rec==player_id)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID= id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami){
                affic.Add(Tuple.Create(player,player_name_rec),timestamp);
            }
        }
        return affic;
    }
        public Dictionary<Tuple<string,string>,DateTime> get_liste_amis_enattente(MySqlConnection conn,string player,bool trie_date){
        //id=player_id
        int player_id= get_id(conn,player);
        Dictionary<Tuple<string,string>,DateTime> affic= new Dictionary<Tuple<string,string>,DateTime>();
        string query="";
        if(trie_date)query="SELECT * FROM Amis WHERE idUsers1=@IDF ORDER BY date_amis DESC";
        else query="SELECT * FROM Amis WHERE idUsers1=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDF=player_id});
        foreach(dynamic row in data){
            int id_rec=row.idUsers2;
            if(id_rec==player_id)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID=id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami==false){
                affic.Add(Tuple.Create(player,player_name_rec),timestamp);
            }
        }
        if(trie_date)query="SELECT * FROM Amis WHERE idUsers2=@IDS ORDER BY date_amis DESC";
        else query="SELECT * FROM Amis WHERE idUsers2=@IDS";
        data=conn.Query(query,new{IDS=player_id});
        foreach(dynamic row in data){
            int id_rec=row.idUsers1;
            if(id_rec==player_id)continue;
            query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
            //id=id_sender
            string player_name_rec= conn.QueryFirstOrDefault<string>(query, new { ID= id_rec});
            if(player_name_rec==null)continue;
            bool stat_ami=row.status_ami;
            DateTime timestamp=row.date_amis;
            if(stat_ami==false){
                affic.Add(Tuple.Create(player,player_name_rec),timestamp);
            }
        }
        return affic;
    }
    public int get_friend_count(MySqlConnection conn,string pseudo){
        int friend_count=0;
        //id=id_player
        int id_player= get_id(conn,pseudo);

        string query="SELECT * FROM Amis WHERE idUsers1=@IDS OR idUsers2=@IDF";
        IEnumerable<dynamic> data=conn.Query(query,new{IDS=id_player,IDF=id_player});
        foreach(dynamic row in data){
            if(row.status_ami==true)friend_count++;
        }
        return friend_count;
    }

*/