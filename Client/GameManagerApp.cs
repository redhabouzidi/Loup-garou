using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerApp : MonoBehaviour
{

    public Button buttonQuit;
    public Button buttonLogin;
    public Button buttonRegistration;

    public static List<player> players;

    public TMP_InputField inputFConnEmail;
    public TMP_InputField inputFConnPassword;

    public TMP_InputField inputFRegEmail;
    public TMP_InputField inputFRegPseudo;
    public TMP_InputField inputFRegPassword;
    public TMP_InputField inputFRegConfirmPassword;

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
            Application.Quit();
        }
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
        Application.Quit();
    }

    private void OnButtonClickConnection()
    {
        // connexion au serveur
        string email = inputFConnEmail.text;
        string password = inputFConnPassword.text;

        // hash password avant
        NetworkManager.login(NetworkManager.client, email, password);
    }

    private void OnButtonClickRegistration()
    {
        string email = inputFRegEmail.text;
        string pseudo = inputFRegPseudo.text;
        string password = inputFRegPassword.text;
        string password2 = inputFRegConfirmPassword.text;

        if (password == password2)
        {
            NetworkManager.sendInscription(NetworkManager.client, pseudo, password, email);
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}