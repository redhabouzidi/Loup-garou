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
    public Button buttonQuit, buttonQuit2, buttonLogin, buttonRegistration, buttonPublic, 
    buttonJoin, buttonSendForgotPass, buttonChangeForgotPass,buttonQuitLobby,buttonLogout, buttonReady,buttonSaveGames,
    buttonRank,buttonRank2;
    public GameObject box_error, loginPage, registrationPage, waitPage;
    public static List<player> players;
    public TMP_InputField inputFConnEmail, inputFConnPassword;
    public TMP_InputField inputFRegEmail, inputFRegPseudo, inputFRegPassword, inputFRegConfirmPassword;
    public TMP_InputField inputEmailForgotPass, inputCodeForgotPass, inputPassForgotPass, inputPassForgotPass2;
    public static List<Game> listGame = new List<Game>();
    public GameObject containerGame, componentGame, toggleGroupGame;
    public static string email;

    // friend
    public GameObject GO_add_research, containerFriend, containerAdd, containerRequest, containerWait;
    public GameObject componentAddWait, componentRequest, componentFriend, componentNo;
    public static List<Friend> listFriend, listAdd, listRequest, listWait;
    public static int scene;

    // profile
    //NetworkManager
    public static Socket client=null;
    // Start is called before the first frame update

    //Waiting screen
    public Color colorGreen, colorWhite;
    public Image readyCheck;

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
        NetworkManager.sgo = NetworkManager.canvas.transform.Find("SavedGames").gameObject;
        NetworkManager.sg = NetworkManager.sgo.GetComponent<SavedGames>();
        NetworkManager.ro = NetworkManager.canvas.transform.Find("Ranking").gameObject;
        NetworkManager.r = NetworkManager.ro.GetComponent<rank>();
        NetworkManager.so = NetworkManager.canvas.transform.Find("Statistiques").gameObject;
        NetworkManager.s = NetworkManager.so.GetComponent<Statistiques>();



        GameManagerApp.players = new List<player>();
        if(listFriend==null)
        listFriend = new List<Friend>();
        if (listAdd == null)
            listAdd = new List<Friend>();
        if (listRequest == null)
            listRequest = new List<Friend>();
        if (listWait == null)
            listWait = new List<Friend>();
        Button buttonResearch = GO_add_research.transform.Find("Button-research").GetComponent<Button>();


        buttonResearch.onClick.AddListener(OnButtonClickResearch);
        buttonQuit.onClick.AddListener(OnButtonClickQuit);
        buttonQuit2.onClick.AddListener(OnButtonClickQuit);
        buttonLogin.onClick.AddListener(OnButtonClickConnection);
        buttonRegistration.onClick.AddListener(OnButtonClickRegistration);
        buttonPublic.onClick.AddListener(OnButtonClickPublic);
        buttonJoin.onClick.AddListener(OnButtonClickJoin);
        buttonChangeForgotPass.onClick.AddListener(onButtonClickChangeForgotPass);
        buttonSendForgotPass.onClick.AddListener(onButtonClickSendForgotPass);
        buttonQuitLobby.onClick.AddListener(onButtonClickQuitLobby);
        buttonLogout.onClick.AddListener(onButtonClickLogout);
        buttonSaveGames.onClick.AddListener(onButtonClickSaveGames);
        buttonRank.onClick.AddListener(onButtonClickRank);
        buttonRank2.onClick.AddListener(onButtonClickRank);
        
        refreshAll();
        NetworkManager.inGame = false;
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
        if (NetworkManager.username != "") profileUsername.text = NetworkManager.username;
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
        if(waitPage.active){
            
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

    /**
        Action executé lorsque le bouton logout est appuyé
        Permet de se deconnecter
    **/
    private void onButtonClickLogout()
    {
        NetworkManager.logout(NetworkManager.client);
    }

    /**
        Action executé lorsque le bouton quit est appuyé
        Permet de quitter l'application
    **/
    private void OnButtonClickQuit()
    {
        exitGame();
    }

    /**
        Action executé lorsque le bouton quit dans un waiting screen est appuyé
        Permet de quitter le waiting screen d'une partie
    **/
    public void onButtonClickQuitLobby()
    {
        NetworkManager.sendQuitLobbyMessage();
    }

    /**
        Action executé lorsque le bouton connection est appuyé
        Recupere les informations des input et les envoye au serveur
        pour verifier les infos et etablir la connection 
    **/
    private void OnButtonClickConnection()
    {
        box_error.SetActive(false);
        string email = inputFConnEmail.text;
        string password = inputFConnPassword.text;
        if (email != "") profileUsername.text = email;
        NetworkManager.task = Task.Run(() =>
        {
            NetworkManager.reseau(email,password);
        });
        Debug.Log(NetworkManager.client);
        
    }

    /**
        Action executé lorsque le bouton d'inscription est appuyé
        Recupere les informations des input et les envoye au serveur
        pour creer un compte au joueur 
    **/
    private void OnButtonClickRegistration()
    {

        box_error.SetActive(false);
        string email = inputFRegEmail.text;
        string pseudo = inputFRegPseudo.text;
        string password = inputFRegPassword.text;
        string password2 = inputFRegConfirmPassword.text;
        if (pseudo != "") profileUsername.text = pseudo;

        if (password == password2)
        {
            NetworkManager.reseau(pseudo, password, email);
        }
        else
        {
            AfficheError("Error: the password is not the same");
        }
        NetworkManager.recvMessage(NetworkManager.client);
    }

    /**
        Action executé lorsque le bouton des games est appuyé
        demande au serveur les parties crées
    **/
    private void OnButtonClickPublic()
    {
        NetworkManager.sendRequestGames();
        Debug.Log("join should work");

    }

    /**
        Action executé lorsque le bouton rejoindre est appuyé
        envoie au serveur que le joueur veut rejoindre la partie qu'il a selectionne
    **/
    private void OnButtonClickJoin()
    {
        NetworkManager.join(GetIdToggleGameOn());
    }

    /**
        Action executé lorsque le bouton de recherche est appuyé
        envoie au serveur l'input de recherche pour effectuer la recherche de joueurs
        commencant par par le texte de l'input 
    **/
    private void OnButtonClickResearch()
    {
        ClearListGameObject(listAdd);

        TMP_InputField input_research = GO_add_research.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
        string pseudo = input_research.text;
        // appel fonction pour la requete
        NetworkManager.sendSearchRequest(NetworkManager.id, pseudo);
    }

    /**
        Action executé lorsque le bouton envoyer l'email est appuyé
        pour changer son mot de passe lorsqu'on l'oublie
        envoie au serveur l'email donné par le joueur
    **/
    private void onButtonClickSendForgotPass()
    {
        email = inputEmailForgotPass.text;
        NetworkManager.reseau(email);

        byte [] message=new byte[1+sizeof(bool)];
        NetworkManager.recvMessage(NetworkManager.client);

    }

    /**
        
    **/
    private void onButtonClickChangeForgotPass()
    {
        string code = inputCodeForgotPass.text;
        string pass = inputPassForgotPass.text;
        string pass2 = inputPassForgotPass2.text;
        
        if(pass == pass2){
            NetworkManager.ResetPassw(email,code,pass);
        }
        else{
            AfficheError("Your password is not the same.");
        }
        NetworkManager.recvMessage(NetworkManager.client);
    }
    private void onButtonClickRank(){
        NetworkManager.sendRankRequest();
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
    private void onButtonClickSaveGames(){
        NetworkManager.sendHistoryRequest();
    }
    public void addFriendAdd(string name,int id){
        SupprNoObject(listAdd);

        GameObject newFriend = Instantiate(componentAddWait, containerAdd.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(true);
        button_cancel.SetActive(false);
        Button bAdd = button_add.GetComponent<Button>();
        bAdd.onClick.AddListener(() =>
        {
            NetworkManager.ajoutAmi(NetworkManager.id, id);
        });
        Button bCancel = button_cancel.GetComponent<Button>();
        bCancel.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, id, false);
        });
        Friend f = new Friend(id, name, newFriend);

        listAdd.Add(f);
    }

    public void addFriendWait(string name,int id){
        
        if (!NetworkManager.inGame)
        {
        GameObject newFriend = Instantiate(componentAddWait, containerWait.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(false);
        button_cancel.SetActive(true);

        Button bAdd = button_add.GetComponent<Button>();
        bAdd.onClick.AddListener(() =>
        {
            NetworkManager.ajoutAmi(NetworkManager.id, id);
        });
        Button bCancel = button_cancel.GetComponent<Button>();
        bCancel.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, id, false);
        });

        Friend f = new Friend(id, name, newFriend);
        listWait.Add(f);

        }
        else
        {
            Friend f = new Friend(id, name, null);
            listWait.Add(f);
        }


    }
    public void refreshAll()
    {
        if (listFriend != null && listFriend.Count != 0)
        {
            foreach (Friend f in listFriend)
            {
                refreshFriend(f);
            }
        }
        if (listRequest != null && listRequest.Count != 0)
        {
            foreach (Friend f in listRequest)
            {

                refreshFriendR(f);
            }
        }
        if (listWait != null && listWait.Count != 0)
        {
            foreach (Friend f in listWait)
            {

                refreshFriendW(f);
            }
        }
    }
    public void refreshFriend(Friend f)
    {
        GameObject newFriend = Instantiate(componentFriend, containerFriend.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = f.name;

        Image imgStatus = newFriend.transform.Find("Image_status").GetComponent<Image>();
        GameObject infoStatus = newFriend.transform.Find("Info_status").gameObject;
        TextMeshProUGUI textStatus = infoStatus.transform.Find("Text_status").GetComponent<TextMeshProUGUI>();
        infoStatus.SetActive(false);

        GameObject GO_buttonJoin = newFriend.transform.Find("Button-join").gameObject;
        GO_buttonJoin.SetActive(false);
        Button buttonJoin = GO_buttonJoin.GetComponent<Button>();
        buttonJoin.onClick.AddListener(() =>
        {
            NetworkManager.joinFriend(f.id);
            GameObject.Find("Canvas").transform.Find("Friends").gameObject.SetActive(false);
            GameObject.Find("Canvas").transform.Find("Home").gameObject.SetActive(true);
        });

        Button buttonDelete = newFriend.transform.Find("Button-delete").GetComponent<Button>();
        buttonDelete.onClick.AddListener(() =>
        {
            NetworkManager.supprimerAmi(NetworkManager.id, f.id);
        });

        switch (f.status)
        {
            case 2:
                GO_buttonJoin.SetActive(true);
                imgStatus.color = new Color32(79, 200, 74, 255);
                textStatus.text = "Online";
                break;
            case 1:
                imgStatus.color = new Color32(79, 200, 74, 255);
                textStatus.text = "Online";
                break;
            case 3:
                imgStatus.color = new Color32(74, 156, 200, 255);
                textStatus.text = "In Game";
                break;
            default:
                imgStatus.color = new Color32(128, 128, 128, 255);
                textStatus.text = "Offline";
                break;

        }
        f.obj = newFriend;
    }
    public void refreshFriendR(Friend f)
    {
        GameObject newFriend = Instantiate(componentRequest, containerRequest.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = f.name;
        Button accepter = newFriend.transform.Find("Button-add").GetComponent<Button>();
        accepter.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, f.id, true);
        });
        Button refuser = newFriend.transform.Find("Button-reject").GetComponent<Button>();
        refuser.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, f.id, false);
        });
        f.obj = newFriend;
    }
    public void refreshFriendW(Friend f)
    {
        GameObject newFriend = Instantiate(componentAddWait, containerWait.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = f.name;
        GameObject button_add = newFriend.transform.Find("Button-add").gameObject;
        GameObject button_cancel = newFriend.transform.Find("Button-cancel").gameObject;
        button_add.SetActive(false);
        button_cancel.SetActive(true);

        Button bAdd = button_add.GetComponent<Button>();
        bAdd.onClick.AddListener(() =>
        {
            NetworkManager.ajoutAmi(NetworkManager.id, f.id);
        });
        Button bCancel = button_cancel.GetComponent<Button>();
        bCancel.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, f.id, false);
        });
        f.obj = newFriend;
    }
    public void addFriend(string name,int id,int status){
        //SupprNoObject(listFriend);
        if (!NetworkManager.inGame)
        {
        GameObject newFriend = Instantiate(componentFriend, containerFriend.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;

        Image imgStatus = newFriend.transform.Find("Image_status").GetComponent<Image>();
        GameObject infoStatus = newFriend.transform.Find("Info_status").gameObject;
        TextMeshProUGUI textStatus = infoStatus.transform.Find("Text_status").GetComponent<TextMeshProUGUI>();
        infoStatus.SetActive(false);

        GameObject GO_buttonJoin = newFriend.transform.Find("Button-join").gameObject;
        GO_buttonJoin.SetActive(false);
        Button buttonJoin = GO_buttonJoin.GetComponent<Button>();
        buttonJoin.onClick.AddListener(() =>
        {
            NetworkManager.joinFriend(id);
            GameObject.Find("Canvas").transform.Find("Friends").gameObject.SetActive(false);
            GameObject.Find("Canvas").transform.Find("Home").gameObject.SetActive(true);
        });

        Button buttonDelete = newFriend.transform.Find("Button-delete").GetComponent<Button>();
        buttonDelete.onClick.AddListener(() =>
        {
            NetworkManager.supprimerAmi(NetworkManager.id, id);
        });
        
        switch(status){
            case 2:
                GO_buttonJoin.SetActive(true);
                imgStatus.color = new Color32(79,200,74,255);
                textStatus.text = "Online";
                break;
            case 1:
                imgStatus.color = new Color32(79,200,74,255);
                textStatus.text = "Online";
                break;
            case 3:
                imgStatus.color = new Color32(74,156,200,255);
                textStatus.text = "In Game";
                break;
            default:
                imgStatus.color = new Color32(128,128,128,255);
                textStatus.text = "Offline";
                break;

        }
        Friend f = new Friend(id, name, newFriend);
        listFriend.Add(f);

        }
        else
        {
            Friend f = new Friend(id, name, null);
            listFriend.Add(f);
        }
    }
// status:
        // 3 = in game
        // 2 = in lobby/waitscreen
        // 1 = connecté
        // 0 = invitation en attente amis
        // -1 = hors ligne

    public void addFriendRequest(string name,int id){

        GameObject newFriend = Instantiate(componentRequest, containerRequest.transform);

        TextMeshProUGUI textName = newFriend.transform.Find("Text-pseudo").GetComponent<TextMeshProUGUI>();
        textName.text = name;
        Button accepter = newFriend.transform.Find("Button-add").GetComponent<Button>();
        accepter.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, id, true);
        });
        Button refuser = newFriend.transform.Find("Button-reject").GetComponent<Button>();
        refuser.onClick.AddListener(() =>
        {
            NetworkManager.reponseAmi(NetworkManager.id, id, false);
        });
        Friend f =new Friend(id, name, newFriend);
        listRequest.Add(f);
    }

    public void addNoFriend (string msg, GameObject container, List<Friend> list){
        GameObject newObject = Instantiate(componentNo, container.transform);

        TextMeshProUGUI textName = newObject.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        textName.text = msg;

        list.Add(new Friend(-1, newObject));
    }

    public void UpdateStatusFriend(int id, int status)
    {
        int indice = GetIndiceFriendId(id, listFriend);
        listFriend[indice].status = status;
        if (!NetworkManager.inGame)
        {
        
        GameObject newFriend = listFriend[indice].obj;

        Image imgStatus = newFriend.transform.Find("Image_status").GetComponent<Image>();
        GameObject infoStatus = newFriend.transform.Find("Info_status").gameObject;
        TextMeshProUGUI textStatus = infoStatus.transform.Find("Text_status").GetComponent<TextMeshProUGUI>();

        GameObject GO_buttonJoin = newFriend.transform.Find("Button-join").gameObject;
        Button buttonJoin = GO_buttonJoin.GetComponent<Button>();
        GO_buttonJoin.SetActive(false);

        buttonJoin.onClick.AddListener(() =>
        {
            //network join avec id
        });
        
        switch(status){
            case 2:
                GO_buttonJoin.SetActive(true);
                imgStatus.color = new Color32(79,200,74,255);
                textStatus.text = "Online";
                break;
            case 1:
                imgStatus.color = new Color32(79,200,74,255);
                textStatus.text = "Online";
                break;
            case 3:
                imgStatus.color = new Color32(74,156,200,255);
                textStatus.text = "In Game";
                break;
            default:
                imgStatus.color = new Color32(128,128,128,255);
                textStatus.text = "Offline";
                break;

        }

        }

    }

    public void SupprimerAmi(int id){
        int indice = GetIndiceFriendId(id, listFriend);
        Destroy(listFriend[indice].obj);
        listFriend.Remove(listFriend[indice]);
        AfficheNoObject();
    }

    // Modification des amis par une reponse à une demande d'ami
    public void ReponseAmi(int id, bool answer)
    {
        List<Friend> list = listRequest;
        int indice = GetIndiceFriendId(id, listRequest);
        if(indice == -1){
            list = listWait;
            indice = GetIndiceFriendId(id, listWait);
            if(indice == -1){
                Debug.Log("id de l'ami introuvable");
                return; // error
            }
        }
        Friend f = list[indice];

        // vérifier si l'ami est dans add avec le bouton cancel
        int indiceAdd = GetIndiceFriendId(id, listAdd);
        if(indiceAdd != -1){
            GameObject obj = listAdd[indiceAdd].obj;
            obj.transform.Find("Button-add").gameObject.SetActive(true);
            obj.transform.Find("Button-cancel").gameObject.SetActive(false);
        } 

        // mise a jour
        if(answer){
            addFriend(f.name, f.id, -1);
        }
        Destroy(f.obj);
        list.Remove(f);
        AfficheNoObject();
    }

    // affiche dans les container des différents interface pour les amis
    // qu'il n'y a pas d'obejct (demande, resultat, ami) si les listes sont vides
    public void AfficheNoObject(){
        if(listRequest.Count == 0){
            addNoFriend("No request", containerRequest, listRequest);
        }
        if(listWait.Count == 0){
            addNoFriend("No request", containerWait, listWait);
        }
        if(listAdd.Count == 0){
            addNoFriend("No result", containerAdd, listAdd);
        }
        if(listFriend.Count == 0){
            addNoFriend("No friend", containerFriend, listFriend);
        }
    }

    public void SupprNoObject(List<Friend> list){
        if(list.Count == 1 && list[0].id == -1){
            Destroy(list[0].obj);
            list.Remove(list[0]);
        }
    }

    public int GetIndiceFriendId(int id, List<Friend> list){
        int indice = -1; 
        for(int i=0; i<list.Count ; i++){
            if(list[i].id == id){
                indice = i;
                break;
            }
        }
        return indice;
    }

    public void ClearListGameObject(List<Friend> list){
        foreach (Friend obj in list){
            Destroy(obj.obj);
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
    public string name;
    public int status;
    public GameObject obj;

    public Friend(){}

    public Friend(GameObject obj){
        this.obj = obj;
    }

    public Friend(int id, string name, int status, GameObject obj)
    {
        this.id = id;
        this.name = name;
        this.status = status;
        this.obj = obj;
    }

    public Friend(int id, string name, GameObject obj)
    {
        this.id = id;
        this.name = name;
        this.obj = obj;
    }

    public Friend(int id, GameObject obj)
    {
        this.id = id;
        this.obj = obj;
    }

}
