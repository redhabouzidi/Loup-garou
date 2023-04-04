using BCrypt.Net;
using Dapper;
using MySql.Data.MySqlClient;
namespace dataBase
{
    class Login
    {
        public static int login_user(MySqlConnection conn, string pseudo, string motdepasse)
        {
            string mdp_hash = conn.QueryFirstOrDefault<string>("SELECT motdepasse FROM Utilisateurs WHERE pseudo = @Pseudo", new { Pseudo = pseudo });
            if (mdp_hash == null || mdp_hash.Length == 0)
            {
                return 1;//Pseudo incorrect
            }

            if (Login.Verifier_Mdp(motdepasse, mdp_hash))
            {
                return 0;//Motdepasse correct
            }

            return 2;

        }
        public static int get_id(MySqlConnection conn, string pseudo)
        {
            //Requete pour recuperer identifiant du joueur
            string query = "SELECT idUsers FROM Utilisateurs WHERE pseudo=@Pseudo";
            //id=id_joueur
            int id_joueur = conn.QueryFirstOrDefault<int>(query, new { Pseudo = pseudo });
            return id_joueur;
        }

        //fonction permettant de verifier le mot de passe entré 'mdp' par utilisateur
        public static bool Verifier_Mdp(string mdp, string hashed_mdp)
        {
            bool res = BCrypt.Net.BCrypt.Verify(mdp, hashed_mdp);//Verification
            if (res)
                Console.WriteLine("Mot de passe est corrrect.");
            else
                Console.WriteLine("Mot de passe est incorrect");
            return res;
        }



    }
}