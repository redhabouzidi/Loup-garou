using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Collections.Generic;
class Inscription{
	public int inscription_user(MySqlConnection conn,string pseudo,string email,string motdepasse){
        	try{
            	if(motdepasse==null)return 1;//si le mot de passe est null
            	string res=Inscription.Hash_Mdp(motdepasse);//Hash mot de passe
            	if(res.Length==0||res==""){
                	Console.WriteLine("Couldnt hash the password");//si le chaine de caractere resultat res est vide || echec hash
                	return 1;
            	};
            	string query = "INSERT INTO Utilisateurs (pseudo,email,motdepasse) VALUES(@Pseudo,@Email,@Mdp)";

	            conn.Execute(query,new{Pseudo=pseudo,Email=email,Mdp=res});//Execution de la requete parametree

	            query="SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
        	    int id = conn.QueryFirstOrDefault<int>(query, new { Pseudo= pseudo });
        	    query = "INSERT INTO Statistiques (idUsers,score,nb_partiejoue,nb_victoire) VALUES(@IDU,@SC,@NBP,@NBV)";
            	conn.Execute(query,new{IDU=id,SC=0,NBP=0,NBV=0});//Execution de la requete parametree
            	return 0;
        	}
        	catch(Exception e){
            	if(e.Message.Contains("PRIMARY"))return 2;//Erreur clé primaire dupliqué
            	if(e.Message.Contains("email")||e.Message.Contains("EMAIL"))return 3;//Erreur email dupliqué ou email contrainte
            	if(e.Message.Contains("pseudo"))return 4;//Erreur pseudo dupliqué
            	return -1;
        }
    }

    public static string Hash_Mdp(string mdp)
        {
            string regx=@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])[a-zA-Z0-9!@#\$%\^&\*]{8,512}$";//Expression Reguliere
            if(Regex.IsMatch(mdp, regx)){//Verification: mot de passe correspond bien a l'expression reguliere
                string hashed_mdp=BCrypt.Net.BCrypt.HashPassword(mdp,BCrypt.Net.BCrypt.GenerateSalt(12));
                return hashed_mdp;
            }
            else{//s'il ne correspond pas a l'expression reguliere, retour chaine de caractere vide: ""
                Console.WriteLine("Constraint CHECK MOTDEPASSE violated");
                return "";
            }
        }

}
