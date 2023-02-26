using BCrypt.Net;
using Dapper;
using MySql.Data.MySqlClient;
class Login{

    public int login_user(MySqlConnection conn,string pseudo,string motdepasse){
        conn.Open();
        string mdp_hash = conn.QueryFirstOrDefault<string>("SELECT motdepasse FROM Utilisateurs WHERE pseudo = @Pseudo", new { Pseudo= pseudo });
        if(mdp_hash==null||mdp_hash.Length==0)return 1;//Pseudo incorrect
        if(Login.Verifier_Mdp(motdepasse,mdp_hash))return 0;//Motdepasse incorrect
        return 2;

    }


    //fonction permettant de verifier le mot de passe entré 'mdp' par utilisateur
    public static bool Verifier_Mdp(string mdp,string hashed_mdp){
            bool res=BCrypt.Net.BCrypt.Verify(mdp,hashed_mdp);//Verification
            if(res)
                Console.WriteLine("Mot de passe est corrrect.");
            else
                Console.WriteLine("Mot de passe est incorrect");
            return res;
        }   



}