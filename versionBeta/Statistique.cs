using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;

class Statistique{
    //Fonction permettant d'incrementer le nombre de partie joué d'un utilisateur et écrire sur BDD
    public void nouveau_jeu(MySqlConnection conn,int[]identifiants){//nouvelle partie entrée
        try{
        using(MySqlCommand command=new MySqlCommand()){
            //Attribuer la connection BDD à la connection de la commande
            command.Connection=conn;
            foreach(int id in identifiants){
                //requete pour recuperer le nombre de partie joué pour l'utilisateur avec l'identifiant ='id'
                string query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID";
                //Execution de la requete
                int nb_partiejoue=conn.QueryFirstOrDefault<int>(query,new{Id=id});
                //incrementation du nombre de partie joué
                nb_partiejoue++;
                //la requete pour mettre à jour le nombre de partie joué
                command.CommandText="UPDATE Statistiques SET nb_partiejoue=@NB WHERE idUsers=@ID";
                command.Parameters.AddWithValue("@NB", nb_partiejoue);
                command.Parameters.AddWithValue("@ID",id);
                int rowcount=command.ExecuteNonQuery();
                command.Parameters.Clear();
                //si aucune ligne est modifiée
                if(rowcount==0)Console.WriteLine("An error occured while updating table");
                //si succes
                else Console.WriteLine("Success");
            }
        }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
        return;
    }

    //Fonction permettant d'incrementer le score d'un utilisateur et écrire sur BDD
    public void score_gain(MySqlConnection conn,int []identifiants,int[] score_to_add){//augmenter le score
        try{
        using(MySqlCommand command=new MySqlCommand()){
            //Attribuer la connection BDD à la connection de la commande
            command.Connection=conn;
            for(int i=0;i<identifiants.Length;i++){     
                //requete pour recuperer le score pour l'utilisateur avec l'identifiant ='id'
                string query="SELECT score FROM Statistiques WHERE idUsers=@ID";
                int score=conn.QueryFirstOrDefault<int>(query,new{Id=identifiants[i]});
                //incrementation du score
                score+=score_to_add[i];
                //la requete pour mettre à jour le score
                command.CommandText="UPDATE Statistiques SET score=@SC WHERE idUsers=@ID";
                command.Parameters.AddWithValue("@SC", score);
                command.Parameters.AddWithValue("@ID",identifiants[i]);
                int rowcount=command.ExecuteNonQuery();
                command.Parameters.Clear();
                //si aucune ligne est modifiée
                if(rowcount==0)Console.WriteLine("An error occured while updating table");
                //si succes
                else Console.WriteLine("Success");
            }
        }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
        return;
    }
    //Fonction permettant d'incrementer le nombre du jeu gagné d'un utilisateur et écrire sur BDD
    public void jeu_gagne(MySqlConnection conn,int [] identifiants){//jeu gagné
        try{
        using(MySqlCommand command=new MySqlCommand()){
            foreach(int id in identifiants){
                //requete pour recuperer le score pour l'utilisateur avec l'identifiant ='id'
                string query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID";
                int nb_victoire=conn.QueryFirstOrDefault<int>(query,new{Id=id});
                nb_victoire++;
                command.Connection=conn;
                //la requete pour mettre à jour le score
                command.CommandText="UPDATE Statistiques SET nb_victoire=@NB WHERE idUsers=@ID";
                command.Parameters.AddWithValue("@NB", nb_victoire);
                command.Parameters.AddWithValue("@ID",id);
                int rowcount=command.ExecuteNonQuery();
                command.Parameters.Clear();
                //si aucune ligne est modifiée
                if(rowcount==0)Console.WriteLine("An error occured while updating table");
                //si succes
                else Console.WriteLine("Success");
            }
        }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
        return;
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de victoire (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Nombre de Victoire')
    public (int[],string[]) Trierpar_nbvictoires(MySqlConnection conn){
        //La requete pour recuperer l'identifiant le nombre de victoire des 50 joueurs (trié)
        string query="SELECT idUsers,nb_victoire FROM Statistiques ORDER BY nb_victoire DESC LIMIT 50;";
        List<(int id, int nb_vic)> data = conn.Query<(int, int)>(query).ToList();
        int[] identifiants = data.Select(r => r.id).ToArray();
        int[] nb_vic = data.Select(r => r.nb_vic).ToArray();
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[]pseudos = new string[50];
        pseudos=conn.Query<string>(query,new{IDS=identifiants}).ToArray();
        return (nb_vic,pseudos);
    }
    public (int,string) nbvictoire_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le nombre de victoire du joueur (trié par nombre de victoire)
        string query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID;";
        int nb_vic = conn.QueryFirstOrDefault<int>(query,new{ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (nb_vic,pseudo);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de score (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public (int[],string[]) Trierpar_score(MySqlConnection conn){
        //La requete pour recuperer l'identifiant le score des 50 joueurs (trié)
        string query="SELECT idUsers,score FROM Statistiques ORDER BY score DESC LIMIT 50;";
        List<(int id, int score)> data = conn.Query<(int, int)>(query).ToList();
        int[] identifiants = data.Select(r => r.id).ToArray();
        int[] score = data.Select(r => r.score).ToArray();
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[]pseudos = new string[50];
        pseudos=conn.Query<string>(query,new{IDS=identifiants}).ToArray();
        return (score,pseudos);
    }
    public (int,string) score_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le score du joueur (trié par score)
        string query="SELECT score FROM Statistiques WHERE idUsers=@ID;";
        int score = conn.QueryFirstOrDefault<int>(query,new{ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (score,pseudo);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de nombre de parties joué (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public (int[],string[]) Trierpar_nbjoues(MySqlConnection conn){
        //La requete pour recuperer l'identifiant,le nombre de parties joues des 50 joueurs (trié par nombre parties joues)
        string query="SELECT idUsers,nb_partiejoue FROM Statistiques ORDER BY nb_partiejoue DESC LIMIT 50;";
        List<(int id, int nb_joues)> data = conn.Query<(int, int)>(query).ToList();
        int[] identifiants = data.Select(r => r.id).ToArray();
        int[] nb_joues = data.Select(r => r.nb_joues).ToArray();
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[]pseudos = new string[50];
        pseudos=conn.Query<string>(query,new{IDS=identifiants}).ToArray();
        return (nb_joues,pseudos);
    }
    public (int,string) nbjoues_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le nombre de parties joues du joueur (trié)
        string query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID;";
        int nb_joues = conn.QueryFirstOrDefault<int>(query,new {ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (nb_joues,pseudo);
    }
}