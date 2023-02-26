using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Security;


//CONNECTION A LA BASE DE DE DONNEES

public class DataConnection : MonoBehaviour
{
    public string host;
    public string username;
    public string password;
    public string database;
    public Text TxtState;
    MySqlConnection con;

    public InputField IfLogin;
    public InputField IfEmail;
    public InputField IfPassword;
    public Text TxtLogin;

    void ConnectionBDD()
    {
        string constr = "Server=" + host + ";DATABASE=" + database + "; User ID=" + username + ";Password=" + password + ";Pooling=true;Charset=utf8;"; 
        try
        {
            con = new MySqlConnection(constr);
            con.Open();
            TxtState.text = con.State.ToString();
        }
        catch(IOException e)
        {
            TxtState.text = e.ToString();
        }
    }

    //fermeture de la connection
    void Fermeture ()
    {
        TxtState.text = ("Connextion fermee");
        if (con != null && con.State.ToString() != "Closed")
            con.Close();
    }

    public static string Hash_Mdp(string mdp)
    {
        string regx = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])[a-zA-Z0-9!@#\$%\^&\*]{8,512}$";//Expression Reguliere
        if (Regex.IsMatch(mdp, regx))
        {//Verification: mot de passe correspond bien a l'expression reguliere
            string hashed_mdp = BCrypt.Net.BCrypt.HashPassword(mdp, BCrypt.Net.BCrypt.GenerateSalt(12));
            return hashed_mdp;
        }
        else
        {//s'il ne correspond pas a l'expression reguliere, retour chaine de caractere vide: ""
            Console.WriteLine("Constraint CHECK MOTDEPASSE violated");
            return "";
        }
    }

    public void Inscription()
    {
        ConnectionBDD();

        //Verificaation de l'existenc du pseudo
        bool exist = false;
        MySqlCommand nouveauEnvoi = new MySqlCommand("SELECT pseudo FROM Utilisateurs WHERE pseudo= '" + IfLogin.text + "'", con);
        MySqlDataReader lecture = nouveauEnvoi.ExecuteReader();

        while (lecture.Read())
        {
            if (lecture["pseudo"].ToString() != "")
            {
                TxtLogin.text = "pseudo exist deja";
                exist = true;
            }
        }
        lecture.Close();

        if (!exist)
        {
            if(IfPassword.text==null)return 1;//si le mot de passe est null
            string res=Inscription.Hash_Mdp(IfPassword.text);//Hash mot de passe  ou IfPassword.text.ToString()?
            if(res.Length==0||res==""){
                TxtLogin.text =TxtLogin+ "Please enter a new password";
                //Console.WriteLine("Couldnt hash the password");//si le chaine de caractere resultat res est vide || echec hash
                //return 1;
            }
            string insertion = "INSERT INTO Utilisateurs VALUES (default, '" + IfLogin.text + "', '" + IfEmail.text + "', '" + res +"')";
            MySqlCommand envoie = new MySqlCommand(insertion, con);

            try
            {
                envoie.ExecuteReader();
                TxtLogin.text = "inscription reussie";
            }
            catch (IOException e)
            {
                TxtState.text = e.ToString();
            }

            envoie.Dispose();
            con.Close();
        }
    }

    //Connexion d'un utilisateur à la base 
    public void Connection()
    {
        ConnectionBDD();
        string mdp;

        try
        {
            MySqlCommand pseudoConnexion = new MySqlCommand("SELECT pseudo FROM Utilisateurs WHERE pseudo= '" + IfLogin.text + "'", con);
            MySqlDataReader pseudoLecture = pseudoConnexion.ExecuteReader();

            while (pseudoLecture.Read())
            {
                mdp = pseudoLecture["motDePasse"].ToString();
                if (mdp == Hash_Mdp(IfPassword.text))
                    TxtLogin.text = "bienvenue";
                else
                    TxtLogin.text = "pseudo/password incorrect";
            }
            pseudoLecture.Close();
        }
        catch (IOException e) { TxtLogin.text = e.ToString(); }
        con.Close();
    }
}
