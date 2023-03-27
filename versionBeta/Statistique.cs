using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;

class Statistique{
    
    public void cmtPartieJoue(MySqlConnection conn,string pseudo) //incremente la partie jouée par l'uid
    {
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        Console.WriteLine("id: "+id);
        using(MySqlCommand command=new MySqlCommand()){
            query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID";
            int nb_partiejoue=conn.QueryFirstOrDefault<int>(query,new{Id=id});
            Console.WriteLine("nbpartie: "+nb_partiejoue);
            nb_partiejoue++;
            Console.WriteLine("nbpartie apres un jeu: "+nb_partiejoue);
            command.Connection=conn;
            command.CommandText="UPDATE Statistiques SET nb_partiejoue=@NB WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@NB", nb_partiejoue);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)
                Console.WriteLine("erreur de mise en jour : partie non ajoutée");
            else Console.WriteLine("Success");
        }
        return;
    }

    public void score_gain(MySqlConnection conn,string pseudo,int score_to_add){//augmenter le score
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        using(MySqlCommand command=new MySqlCommand()){
            query="SELECT score FROM Statistiques WHERE idUsers=@ID";
            int score=conn.QueryFirstOrDefault<int>(query,new{Id=id});
            score+=score_to_add;
            command.Connection=conn;
            command.CommandText="UPDATE Statistiques SET score=@SC WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@SC", score);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)
                Console.WriteLine("probleme d'ajout du score");
            else Console.WriteLine("Success");
        }
        return;
    }

    public void victoire(MySqlConnection conn,string pseudo)
    {
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        using(MySqlCommand command=new MySqlCommand()){
            query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID";
            int nb_victoire=conn.QueryFirstOrDefault<int>(query,new{ID=id});
            nb_victoire++;
            command.Connection=conn;
            command.CommandText="UPDATE Statistiques SET nb_victoire=@NB WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@NB", nb_victoire);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            if(rowcount==0)Console.WriteLine("An error occured while updating table");
            else Console.WriteLine("Success");
        }
        return;
    }


    public Dictionary<string,int> triParNbvictoires(MySqlConnection conn)
    {
        string query="SELECT idUsers,nb_victoire FROM Statistiques ORDER BY nb_victoire DESC;";
        int nbplayer=0;
        List<int> ids=new List<int>();
        List<int> nbvic=new List<int>();
        MySqlCommand command=new MySqlCommand(query,conn);
        MySqlDataReader reader=command.ExecuteReader();
        while(reader.Read()){
            int tmpid=(int)reader[0];
            int tmpnbvic=(int)reader[1];
            ids.Add(tmpid);
            nbvic.Add(tmpnbvic);
            nbplayer++;
        }
        reader.Close();
        Dictionary<string,int> affichage=new Dictionary<string, int>();
        if(nbplayer>10)
            nbplayer=10;
        for(int i=0;i<nbplayer;i++)//First 10 players
        {
            query="SELECT pseudo FROM Utilisateurs WHERE idusers=@ID";
            string pseudo = conn.QueryFirstOrDefault<string>(query, new {ID=ids[i]});
            affichage.Add(pseudo,nbvic[i]);
        }
        return affichage;
    }


    public Dictionary<string,int> triParScore(MySqlConnection conn)
    {
        string query="SELECT idUsers,score FROM Statistiques ORDER BY score DESC;";
        int nbplayer=0;
        List<int> ids=new List<int>();
        List<int> scores=new List<int>();
        MySqlCommand command=new MySqlCommand(query,conn);
        MySqlDataReader reader=command.ExecuteReader();
        while(reader.Read()){
            int tmpid=(int)reader[0];
            int tmpscore=(int)reader[1];
            ids.Add(tmpid);
            scores.Add(tmpscore);
            nbplayer++;
        }
        reader.Close();
        Dictionary<string,int> affichage=new Dictionary<string, int>();
        if(nbplayer>10)nbplayer=10;
        for(int i=0;i<nbplayer;i++){//First 10 players
            query="SELECT pseudo FROM Utilisateurs WHERE idusers=@ID";
            string pseudo = conn.QueryFirstOrDefault<string>(query, new {ID=ids[i]});
            affichage.Add(pseudo,scores[i]);
        }
        return affichage;
    }
}

