using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System.IO;
using System;
using System.Text.RegularExpressions;
using Dapper;
using System.Linq;
public class Login : MonoBehaviour
{
    public string server;
    public string database;
    public string username;
    //public string password;
    public MySqlConnection connection;
    public InputField usernameInput;
    public InputField passwordInput;
    public Button loginButton;
    public Text statusBDD;
    // Start is called before the first frame update
    void Start()
    {
        loginButton.onClick.AddListener(login);
        try{
            string constr="Server=" + server + ";DATABASE=" + database + "; User ID=" + username + ";Password="+ ";Pooling=true;Charset=utf8;";
            connection=new MySqlConnection(constr);
            connection.Open();
            statusBDD.text="Connection Successful";
        }
        catch{
            statusBDD.text="Connection Failed";
        }
    }


    void login(){
        if(usernameInput.text.Length==0&&passwordInput.text.Length==0)statusBDD.text="Please enter your username and your password";
        else{
            //statusBDD.text="login attempt...";
            string user =usernameInput.text;
            string pass=passwordInput.text;
            login_user(user,pass);
        }
    }

    public int login_user(string pseudo,string motdepasse){
        string mdp_hash = connection.QueryFirstOrDefault<string>("SELECT motdepasse FROM Utilisateurs WHERE pseudo = @Pseudo", new { Pseudo= pseudo });
        if(mdp_hash==null||mdp_hash.Length==0){//Pseudo incorrect
            statusBDD.text="pseudo incorrect";
            return 1;
        }
        if(Login.Verifier_Mdp(motdepasse,mdp_hash)==false){//Motdepasse incorrect
            statusBDD.text="mot de passe incorrect";
            return 2;
            }
        statusBDD.text="login Successful";
        return 0;

    }
    public static bool Verifier_Mdp(string mdp,string hashed_mdp){
            bool res=BCrypt.Net.BCrypt.Verify(mdp,hashed_mdp);//Verification
            return res;
        }   
    // Update is called once per frame
    void Update()
    {
        
    }
}
