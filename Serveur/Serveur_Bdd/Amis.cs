using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;

class Amis
{

    public static int get_id(MySqlConnection conn,string pseudo){
        try{
            //Requete pour recuperer identifiant du joueur
            string query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
            //id=id_joueur
            int id_joueur= conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
            return id_joueur;
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
            return -1;
        }
    }

    public static string get_pseudo(MySqlConnection conn,int idplayer){
        try{
            //requete pour recuperer le pseudo d'un joueur
            string query="SELECT pseudo FROM Utilisateurs WHERE idUsers=@IDJ";
            //pseudo_joueur
            string pseudo_joueur= conn.QueryFirstOrDefault<string>(query, new { IDJ=idplayer});
            return pseudo_joueur;
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
            return "";
        }
    }



    public static void send_friend_request(MySqlConnection conn,int sender,int receiver){//envoyer une demande
        try{
            using(MySqlCommand command=new MySqlCommand()){
                //Attribuer la connection à la commande
                command.Connection=conn;
                //Inserer dans la BDD, Creation d'un lien amitié
                command.CommandText="INSERT INTO Amis (idUsers1,idUsers2,status_ami,date_amis) VALUES (@IDS,@IDR,@STA,@DA)";
                //Ajout des parametres
                command.Parameters.AddWithValue("@IDS",sender);
                command.Parameters.AddWithValue("@IDR",receiver);
                command.Parameters.AddWithValue("@STA",false);
                command.Parameters.AddWithValue("@DA",DateTime.Now);
                //Execution
                int rowcount=command.ExecuteNonQuery();
                //si rowcount (ligne inseré) = 0
                if(rowcount==0)Console.WriteLine("Failed while inserting");
                else Console.WriteLine("Success");
            }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
    }



    public static void accept_friend_request(MySqlConnection conn,int playeraccepte,int playersend){//accepter la demande
        try{
            using(MySqlCommand command=new MySqlCommand()){
                //Attribuer la connection à la commande
                command.Connection=conn;
                //Mis a jour dans la BDD, Mis a jour d'un lien amitié (changement de status_amis->True, date_amis->La date courant)
                command.CommandText="UPDATE Amis SET status_ami=@STA, date_amis=@DTE WHERE idUsers1=@IDS AND idUsers2=@IDR";
                //Ajout des parametres
                command.Parameters.AddWithValue("@IDS",playersend);
                command.Parameters.AddWithValue("@IDR",playeraccepte);
                command.Parameters.AddWithValue("@STA",true);
                command.Parameters.AddWithValue("@DTE",DateTime.Now);
                //Execution
                int rowcount=command.ExecuteNonQuery();
                //si rowcount (ligne mis a jour) = 0
                if(rowcount==0)Console.WriteLine("Failed while updating");
                else Console.WriteLine("Success");
            }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
            }
        }



    public static void refuse_friend_request(MySqlConnection conn,int playerrefuse,int playersender){//refuser la demande ou supprimer
        try{
            using(MySqlCommand command=new MySqlCommand()){
                //Attribuer la connection à la commande
                command.Connection=conn;
                //Supprimer le lien d'amitié
                command.CommandText="DELETE FROM Amis WHERE idUsers1=@IDS AND idUsers2=@IDR";
                //Ajout des parametres
                command.Parameters.AddWithValue("@IDS",playersender);
                command.Parameters.AddWithValue("@IDR",playerrefuse);
                //Execution
                int rowcount=command.ExecuteNonQuery();
                //si rowcount (ligne supprimé) = 0
                if(rowcount==0)Console.WriteLine("Failed while deleting");
                else Console.WriteLine("Success");
            }
        }
        catch(Exception e){
            Console.WriteLine(e.Message);
        }
    }



    public static (int[], string[], DateTime[]) get_liste_amis(MySqlConnection conn, int idPlayer)
    {
        try
        {
            //Recuperer les amis qui a reçu une demande de joueur idPlayer
            string query = "SELECT idUsers2,date_amis FROM Amis WHERE idUsers1=@IDF AND status_ami=@SA;";
            List<(int id, DateTime date)> data_f = conn.Query<(int, DateTime)>(query, new { IDF = idPlayer, SA = true }).ToList();
            //Recuperer les amis qui a envoyé une demande au joueur idPlayer
            query = "SELECT idUsers1,date_amis FROM Amis WHERE idUsers2=@IDF AND status_ami=@SA;";
            List<(int id, DateTime date)> data_s = conn.Query<(int, DateTime)>(query, new { IDF = idPlayer, SA = true }).ToList();
            //concatenation des listes reçu
            int[] identifiants = data_f.Select(r => r.id).Concat(data_s.Select(r => r.id)).ToArray();
            DateTime[] dates = data_f.Select(r => r.date).Concat(data_s.Select(r => r.date)).ToArray();
            //Recuperer les pseudos a partir du liste des identifiants
            query = "SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
            string[] pseudos = conn.Query<string>(query, new { IDS = identifiants }).ToArray();
            //retour
            return (identifiants, pseudos, dates);
        }
        catch (Exception e)
        {
            return (new int[0], new string[0], new DateTime[0]);
        }
    }

    public static (int[],string[],DateTime[]) get_liste_amis_enattente(MySqlConnection conn,int idPlayer){
        //Recuperer les amis qui a reçu une demande de joueur idPlayer
        string query="SELECT idUsers2,date_amis FROM Amis WHERE idUsers1=@IDF AND status_ami=@SA";
        List<(int id,DateTime date)> data_f=conn.Query<(int,DateTime)>(query,new{IDF=idPlayer,SA=false}).ToList();
        data_f.Add((-1,new DateTime()));
        //Recuperer les amis qui a envoyé une demande au joueur idPlayer
        query="SELECT idUsers2,date_amis FROM Amis WHERE idUsers2=@IDF AND status_ami=@SA";
        List<(int id,DateTime date)> data_s=conn.Query<(int,DateTime)>(query,new{IDF=idPlayer,SA=false}).ToList();
        //concatenation des listes reçu
        int[] identifiants = data_f.Select(r => r.id).Concat(data_s.Select(r=>r.id)).ToArray();
        DateTime[] dates = data_f.Select(r => r.date).Concat(data_s.Select(r=>r.date)).ToArray();
        //Recuperer les pseudos a partir du liste des identifiants
        query = "SELECT pseudo FROM Utilisateurs WHERE idUsers IN @IDS ORDER BY FIELD(idUsers, " + string.Join(",", identifiants) + ")";
        string[]temp=conn.Query<string>(query,new{IDS=identifiants}).ToArray();
        string[]pseudos = new string[temp.Length + 1];
        Array.Copy(temp, 0, pseudos, 0, temp.Length);
        pseudos[pseudos.Length - 1] = "";
        Console.WriteLine(pseudos.Length+" hey "+ identifiants.Length );
        //retour
        return (identifiants,pseudos,dates);
    }


   



    public static string[] search_for_player(MySqlConnection conn,string name_player){
        string query="SELECT pseudo FROM Utilisateurs WHERE pseudo LIKE @NP";
        string[] data=conn.Query<string>(query,new{NP = "%" + name_player + "%"}).ToArray();
        return data;
    }

    public static int get_friend_count(MySqlConnection conn,int id_player){
        int friend_count=0;
        //Recuperer le nombre d'amis d'un joueur
        //soit ses amis qui ont recu la demande de ce joueur
        //soit ses amis qui ont envoyé la demande a ce joueur
        string query="SELECT count(*) FROM Amis WHERE (idUsers1=@IDS OR idUsers2=@IDF) AND status_ami=@SA";
        friend_count=conn.QueryFirstOrDefault<int>(query,new{IDS=id_player,IDF=id_player,SA=true});
        return friend_count;
    }
}
