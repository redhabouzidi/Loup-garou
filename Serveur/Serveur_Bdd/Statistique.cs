using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;

class Statistique{
    //Fonction permettant d'incrementer le nombre de partie joué d'un utilisateur et écrire sur BDD
    public static void nouveau_jeu(MySqlConnection conn,int[]identifiants){//nouvelle partie entrée
        try{
        using(MySqlCommand command=new MySqlCommand()){
            //Attribuer la connection BDD à la connection de la commande
            command.Connection=conn;
                Console.WriteLine("on entre dans le test");

            foreach(int id in identifiants){
                Console.WriteLine("on entre dans le test");
                string query="SELECT count(*) FROM Statistiques WHERE idUsers=@ID";
                int rowcount=conn.QueryFirstOrDefault<int>(query,new{ID=id});
                if(rowcount==0){
                    query="INSERT INTO Statistiques (idUsers,score,nb_partiejoue,nb_victoire) VALUES (@idu,@sco,@nbp,@nbv)";
                    conn.Execute(query,new{idu=id,sco=0,nbp=0,nbv=0});
                }
                //requete pour recuperer le nombre de partie joué pour l'utilisateur avec l'identifiant ='id'
                query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID";
                //Execution de la requete
                int nb_partiejoue=conn.QueryFirstOrDefault<int>(query,new{Id=id});
                //incrementation du nombre de partie joué
                nb_partiejoue++;
                //la requete pour mettre à jour le nombre de partie joué
                command.CommandText="UPDATE Statistiques SET nb_partiejoue=@NB WHERE idUsers=@ID";
                command.Parameters.AddWithValue("@NB", nb_partiejoue);
                command.Parameters.AddWithValue("@ID",id);
                rowcount=command.ExecuteNonQuery();
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
    public static void score_gain(MySqlConnection conn,int []identifiants,int[] score_to_add){//augmenter le score
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
    public static void jeu_gagne(MySqlConnection conn,int [] identifiants,bool [] win){//jeu gagné
        try{
        using(MySqlCommand command=new MySqlCommand()){
            for(int i=0;i<identifiants.Length;i++){
                if(win[i]){
                //requete pour recuperer le score pour l'utilisateur avec l'identifiant ='id'
                string query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID";
                int nb_victoire=conn.QueryFirstOrDefault<int>(query,new{Id=identifiants[i]});
                nb_victoire++;
                command.Connection=conn;
                //la requete pour mettre à jour le score
                command.CommandText="UPDATE Statistiques SET nb_victoire=@NB WHERE idUsers=@ID";
                command.Parameters.AddWithValue("@NB", nb_victoire);
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
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
        return;
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de victoire (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Nombre de Victoire')
    public static (int[],string[]) Trierpar_nbvictoires(MySqlConnection conn){
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
    public static (int,string) nbvictoire_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le nombre de victoire du joueur (trié par nombre de victoire)
        string query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID;";
        int nb_vic = conn.QueryFirstOrDefault<int>(query,new{ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (nb_vic,pseudo);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de score (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public static (int[],string[]) Trierpar_score(MySqlConnection conn){
        string query="SELECT count(*) FROM Statistiques";
        int rowcount=conn.QueryFirstOrDefault<int>(query);
        if(rowcount==0)return (null,null);
        //La requete pour recuperer l'identifiant le score des 50 joueurs (trié)
        query="SELECT idUsers,score FROM Statistiques ORDER BY score DESC LIMIT 50;";
        List<(int id, int score)> data = conn.Query<(int, int)>(query).ToList();
        int[] identifiants = data.Select(r => r.id).ToArray();
        int[] score = data.Select(r => r.score).ToArray();
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[]pseudos = new string[50];
        pseudos=conn.Query<string>(query,new{IDS=identifiants}).ToArray();
        return (score,pseudos);
    }
    public static (int,string) score_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le score du joueur (trié par score)
        string query="SELECT score FROM Statistiques WHERE idUsers=@ID;";
        int score = conn.QueryFirstOrDefault<int>(query,new{ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (score,pseudo);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de nombre de parties joué (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public static (int[],string[]) Trierpar_nbjoues(MySqlConnection conn){
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
    public static (int,string) nbjoues_joueur(MySqlConnection conn,int id){
        //La requete pour recuperer l'identifiant le nombre de parties joues du joueur (trié)
        string query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID;";
        int nb_joues = conn.QueryFirstOrDefault<int>(query,new {ID=id});
        query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@ID";
        string pseudo=conn.QueryFirstOrDefault<string>(query,new{ID=id});
        return (nb_joues,pseudo);
    }
	//retourne pseudos,idusers,score,nb_partiejoue,nb_victoire
	public (string[],int[],int[],int[],double[]) Get_all(MySqlConnection conn,int id,int trier){//trier 0-> par score, trier 1->nb_partiejoue, trier 2->winrate, trier 3->pseudo
        //La requete pour recuperer l'identifiant,le nombre de parties joues des 50 joueurs (trié par nombre parties joues)
string query = "SELECT idUsers, score,nb_partiejoue,nb_victoire " +
               "FROM Statistiques " +
               "WHERE idUsers IN (SELECT idUsers1 FROM Amis WHERE idUsers2 = @idu AND status_ami = @SA) " +
               "UNION " +
               "SELECT idUsers, score,nb_partiejoue,nb_victoire " +
               "FROM Statistiques " +
               "WHERE idUsers IN (SELECT idUsers2 FROM Amis WHERE idUsers1 = @idu AND status_ami = @SA) "+
               "UNION "+
               "SELECT idUsers, score,nb_partiejoue,nb_victoire FROM Statistiques WHERE idUsers=@idu";
    List<(int idUsers, int score,int nb_partiejoue,int nb_victoire)> data = conn.Query<(int, int,int,int)>(query, new { idu = id, SA = true }).ToList();
    

        int[] identifiants = data.Select(r => r.idUsers).ToArray();
        int[] score=data.Select(r=>r.score).ToArray();
        int[] nb_partiejoue=data.Select(r=>r.nb_partiejoue).ToArray();
        int[] nb_victoire=data.Select(r=>r.nb_victoire).ToArray();
        double[] winrates=new double[nb_victoire.Length];
        for(int i=0;i<nb_victoire.Length;i++){
            winrates[i]=(nb_victoire[i]/(double)nb_partiejoue[i])*100;
        }

        query = "SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[] pseudos = conn.Query<string>(query, new { IDS = identifiants }).ToArray();
        List<(string pseudo, int id, int score, int nb_partiejoue, double winrate)> dataList = new List<(string, int, int, int, double)>();
        for (int i = 0; i < pseudos.Length; i++)
{
    dataList.Add((pseudos[i], identifiants[i], score[i], nb_partiejoue[i], winrates[i]));
}
    if(trier==0){
        dataList = dataList.OrderByDescending(d => d.score)
                   .ThenByDescending(d => d.winrate)
                   .ThenByDescending(d => d.nb_partiejoue)
                   .ToList();
    }
    else if(trier==1){
        dataList = dataList.OrderByDescending(d => d.nb_partiejoue)
                   .ThenByDescending(d => d.winrate)
                   .ThenByDescending(d => d.score)
                   .ToList();
    }
    else if(trier==2){
        dataList = dataList.OrderByDescending(d => d.winrate)
                   .ThenByDescending(d => d.score)
                   .ThenByDescending(d => d.nb_partiejoue)
                   .ToList();
    }
    else if(trier==3){
        dataList = dataList.OrderBy(d => d.pseudo)
                   .ThenByDescending(d => d.winrate)
                   .ThenByDescending(d => d.score)
                   .ThenByDescending(d => d.nb_partiejoue)
                   .ToList();
    }
        return (dataList.Select(r => r.pseudo).ToArray(),dataList.Select(r => r.id).ToArray(),dataList.Select(r => r.score).ToArray(),dataList.Select(r => r.nb_partiejoue).ToArray(),dataList.Select(r => r.winrate).ToArray());
    }
}