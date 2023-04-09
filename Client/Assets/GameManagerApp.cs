using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerApp : MonoBehaviour
{

    public Button buttonQuit, buttonQuit2, buttonLogin, buttonRegistration, buttonPublic, buttonJoin,buttonAdd,buttonAccept;
    public GameObject box_error, loginPage, registrationPage, waitPage;
    public static List<player> players;
    public TMP_InputField inputFConnEmail, inputFConnPassword;
    public TMP_InputField inputFRegEmail, inputFRegPseudo, inputFRegPassword, inputFRegConfirmPassword;
    public static List<Game> listGame = new List<Game>();
    public GameObject containerGame, componentGame, toggleGroupGame;

    // friend
    public GameObject GO_add_research, containerFriend, containerAdd, containerRequest, containerWait;
    public GameObject componentAddWait, componentRequest, componentFriend;
    public List<GameObject> listFriend, listAdd, listRequest, listWait;

    // Start is called before the first frame update
    void Start()
    {
        GameManagerApp.players = new List<player>();
        listFriend = new List<GameObject>();
        listAdd = new List<GameObject>();
        listRequest = new List<GameObject>();
        listWait = new List<GameObject>();
        Button buttonResearch = GO_add_research.transform.Find("Button-research").GetComponent<Button>();

        buttonResearch.onClick.AddListener(OnButtonClickResearch);
        buttonQuit.onClick.AddListener(OnButtonClickQuit);
        buttonQuit2.onClick.AddListener(OnButtonClickQuit);
        buttonLogin.onClick.AddListener(OnButtonClickConnection);
        buttonRegistration.onClick.AddListener(OnButtonClickRegistration);
        buttonPublic.onClick.AddListener(OnButtonClickPublic);
        buttonJoin.onClick.AddListener(OnButtonClickJoin);
        buttonAdd.onClick.AddListener(onButtonClickAdd);
        buttonAccept.onClick.AddListener(onButtonClickAccept);
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

    public static void exitGame()
    {
        NetworkManager.prog = false;
#if UNITY_EDITOR
                                // Stop play mode in the editor
                                UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the game
        Application.Quit();
#endif

    }

    private void OnButtonClickQuit()
    {
        exitGame();
        //Application.Quit();
    }

    private void OnButtonClickConnection()
    {
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
        else
        {
            AfficheError("Error: the password is not the same");
        }
    }

    private void OnButtonClickPublic()
    {
        NetworkManager.sendRequestGames(NetworkManager.client);
    }

    private void OnButtonClickJoin()
    {
        NetworkManager.join(NetworkManager.client, GetIdToggleGameOn(), NetworkManager.id, NetworkManager.username);
    }
    private void onButtonClickAdd()
    {
        NetworkManager.ajoutAmi(NetworkManager.client,NetworkManager.id,"demonow");
    }
    private void onButtonClickAccept()
    {
        NetworkManager.reponseAmi(NetworkManager.client, NetworkManager.id, 4,true);
    }
    private void OnButtonClickResearch()
    {
        TMP_InputField input_research = GO_add_research.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        string pseudo = input_research.text;

        // appel fonction pour la requete
    }

    public void AfficheError(string msg)
    {
        box_error.SetActive(true);
        TextMeshProUGUI text_error = box_error.transform.Find("Text_error").GetComponent<TextMeshProUGUI>();
        text_error.text = msg;
    }

    public void AddGame(int id, string name, int nbPlayer){
        GameObject newGame = Instantiate(componentGame, containerGame.transform);
        Game g = new Game(id, name, nbPlayer, newGame);

        TextMeshProUGUI textName = newGame.transform.Find("Text-village").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        GameObject GO_Player = newGame.transform.Find("numberPlayers").gameObject;
        TextMeshProUGUI textPlayer = GO_Player.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        textPlayer.text = "" + (nbPlayer - g.nbPlayer_rest) + "/"+ nbPlayer;

        //ajout dans le toggle groupe
        newGame.GetComponent<Toggle>().group = toggleGroupGame.GetComponent<ToggleGroup>();

        listGame.Add(g);
    }

    public void UpdateGame(int id){
        int indice = findIndiceGameId(id);
        if(indice != -1){
            GameObject GO_Player = listGame[indice].game.transform.Find("numberPlayers").gameObject;
            TextMeshProUGUI textPlayer = GO_Player.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            textPlayer.text = "" + (listGame[indice].nbPlayer - listGame[indice].nbPlayer_rest) + "/"+ listGame[indice].nbPlayer;
        }
    }

    public int GetIdToggleGameOn(){
        for(int i=0 ; i<listGame.Count ; i++){
            if(listGame[i].game.GetComponent<Toggle>().isOn){
                return listGame[i].id;
            }
        }
        return -1;
    }

    public int findIndiceGameId(int id){
        for(int i=0; i<listGame.Count; i++){
            if(listGame[i].id == id){
                return i;
            }
        }
        return -1;
    }

    public void addFriendAdd(string name){
        GameObject newFriend = Instantiate(componentAddWait, containerAdd.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(true);
        button_cancel.SetActive(false);

        listAdd.Add(newFriend);
    }

    public void addFriendWait(string name){
        GameObject newFriend = Instantiate(componentAddWait, containerWait.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(false);
        button_cancel.SetActive(true);

        listWait.Add(newFriend);
    }

    public void addFriend(string name){
        GameObject newFriend = Instantiate(componentFriend, containerFriend.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        listFriend.Add(newFriend);
    }

    public void addFriendRequest(string name){
        GameObject newFriend = Instantiate(componentRequest, containerRequest.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        listRequest.Add(newFriend);
    }

    public void ClearListGameObject(List<GameObject> list){
        foreach (GameObject obj in list){
            Destroy(obj);
        }
        list.Clear();
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
}

[System.Serializable]
public class Game
    {
        public int id;
        public string name;
        public int nbPlayer;
        public int nbPlayer_rest;
        public GameObject game;

        public Game(int id, string name, int nbPlayer, GameObject game)
        {
            this.id = id;
            this.name = name;
            this.nbPlayer = nbPlayer;
            this.game = game;
            nbPlayer_rest = nbPlayer;
        }
    }