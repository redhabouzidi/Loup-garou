using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;

class Statistique{
    //Fonction permettant d'incrementer le nombre de partie joué d'un utilisateur et écrire sur BDD
    public void nouveau_jeu(MySqlConnection conn,string pseudo){//nouvelle partie entrée
        //la requete pour recuperer l'identifiant de l'utilisateur de pseudo = 'pseudo' passé en parametre
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //Execution de la requete  (QueryFirstOrDefault: permet de recuperer le premier resultat de la requete)
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });//id->
        using(MySqlCommand command=new MySqlCommand()){
            //requete pour recuperer le nombre de partie joué pour l'utilisateur avec l'identifiant ='id'
            query="SELECT nb_partiejoue FROM Statistiques WHERE idUsers=@ID";
            //Execution de la requete
            int nb_partiejoue=conn.QueryFirstOrDefault<int>(query,new{Id=id});
            //incrementation du nombre de partie joué
            nb_partiejoue++;
            //Attribuer la connection BDD à la connection de la commande
            command.Connection=conn;
            //la requete pour mettre à jour le nombre de partie joué
            command.CommandText="UPDATE Statistiques SET nb_partiejoue=@NB WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@NB", nb_partiejoue);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            //si aucune ligne est modifiée
            if(rowcount==0)Console.WriteLine("An error occured while updating table");
            //si succes
            else Console.WriteLine("Success");
        }
        return;
    }

    //Fonction permettant d'incrementer le score d'un utilisateur et écrire sur BDD
    public void score_gain(MySqlConnection conn,string pseudo,int score_to_add){//augmenter le score
        //la requete pour recuperer l'identifiant de l'utilisateur de pseudo = 'pseudo' passé en parametre
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //Execution de la requete  (QueryFirstOrDefault: permet de recuperer le premier resultat de la requete)
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        using(MySqlCommand command=new MySqlCommand()){
            //requete pour recuperer le score pour l'utilisateur avec l'identifiant ='id'
            query="SELECT score FROM Statistiques WHERE idUsers=@ID";
            int score=conn.QueryFirstOrDefault<int>(query,new{Id=id});
            //incrementation du score
            score+=score_to_add;
            //Attribuer la connection BDD à la connection de la commande
            command.Connection=conn;
            //la requete pour mettre à jour le score
            command.CommandText="UPDATE Statistiques SET score=@SC WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@SC", score);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            //si aucune ligne est modifiée
            if(rowcount==0)Console.WriteLine("An error occured while updating table");
            //si succes
            else Console.WriteLine("Success");
        }
        return;
    }
    //Fonction permettant d'incrementer le nombre du jeu gagné d'un utilisateur et écrire sur BDD
    public void jeu_gagne(MySqlConnection conn,string pseudo){//jeu gagné
        //la requete pour recuperer l'identifiant de l'utilisateur de pseudo = 'pseudo' passé en parametre
        string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        //Execution de la requete  (QueryFirstOrDefault: permet de recuperer le premier resultat de la requete)
        int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        using(MySqlCommand command=new MySqlCommand()){
            //requete pour recuperer le score pour l'utilisateur avec l'identifiant ='id'
            query="SELECT nb_victoire FROM Statistiques WHERE idUsers=@ID";
            int nb_victoire=conn.QueryFirstOrDefault<int>(query,new{Id=id});
            nb_victoire++;
            command.Connection=conn;
            //la requete pour mettre à jour le score
            command.CommandText="UPDATE Statistiques SET nb_victoire=@NB WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@NB", nb_victoire);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            //si aucune ligne est modifiée
            if(rowcount==0)Console.WriteLine("An error occured while updating table");
            //si succes
            else Console.WriteLine("Success");
        }
        return;
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de victoire (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Nombre de Victoire')
    public (int[],string[]) Trierpar_nbvictoires(MySqlConnection conn){
        //La requete pour recuperer l'identifiant le nombre de victoire du joueur (trié)
        string query="SELECT idUsers,nb_victoire FROM Statistiques ORDER BY nb_victoire DESC;";
        //Variable pour voir le nombre de joueur
        int nbplayer=0;
        //Liste pour des identifiants de joueurs
        List<int> ids=new List<int>();
        //Liste pour des nombres de victoires de joueurs
        List<int> nbvic=new List<int>();
        MySqlCommand command=new MySqlCommand(query,conn);
        //MySqlDataReader pour lire les données une par une, pour recuperer ses donées
        MySqlDataReader reader=command.ExecuteReader();
        while(reader.Read()){
            int tmpid=(int)reader[0];//identifiant d'un joueur
            int tmpnbvic=(int)reader[1];//nombre de victoire d'un joueur
            ids.Add(tmpid);//stocker l'identifiant au liste correspondant
            nbvic.Add(tmpnbvic);//stocker le nombre victoire au liste correspondant
            nbplayer++; //incrementer le nombre de joueur
        }
        reader.Close();
        if(nbplayer>10)nbplayer=10;//si nombre de joueurs est superieur a 10,nbplayer=10 (pour recuperer les 10 meilleurs)
        int[] returnnbvic=new int[nbplayer];
        string[] returnpseudos=new string[nbplayer];
        for(int i=0;i<nbplayer;i++){//First 10 players
            //Recuperer les pseudos des joueurs avec des identifiants de la table ids
            query="SELECT pseudo FROM Utilisateurs WHERE idusers=@ID";
            string pseudo = conn.QueryFirstOrDefault<string>(query, new {ID=ids[i]});
            //Ajouter à la dictionnaire les données
            //affic.Add(pseudo,nbvic[i]);
            returnnbvic[i]=nbvic[i];
            returnpseudos[i]=pseudo;
        }
        return (returnnbvic,returnpseudos);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de score (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public (int[],string[]) Trierpar_score(MySqlConnection conn){
        //La requete pour recuperer l'identifiant le nombre de victoire du joueur (trié)
        string query="SELECT idUsers,score FROM Statistiques ORDER BY score DESC;";
        //Variable pour voir le nombre de joueur
        int nbplayer=0;
        //Liste pour des identifiants de joueurs
        List<int> ids=new List<int>();
        //Liste pour des nombres de victoires de joueurs
        List<int> scores=new List<int>();
        MySqlCommand command=new MySqlCommand(query,conn);
        //MySqlDataReader pour lire les données une par une, pour recuperer ses donées
        MySqlDataReader reader=command.ExecuteReader();
        while(reader.Read()){
            int tmpid=(int)reader[0];//identifiant d'un joueur
            int tmpscore=(int)reader[1];//nombre de victoire d'un joueur
            ids.Add(tmpid);//stocker l'identifiant au liste correspondant
            scores.Add(tmpscore);//stocker le score au liste correspondant
            nbplayer++;//incrementer le nombre de joueur
        }
        reader.Close();
        //si nombre de joueurs est superieur a 10,nbplayer=10 (pour recuperer les 10 meilleurs)
        if(nbplayer>10)nbplayer=10;
        int[] returnscore=new int[nbplayer];
        string[] returnpseudos=new string[nbplayer];
        for(int i=0;i<nbplayer;i++){//First 10 players
            //Recuperer les pseudos des joueurs avec des identifiants de la table ids
            query="SELECT pseudo FROM Utilisateurs WHERE idusers=@ID";
            string pseudo = conn.QueryFirstOrDefault<string>(query, new {ID=ids[i]});
            //Ajouter à la dictionnaire les données
            returnscore[i]=scores[i];
            returnpseudos[i]=pseudo;
        }
        return (returnscore,returnpseudos);
    }
    //Fonction permettant de recuperer les joueurs qui ont plus de nombre de parties joué (Trié) et retourner un Dictionnaire qui stocke les données comme (Key='Pseudo':Value='Score')
    public (int[],string[]) Trierpar_nbjoues(MySqlConnection conn){
        //La requete pour recuperer l'identifiant le nombre de victoire du joueur (trié)
        string query="SELECT idUsers,nb_partiejoue FROM Statistiques ORDER BY score DESC;";
        //Variable pour voir le nombre de joueur
        int nbplayer=0;
        //Liste pour des identifiants de joueurs
        List<int> ids=new List<int>();
        //Liste pour des nombres de victoires de joueurs
        List<int> nbjoues=new List<int>();
        MySqlCommand command=new MySqlCommand(query,conn);
        //MySqlDataReader pour lire les données une par une, pour recuperer ses donées
        MySqlDataReader reader=command.ExecuteReader();
        while(reader.Read()){
            int tmpid=(int)reader[0];//identifiant d'un joueur
            int tmpscore=(int)reader[1];//nombre de victoire d'un joueur
            ids.Add(tmpid);//stocker l'identifiant au liste correspondant
            nbjoues.Add(tmpscore);//stocker le nombre de partie joué au liste correspondant
            nbplayer++;//incrementer le nombre de joueur
        }
        reader.Close();
        //si nombre de joueurs est superieur a 10,nbplayer=10 (pour recuperer les 10 meilleurs)
        if(nbplayer>10)nbplayer=10;
        int[] returnnbjoue=new int[nbplayer];
        string[] returnpseudos=new string[nbplayer];
        for(int i=0;i<nbplayer;i++){//First 10 players
            //Recuperer les pseudos des joueurs avec des identifiants de la table ids
            query="SELECT pseudo FROM Utilisateurs WHERE idusers=@ID";
            string pseudo = conn.QueryFirstOrDefault<string>(query, new {ID=ids[i]});
            //Ajouter à la dictionnaire les données
            returnnbjoue[i]=nbjoues[i];
            returnpseudos[i]=pseudo;
        }
        return (returnnbjoue,returnpseudos);
    }

}