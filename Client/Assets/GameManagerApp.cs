using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerApp : MonoBehaviour
{

    public TextMeshProUGUI profileUsername;
    public Button buttonQuit, buttonQuit2, buttonLogin, buttonRegistration, 
    buttonPublic, buttonJoin, buttonAdd, buttonAccept, buttonSendForgotPass, buttonChangeForgotPass,buttonQuitLobby;
    public GameObject box_error, loginPage, registrationPage, waitPage;
    public static List<player> players;
    public TMP_InputField inputFConnEmail, inputFConnPassword;
    public TMP_InputField inputFRegEmail, inputFRegPseudo, inputFRegPassword, inputFRegConfirmPassword;
    public TMP_InputField inputEmailForgotPass, inputCodeForgotPass, inputPassForgotPass, inputPassForgotPass2;
    public static List<Game> listGame = new List<Game>();
    public GameObject containerGame, componentGame, toggleGroupGame;


    // friend
    public GameObject GO_add_research, containerFriend, containerAdd, containerRequest, containerWait;
    public GameObject componentAddWait, componentRequest, componentFriend, componentNo;
    public List<Friend> listFriend, listAdd, listRequest, listWait;
    public static int scene;

    // profile
    //NetworkManager
    public static Socket client=null;
    // Start is called before the first frame update
    void Start()
    {
        if (client != null)
        {
            NetworkManager.client = client;
        }
        else
        {
        NetworkManager.rep = new List<byte[]>();

        }
        NetworkManager.canvas = GameObject.Find("Canvas");
        NetworkManager.ho = NetworkManager.canvas.transform.Find("Home").gameObject;
        NetworkManager.cpo = NetworkManager.canvas.transform.Find("ConnectionPage").gameObject;
        NetworkManager.sp = NetworkManager.canvas.transform.Find("StartPage").gameObject;
        NetworkManager.wso = NetworkManager.canvas.transform.Find("WaitingScreen").gameObject;
        NetworkManager.lo = NetworkManager.canvas.transform.Find("Lobby").gameObject;
        NetworkManager.gmao = GameObject.Find("GameManagerApp").gameObject;
        NetworkManager.gma = NetworkManager.gmao.GetComponent<GameManagerApp>();
        NetworkManager.ws = NetworkManager.wso.GetComponent<WaitingScreen>();


        GameManagerApp.players = new List<player>();
        listFriend = new List<Friend>();
        listAdd = new List<Friend>();
        listRequest = new List<Friend>();
        listWait = new List<Friend>();
        Button buttonResearch = GO_add_research.transform.Find("Button-research").GetComponent<Button>();
        profileUsername.text = inputFRegPseudo.text;


        buttonResearch.onClick.AddListener(OnButtonClickResearch);
        buttonQuit.onClick.AddListener(OnButtonClickQuit);
        buttonQuit2.onClick.AddListener(OnButtonClickQuit);
        buttonLogin.onClick.AddListener(OnButtonClickConnection);
        buttonRegistration.onClick.AddListener(OnButtonClickRegistration);
        buttonPublic.onClick.AddListener(OnButtonClickPublic);
        buttonJoin.onClick.AddListener(OnButtonClickJoin);
        buttonAdd.onClick.AddListener(onButtonClickAdd);
        buttonChangeForgotPass.onClick.AddListener(onButtonClickChangeForgotPass);
        buttonSendForgotPass.onClick.AddListener(onButtonClickSendForgotPass);
        buttonAccept.onClick.AddListener(onButtonClickAccept);
        buttonQuitLobby.onClick.AddListener(onButtonClickQuitLobby);

        if (scene == 1)
        {
            scene = 0;
            Transform temp = GameObject.Find("Canvas").transform;
            temp.Find("Home").gameObject.SetActive(true);
            Debug.Log("scene 1");
        }else
        if (scene == 2)
        {
            scene = 0;
            NetworkManager.sendRequestGames();
            Transform temp = GameObject.Find("Canvas").transform;
            temp.Find("Home").gameObject.SetActive(true);
            Debug.Log("scene 2");

        }
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
        NetworkManager.listener();
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
    public void onButtonClickQuitLobby()
    {
        NetworkManager.sendQuitLobbyMessage();
    }
    private void OnButtonClickConnection()
    {
        box_error.SetActive(false);
        string email = inputFConnEmail.text;
        string password = inputFConnPassword.text;
        NetworkManager.task = Task.Run(() =>
        {
            NetworkManager.reseau(email,password);
        });

        // hash password avant
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
        box_error.SetActive(false);
        string email = inputFRegEmail.text;
        string pseudo = inputFRegPseudo.text;
        string password = inputFRegPassword.text;
        string password2 = inputFRegConfirmPassword.text;

        if (password == password2)
        {
            NetworkManager.sendInscription( pseudo, password, email);
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
        NetworkManager.sendRequestGames();
        Debug.Log("join should work");

    }

    private void OnButtonClickJoin()
    {
        NetworkManager.join(GetIdToggleGameOn(), NetworkManager.id, NetworkManager.username);
    }
    private void onButtonClickAdd()
    {
        NetworkManager.ajoutAmi(NetworkManager.id,"demonow");
    }
    private void onButtonClickAccept()
    {
        NetworkManager.reponseAmi( NetworkManager.id, 4,true);
    }
    private void OnButtonClickResearch()
    {
        TMP_InputField input_research = GO_add_research.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        string pseudo = input_research.text;

        // appel fonction pour la requete
    }
    private void onButtonClickSendForgotPass()
    {
        string email = inputEmailForgotPass.text;

    }
    private void onButtonClickChangeForgotPass()
    {
        string code = inputCodeForgotPass.text;
        string pass = inputPassForgotPass.text;
        string pass2 = inputPassForgotPass2.text;

        if(pass == pass2){
            // envoyer 
        }
        else{
            AfficheError("Your password is not the same.");
        }
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

    public void addFriendAdd(string name, int id){
        GameObject newFriend = Instantiate(componentAddWait, containerAdd.transform);
        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(true);
        button_cancel.SetActive(false);

        Friend f = new Friend(id, newFriend);
        listAdd.Add(f);
    }

    public void addFriendWait(string name, int id){
        GameObject newFriend = Instantiate(componentAddWait, containerWait.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(false);
        button_cancel.SetActive(true);

        Friend f = new Friend(id, newFriend);
        listWait.Add(f);
    }

    // status:
        // 3 = in game
        // 2 = in lobby/waitscreen
        // 1 = connecté
        // 0 = invitation en attente amis
        // -1 = hors ligne
    public void addFriend(string name, int status, int id){
        GameObject newFriend = Instantiate(componentFriend, containerFriend.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        Image imgStatus = newFriend.transform.Find("Image_status").GetComponent<Image>();
        TextMeshProUGUI textStatus = newFriend.transform.Find("Text_status").GetComponent<TextMeshProUGUI>();

        GameObject GO_buttonJoin = newFriend.transform.Find("Button-join").gameObject;
        GO_buttonJoin.SetActive(false);
        

        Friend f = new Friend(id, status, newFriend);

        switch(status){
            case 2:
                GO_buttonJoin.SetActive(true);
                imgStatus.color = new Color32(79,200,74,100);
                textStatus.text = "Online";
                break;
            case 1:
                imgStatus.color = new Color32(79,200,74,100);
                textStatus.text = "Online";
                break;
            case 3:
                imgStatus.color = new Color32(74,156,200,100);
                textStatus.text = "In Game";
                break;
            default:
                imgStatus.color = new Color32(128,128,128,100);
                textStatus.text = "Offline";
                break;

        }

        listFriend.Add(f);
    }

    public void addFriendRequest(string name, int id){
        GameObject newFriend = Instantiate(componentRequest, containerRequest.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        Friend f =new Friend(id, newFriend);
        listRequest.Add(f);
    }

    public void addNoFriend (string msg, GameObject container, List<GameObject> list){
        GameObject newObject = Instantiate(componentNo, container.transform);

        TextMeshProUGUI textName = newObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        textName.text = msg;
        
        list.Add(newObject);
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

[System.Serializable]
public class Friend
{
    public int id;
    public int status;
    public GameObject obj;

    public Friend(){}

    public Friend(int id, int status, GameObject obj)
    {
        this.id = id;
        this.status = status;
        this.obj = obj;
    }

    public Friend(int id, GameObject obj)
    {
        this.id = id;
        this.obj = obj;
    }

}