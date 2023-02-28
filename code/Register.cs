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
//using MySql.Data.MySqlClient;
public class Register : MonoBehaviour
{
    public string server;
    public string database;
    public string username;
    //public string password;
    public MySqlConnection connection;
    public InputField usernameInput;
    public InputField passwordInput;
    public InputField emailInput;
    public Button registerButton;
    public Text statusBDD;
    // Start is called before the first frame update
    void Start()
    {
        registerButton.onClick.AddListener(register);
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
    
    void register(){
        Dictionary<string,object> param=new Dictionary<string, object>{};
        if(emailInput.text.Length==0&&usernameInput.text.Length==0&&passwordInput.text.Length==0)statusBDD.text="Please enter all the fields";
        else{
        param["email"]=emailInput.text;
        param["pseudo"]=usernameInput.text;
        param["motdepasse"]=passwordInput.text;
        inscription_user(param);
        }
    }


    public int inscription_user(Dictionary<string,object> qparams){
         try{
            string res=Register.Hash_Mdp(qparams["motdepasse"].ToString());//Hash mot de passe
            if(res.Length==0||res==""){
                Console.WriteLine("Couldnt hash the password");//si le chaine de caractere resultat res est vide || echec hash
                return 1;
            };
            qparams["motdepasse"]=res;
            string query = $"INSERT INTO Utilisateurs ({string.Join(", ", qparams.Keys)}) VALUES ({string.Join(", ", qparams.Keys.Select(k => "@" + k))})";
            connection.Execute(query,qparams);//Execution de la requete parametree
            statusBDD.text="Query successfuly executed";
            return 0;
        }
        catch(Exception e){
            statusBDD.text=e.Message;
            if(e.Message.Contains("PRIMARY"))return 2;//Erreur clé primaire dupliqué
            if(e.Message.Contains("email"))return 3;//Erreur email dupliqué
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
                return "";
            }
        }
    // Update is called once per frame
    void Update()
    {
        
    }
}
