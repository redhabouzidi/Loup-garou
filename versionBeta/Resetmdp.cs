using MySql.Data.MySqlClient;
using MySql.Data;
using System.Text.RegularExpressions;
using System;
using Dapper;
using System.Linq;
using System.Net.Mail;
using System.Collections.Generic;
using BCrypt.Net;
class Resetmdp{
public bool ResetPassword(MySqlConnection conn,string email)
{
    // Recherche du compte utilisateur
    int user_count=0;
    string query="SELECT count(*) FROM Utilisateurs WHERE email=@EM";
    user_count=conn.Execute(query,new{EM=email});
    if(user_count==0)return false;
    // Génération d'un nouveau mot de passe aléatoire
    string newPassword = GenerateRandomPassword(12);

    // Stockage du nouveau mot de passe haché dans la base de données

    string hashed_mdp=Resetmdp.Hash_Mdp(newPassword);
    using(MySqlCommand command=new MySqlCommand()){
        command.Connection=conn;
        //la requete pour mettre à jour le nombre de partie joué
        command.CommandText="UPDATE Utilisateurs SET motdepasse=@MDP WHERE email=@EM";
        command.Parameters.AddWithValue("@MDP",hashed_mdp);
        command.Parameters.AddWithValue("@EM",email);
        int rowcount=command.ExecuteNonQuery();
        //si aucune ligne est modifiée
        if(rowcount==0)Console.WriteLine("An error occured while updating table");
        //si succes
        else Console.WriteLine("Success");
        }
    // Envoi du nouveau mot de passe par e-mail (optionnel)
    SendPasswordResetEmail(email,newPassword);

    return true;
}
    public static string GenerateRandomPassword(int length) {
        const string regexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])[a-zA-Z0-9!@#\$%\^&\*]{8,512}$";

        string password = "";
        Random random = new Random();

        while (!Regex.IsMatch(password, regexPattern)) {
            password = "";
            for (int i = 0; i < length; i++) {
                int randInt = random.Next(0, 4);
                if (randInt == 0) {
                    password += (char)random.Next(65, 91); // uppercase letters
                } else if (randInt == 1) {
                    password += (char)random.Next(97, 123); // lowercase letters
                } else if (randInt == 2) {
                    password += (char)random.Next(48, 58); // numbers
                } else {
                    password += "!@#$%^&*"[random.Next(0, 8)]; // special characters
                }
            }
        }

        return password;
    }
    public int changement_mdp(MySqlConnection conn,int id,string oldmdp,string new_mdp){
        string query="SELECT motdepasse FROM Utilisateurs WHERE idUsers=@ID";
        string mdp_joueur= conn.QueryFirstOrDefault<string>(query, new { ID= id});
        bool mdptrue=Resetmdp.Verifier_Mdp(oldmdp,mdp_joueur);
        if(mdptrue==false)return 1;
        string res=Inscription.Hash_Mdp(new_mdp);//Hash mot de passe
        if(res.Length==0||res==""){
            Console.WriteLine("Couldnt hash the password");//si le chaine de caractere resultat res est vide || echec hash
            return 2;
        }
        using(MySqlCommand command=new MySqlCommand()){
            command.Connection=conn;
            //la requete pour mettre à jour le nombre de partie joué
            command.CommandText="UPDATE Utilisateurs SET motdepasse=@MDP WHERE idUsers=@ID";
            command.Parameters.AddWithValue("@MDP",res);
            command.Parameters.AddWithValue("@ID",id);
            int rowcount=command.ExecuteNonQuery();
            //si aucune ligne est modifiée
            if(rowcount==0)return 3;
            //si succes
            else return 0;
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
    public static bool Verifier_Mdp(string mdp,string hashed_mdp){
            bool res=BCrypt.Net.BCrypt.Verify(mdp,hashed_mdp);//Verification
            return res;
        }  
    private void SendPasswordResetEmail(string email, string newPassword)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.To.Add(email);
            mailMessage.From = new MailAddress("loupgarougameunistra67@gmail.com", "LoupGarouGame");
            mailMessage.Subject = "Password reset request";
            mailMessage.Body = $"Hi!,Your new password is {newPassword}. Please use it to login and reset your password.";
            SmtpClient smtpClient = new SmtpClient("smtp-relay.sendinblue.com",587);
            smtpClient.Credentials = new System.Net.NetworkCredential("loupgarougameunistra67@gmail.com", "xsmtpsib-4b83fbe2ae299db5e4f3c591ac5771c10d111cec6ea74947a0d174731843a274-9BRbKGjVkIqUZ7yD");
            smtpClient.EnableSsl = true;
            smtpClient.Send(mailMessage);
        }
}