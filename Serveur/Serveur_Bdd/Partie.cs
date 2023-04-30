using MySql.Data.MySqlClient;
using Dapper;

class Partie{

    public static int create_partie(MySqlConnection conn,string nompartie){
        string query = "INSERT INTO Partie (nomPartie,Datesauvegarde) VALUES(@NP,@DT)";

        conn.Execute(query,new{NP=nompartie,DT=DateTime.Now});//Execution de la requete parametree
        query = "SELECT LAST_INSERT_ID()";
        return conn.QueryFirstOrDefault<int>(query);
    }

    public static void init_sauvegardepartie(MySqlConnection conn,int id_partie,int id_user,int score){
        string query = "INSERT INTO SauvegardePartie (idPartie,idUsers,scoregained) VALUES(@IDP,@IDU,@SCO)";

        conn.Execute(query,new{IDP=id_partie,IDU=id_user,SCO=score});//Execution de la requete parametree
    }
    public static void write_action(MySqlConnection conn,string action,int id_partie){
        string query = "INSERT INTO Actions (idPartie,actions) VALUES(@ID,@ACT)";

        conn.Execute(query,new{ID=id_partie,ACT=action});//Execution de la requete parametree
    }
    public (string,DateTime) get_village_name(MySqlConnection conn,int id_partie){
        string query="SELECT nomPartie,Datesauvegarde FROM Partie WHERE idPartie=@id";
        (string village_name,DateTime datesauv) = conn.QueryFirstOrDefault<(string,DateTime)>(query,new{id=id_partie});
        return (village_name,datesauv);
    }
    public static string get_action(MySqlConnection conn,int id,int id_partie){
        string query="SELECT count(*) FROM SauvegardePartie WHERE idUsers=@idU AND idPartie=@idP";
        int rowcount=conn.QueryFirstOrDefault<int>(query,new{idU=id,idP=id_partie});
        if(rowcount>0){
        query="SELECT actions FROM Actions WHERE idPartie=@id";
        string act_all=conn.QueryFirstOrDefault<string>(query,new{id=id_partie});   
        return act_all;
        }
        return null;
    }
    public static (int[],string[],DateTime[],int[]) get_partie(MySqlConnection conn,int idUser){
        string query="SELECT SauvegardePartie.idPartie,Partie.nomPartie,Partie.Datesauvegarde,SauvegardePartie.scoregained FROM SauvegardePartie join Partie on SauvegardePartie.idPartie = Partie.idPartie WHERE idUsers=@id";
        List<(int id,string names,DateTime date,int score)> data=conn.Query<(int,string,DateTime,int)>(query,new{id=idUser}).ToList();
        int[] identifiants = data.Select(r => r.id).ToArray();
        string[] names = data.Select(r => r.names).ToArray();
        DateTime[] dates = data.Select(r=> r.date).ToArray();
        int[] score= data.Select(r=> r.score).ToArray();
        return (identifiants,names,dates,score);
    }
}
