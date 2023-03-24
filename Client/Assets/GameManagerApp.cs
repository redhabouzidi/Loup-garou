using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerApp : MonoBehaviour
{

    public Button buttonQuit, buttonLogin, buttonRegistration;
    public GameObject box_error, loginPage, registrationPage, waitPage;
    public static List<player> players;
    public TMP_InputField inputFConnEmail, inputFConnPassword;
    public TMP_InputField inputFRegEmail, inputFRegPseudo, inputFRegPassword, inputFRegConfirmPassword;

    // Start is called before the first frame update
    void Start()
    {
        GameManagerApp.players = new List<player>();

        buttonQuit.onClick.AddListener(OnButtonClickQuit);
        buttonLogin.onClick.AddListener(OnButtonClickConnection);
        buttonRegistration.onClick.AddListener(OnButtonClickRegistration);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Stop play mode in the editor
            exitGame();
            // Quit the game
        }
    }
    
    public static void exitGame(){
        NetworkManager.prog=false;
        #if UNITY_EDITOR
                    // Stop play mode in the editor
                    UnityEditor.EditorApplication.isPlaying = false;
            #else
                    // Quit the game
                    Application.Quit();
            #endif
    }

    public struct player
    {
        public int id;
        public string name;
        public player(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    private void OnButtonClickQuit()
    {
        exitGame();
        //Application.Quit();
    }

    private void OnButtonClickConnection()
    {
        bool isSuccess = true;
        string email = inputFConnEmail.text;
        string password = inputFConnPassword.text;

        // hash password avant
        NetworkManager.login(NetworkManager.client, email, password);
        //isSuccess = ??
        /*if (isSuccess){
            box_error.SetActive(false);
            loginPage.SetActive(false);
            waitPage.SetActive(true);
        }
        else{
            AfficheError("Error: Email/Pseudo or password is invalide");
        }*/
    }

    private void OnButtonClickRegistration()
    {
        bool isSuccess = true;
        string email = inputFRegEmail.text;
        string pseudo = inputFRegPseudo.text;
        string password = inputFRegPassword.text;
        string password2 = inputFRegConfirmPassword.text;

        if (password == password2)
        {
            NetworkManager.sendInscription(NetworkManager.client, pseudo, password, email);
            //isSuccess = retour du serveur / bdd
            /*if (isSuccess){
                box_error.SetActive(false);
                registrationPage.SetActive(false);
                loginPage.SetActive(true);
            }
            else {
                AfficheError("Error: Dire ce qu'il va pas");
            }*/
        }
        else {
            AfficheError("Error: the password is not the same");
        }
    }
    
    public void AfficheError (string msg){
        box_error.SetActive(true);
        TextMeshProUGUI text_error = box_error.transform.Find("Text_error").GetComponent<TextMeshProUGUI>();
        text_error.text = msg;
    }
}