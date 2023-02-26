using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using Dapper;
using MySql.Data.MySqlClient;



class Inscription{
    //Fonction inscription d'un utilisateur 
    //Parametres: Connection MySQL conn, les données de l'utilisateur dans une dictionnaire qparams
    public int inscription_user(MySqlConnection conn,Dictionary<string,object> qparams){
         try{
            string res=Inscription.Hash_Mdp(qparams["motdepasse"].ToString());//Hash mot de passe
            //si le chaine de caractere resultat res est vide || echec hash
            if(res.Length==0||res=="")
            {
                Console.WriteLine("Couldnt hash the password");
                return 1;
            }
            qparams["motdepasse"]=res;
            string query = $"INSERT INTO Utilisateurs ({string.Join(", ", qparams.Keys)}) VALUES ({string.Join(", ", qparams.Keys.Select(k => "@" + k))})";
            conn.Execute(query,qparams);//Execution de la requete parametre
            return 0;
        }
        catch(Exception e){
            Console.WriteLine("Erreur due a: "+ e);
            if(e.Message.Contains("PRIMARY"))return 2;//Erreur clé primaire dupliqué
            if(e.Message.Contains("email"))return 3;//Erreur email dupliqué
            if(e.Message.Contains("pseudo"))return 4;//Erreur pseudo dupliqué
            return -1;
        }
    }
    //Fonction pour mettre a jour le mot de passe
    //Parametres: connection MySQL conn, le nouveau mot de passe new_mdp et id de l'utilisateur dont le mot de passe va être modifié
    public int mettreajour_mdp(MySqlConnection conn,string new_mdp,int id){
        string res=Inscription.Hash_Mdp(new_mdp);//Hash mot de passe
        if(res.Length==0||res=="")return 1;//Echec hash
        try{
            string query=$"UPDATE Utilisateurs SET motdepasse=@motdepasse WHERE idUsers=@idUsers";
            conn.Query(query,new{motdepasse=res,idUsers=id});
            return 0;//Execution de la requête parametree
        }
        catch(Exception e){
            Console.WriteLine("Erreur due a : "+ e.Message);//Si une erreur se produit
            return 1;
        }
    }
    public static string Hash_Mdp(string mdp)
        {
            string regx=@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])[a-zA-Z0-9!@#\$%\^&\*]{8,512}$";//Expression Reguliere
            
            //Verification: mot de passe correspond bien a l'expression reguliere
            if(Regex.IsMatch(mdp, regx)){
                string hashed_mdp=BCrypt.Net.BCrypt.HashPassword(mdp,BCrypt.Net.BCrypt.GenerateSalt(12));
                return hashed_mdp;
            }
            //s'il ne correspond pas a l'expression reguliere, retour chaine de caractere vide: ""
            else
            {
                Console.WriteLine("Constraint CHECK MOTDEPASSE violated");
                return "";
            }
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
