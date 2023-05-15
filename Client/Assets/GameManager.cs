    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Linq;



public class GameManager : MonoBehaviour
{
    ////////////////////////// Variables //////////////////////
    
    // joueur
    public Player p;
    // jeu
    private int nbPlayer;
    public List<Player> listPlayer = new List<Player>();
    public List<GameObject> listCard = new List<GameObject>();
    private List<Toggle> toggleOn = new List<Toggle>();
    public static List<Roles> roleRestant;
    public GameObject cardContainer, cardComponent, containerRole, roleComponent, GO_dead_bg, GO_rolesRestant, GO_tourRoles;
    public static bool isNight,action=false,useHeal=false,useKill=false;
    public bool soundNight,sceneNight=true;
    public static int tour = 0,turn,maire=-1; 
    public TextMeshProUGUI timer;
    public static float value_timer;
    public bool sestPresente = false, electionMaire = false;
    public Image banderoleMaire;
    public static List<(int,int)> newDead;

    // timer pour le texte qui s'affiche a l'ecran
    private float timer_text_screen = 2f;
    private bool text_screen_active = false;

    public Color colorRed, colorWhite, colorBlack, colorYellow, colorGreen, colorPink, colorBlue;
    public TextMeshProUGUI text_day, player_role, text_screen;
    public GameObject panel_text_screen;
    public Sprite VoyanteSprite, VillageoisSprite, LoupSprite, CupidonSprite, SorciereSprite;
    public Sprite ChasseurSprite, DictateurSprite, GardeSprite;


    public Button buttonValiderVote, buttonRole,buttonLeave,buttonPlayAgain,sePresenter;
    public GameObject GO_buttonAfficheCarte, GO_potion;

    // win screen
    public bool gameover = false;
    public int isVillageWin = 1;
    public GameObject winPanel, textWinPlayer;
    List<TextMeshProUGUI> listTextwin = new List<TextMeshProUGUI>();
    public TextMeshProUGUI groupWin, nbPoints;

    public GameObject gamePage, winScreenPage;
    //Cupidon
    public int lover1_id=-1;
    public int lover2_id=-1;

    // variable pour le chat
    List<Message> msgList = new List<Message>();
    List<Message> msgListLG = new List<Message>();
    public GameObject chat, chatLG, chatPanel, chatPanelLG, textComponent, chatNotification;
    public TMP_InputField inputChat, inputChatLG;
    public Button sendChat, sendChatLG;
    public Color playerC, systemC, loupC;
    // choix pendant l'action
    public GameObject choixAction;

    // options page
    public GameObject settingPage;
    public Button buttonLeaveGame;

    public AudioSource soundManager_day,soundManager_night;

    // Start is called before the first frame update
    void Start()
    {
		soundManager_day = GameObject.Find("SoundManager_day").GetComponent<AudioSource>();
        soundManager_night = GameObject.Find("SoundManager_night").GetComponent<AudioSource>();
        NetworkManager.inGame = true;
        nbPlayer = NetworkManager.nbplayeres;
        
        Button buttonAfficheCarte = GO_buttonAfficheCarte.GetComponent<Button>();
        Button buttonOui = choixAction.transform.Find("Button-Oui").GetComponent<Button>();
        Button buttonNon = choixAction.transform.Find("Button-Non").GetComponent<Button>();
        sendChat.onClick.AddListener(OnButtonClickSendMsg);
        sendChatLG.onClick.AddListener(OnButtonClickSendMsgLG);
        buttonNon.onClick.AddListener(OnButtonClickNon);
        buttonOui.onClick.AddListener(OnButtonClickOui);
        buttonRole.onClick.AddListener(OnButtonClickRole);
        buttonValiderVote.onClick.AddListener(OnButtonClickVote);
        buttonAfficheCarte.onClick.AddListener(OnButtonClickAffiche);
        buttonLeave.onClick.AddListener(OnButtonClickLeaveGame);
        buttonLeaveGame.onClick.AddListener(OnButtonClickLeaveGame);
        buttonPlayAgain.onClick.AddListener(OnButtonClickPlayAgain);
        sePresenter.onClick.AddListener(OnButtonClickSePresenter);
        soundNight=true;
        play_sound_night();
        NetworkManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        tour = 0;
        sceneNight = true;
        if(newDead==null){
            newDead=new List<(int,int)>();
        }
        // remplir les informations des joueurs 
        foreach (WPlayer p in NetworkManager.players)
        {
            switch (p.GetRole())
            {
                case 0:
                    listPlayer.Add(new Player(p.GetUsername(), "Villageois", 0, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Villageois", 0, p.GetId(), true);
                        player_role.text = "Villager";
                    }
                    break;
                case 1:
                    listPlayer.Add(new Player(p.GetUsername(), "Villageois", 1, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Villageois", 1, p.GetId(), true);
                        player_role.text = "Villager";
                    }
                    break;
                case 2:
                    listPlayer.Add(new Player(p.GetUsername(), "Cupidon", 2, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Cupidon", p.GetRole(), p.GetId(), true);
                        player_role.text = "Cupidon";
                    }
                    break;
                case 3:
                    listPlayer.Add(new Player(p.GetUsername(), "Voyante", 3, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Voyante", p.GetRole(), p.GetId(), true);

                        player_role.text = "Fortune teller";
                    }
                    break;
                case 4:
                    listPlayer.Add(new Player(p.GetUsername(), "Loup-garou", 4, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Loup-garou", p.GetRole(), p.GetId(), true);

                        player_role.text = "Werewolf";
                    }
                    break;
                case 5:
                    listPlayer.Add(new Player(p.GetUsername(), "Sorciere", 5, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Sorciere", p.GetRole(), p.GetId(), true);
                        player_role.text = "Witch";

                    }
                    break;
                case 6:
                    listPlayer.Add(new Player(p.GetUsername(), "Chasseur", 6, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Chasseur", p.GetRole(), p.GetId(), true);
                        player_role.text = "Hunter";

                    }
                    break;
                case 7:
                    listPlayer.Add(new Player(p.GetUsername(), "Dictateur", 7, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Dictateur", p.GetRole(), p.GetId(), true);
                        player_role.text = "Dictator";

                    }
                    break;
                case 8:
                    listPlayer.Add(new Player(p.GetUsername(), "Garde", 8, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Garde", p.GetRole(), p.GetId(), true);
                        player_role.text = "Guard";

                    }
                    break;
            }
            
        }
        if(maire!=-1){
            if (maire == p.GetId())
                {
                    p.SetIsMaire(true);
                }
                else
                {
                    p.SetIsMaire(false);

                }
                if (!p.GetIsAlive())
                {
                    GO_dead_bg.SetActive(true);
                }
                foreach (Player j in listPlayer)
                {
                    if (j.GetId() == maire)
                    {
                        j.SetIsMaire(true);
                    }
                    else
                    {
                        j.SetIsMaire(false);
                    }
                }
                maire=-1;
        }

        AfficherJour();
        AfficheCard();
        MiseAJourAffichage();
        InitPotion();
        EndVote();
        listerRoles();
        Debug.Log("turn ="+turn);
    }

    // Update is called once per frame
    void Update()
    {

        NetworkManager.listener();
        if(isNight!=soundNight){
            if(isNight){
                play_sound_night();
            }else{
                play_sound_day();
            }
            soundNight=isNight;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingPage.SetActive(!settingPage.activeSelf);
        }
        // envoyer un message dans le chat
        if (inputChat.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            string msg = p.GetPseudo() + ": " + inputChat.text.ToString();
            NetworkManager.sendchatMessage(msg);
            inputChat.text = "";
            inputChat.ActivateInputField();
        }
        if(inputChatLG.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            string msg = p.GetPseudo() + ": " + inputChatLG.text.ToString();
            NetworkManager.sendchatLGMessage(msg);
            inputChatLG.text = "";
            inputChatLG.ActivateInputField();
        }

        
        if(useHeal){
            useHeal=false;
            UseHealthPotion();
        }
        if(useKill){
            useKill=false;
            UseDeathPotion();
        }
        setDead();
        isFinished();
        AfficheTimer();
        AfficherJour();
        Timer_text_screen();
        changeTurn();
        

    }

    //Fonctions pour les sons ->
    //Son à jouer pendant la nuit
    public void play_sound_night(){
            soundManager_day.Stop();
            soundManager_night.Play();
    }
    //Son à jouer pendant la journée
    public void play_sound_day(){
        soundManager_night.Stop();
        soundManager_day.Play();
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'envoyer une candidature pour le vote du maire
    **/
    private void OnButtonClickSePresenter()
    {
        NetworkManager.sendMayorPresentation();
    }
    private void setDead(){
    while(newDead.Count!=0){
        Debug.Log("hey hes ded");
        (int val,int role)=newDead[0];
        newDead.RemoveAt(0);
        foreach (Player p in listPlayer)
        {
            if (p.GetId() == val)
            {
                if (p.GetId() == this.p.GetId())
                {
                    p.SetIsAlive(false);
                    if(!p.GetIsMaire()){
                        LITTERALLYDIE();
                    }
                }
                p.SetRole(role);
                p.SetIsAlive(false);
                RemoveRoleRestant(role);
            }

            Debug.Log(p.GetIsAlive());
        }
        updateImage(val, role);
        
        MiseAJourAffichage();
    }
    }
    private void isFinished(){
        if (gameover)
        {
            gamePage.transform.gameObject.SetActive(false);
            winScreenPage.transform.gameObject.SetActive(true);
            AfficheWinScreen();
            gameover = false;
        }
    }
    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet de quitter la partie de loup garou
    **/
    private void OnButtonClickLeaveGame()
    {
        GameManagerApp.client = NetworkManager.client;
        GameManagerApp.scene = 1;
        LoadScene("Jeu");
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet de rejouer au loup-garou, conduit dans la page home
    **/
    private void OnButtonClickPlayAgain()
    {
        GameManagerApp.client = NetworkManager.client;
        GameManagerApp.scene = 2;
        LoadScene("Jeu");
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'envoyer un message dans le chat
    **/
    private void OnButtonClickSendMsg()
    {
        if (inputChat.text != "")
        {
            string msg = p.GetPseudo() + ": " + inputChat.text.ToString();
            NetworkManager.sendchatMessage( msg);
            inputChat.text = "";
            inputChat.ActivateInputField();
        }
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'envoyer un message dans le chat des loups
    **/
    private void OnButtonClickSendMsgLG()
    {
        if (inputChat.text != "")
        {
            string msg = p.GetPseudo() + ": " + inputChat.text.ToString();
            NetworkManager.sendchatLGMessage(msg);
            inputChat.text = "";
            inputChat.ActivateInputField();
        }
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet de valider et envoyer le vote au serveur
    **/
    private void OnButtonClickVote()
    {
        Vote();
    }

    /**
        Affiche à qui c'est le tour en grand sur l'écran
    **/
    public void changeTurn()
    {
        if (turn!=0)
        {
            action = false;
            electionMaire = false;
            if(p.GetRoleId() == 7){
                Debug.Log("c'est le 7 mec");
            }
            switch (turn)
            {

                case 1:
                    affiche_tour_role("It is village's turn...", turn);
                    break;
                case 2:
                    affiche_tour_role("It is Cupido's turn...", turn);
                    if (p.GetRoleId() == 2)
                    {
                        action=true;
                        Debug.Log("tour du cupi");
                        actionCupidon();
                    }
                    break;
                case 3:
                    affiche_tour_role("It is the Fortune teller's turn...", turn);
                    break;
                case 4:
                    affiche_tour_role("It is Werewolves' turn...", turn);
                    break;
                case 5:
                    affiche_tour_role("It is Witch's turn...", turn);
                    if (p.GetRoleId() == 5)
                    {
                        Debug.Log("je demande a la sorciere si elle veut utiliser sa posion de mort ou non");
                        affiche_choix_action("Wanna kill someone ?");
                    }
                    break;
                case 6:
                    affiche_tour_role("It is Hunter's turn...", turn);
                    break;
                case 7:
                    affiche_tour_role("It is Dictator's turn...", turn);
                    if (p.GetRoleId() == 7)
                    {
                        ChoixDictateur();
                    }
                    break;
                case 8:
                    affiche_tour_role("It is Guard's turn...", turn);
                    break;
                case 255:
                    affiche_tour_role("It is Mayor's turn...", turn);
                    electionMaire = true;
                    break;
                case 254 :
                    affiche_tour_role("The mayor will choose the one to die ...", turn); 
                    break;
                case 253:
                    affiche_tour_role("Mayor must select his new successor ...", turn);
                    if(p.GetIsMaire()){
                        GO_dead_bg.SetActive(false);
                    }
                    break;
            }
            foreach (Player p in listPlayer)
            {
                p.SetVote(-1);
            }
            UpdateVote();
            turn = 0;
        }
    }

    /**
        La fonction permet d'afficher l'image de role d'un joueur dans sa carte
        Args :  - id : l'id du joueur dont on veut afficher son role
                - role : role d'un joueur
    **/
    public void updateImage(int id, int role)
    {
        int indice = chercheIndiceJoueurId(id);
        Toggle toggleCard = listCard[indice].transform.Find("Toggle-Card").GetComponent<Toggle>();
        Image roleImg = toggleCard.transform.Find("Image-Card").GetComponent<Image>();
        switch (role)
        {
            case 2:
                roleImg.sprite = CupidonSprite;
                break;
            case 3:
                roleImg.sprite = VoyanteSprite;
                break;
            case 4:
                roleImg.sprite = LoupSprite;
                break;
            case 5:
                roleImg.sprite = SorciereSprite;
                break;
            case 6:
                roleImg.sprite = ChasseurSprite;
                break;
            case 7:
                roleImg.sprite = DictateurSprite;
                break;
            case 8:
                roleImg.sprite = GardeSprite;
                break;
            default:
                roleImg.sprite = VillageoisSprite;
                break;
        }
        roleImg.enabled = true;
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet de voir la carte d'un joueur (action de la voyante)
    **/
    private void OnButtonClickAffiche(){
        AfficheVoyante();
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'afficher à l'écran la page des roles restants
    **/
    private void OnButtonClickRole(){
        bool active = GO_rolesRestant.activeSelf;
        GO_rolesRestant.SetActive(!active);
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'envoyer au serveur que la reponse au choix est négative
    **/
    public void OnButtonClickNon() {
        choixAction.SetActive(false);
        NetworkManager.Vote( NetworkManager.id,0);
        // envoyer au serveur NON
    }

    /**
        Action effectué lorsqu'on appuie sur le bouton associer à la fonction
        Permet d'envoyer au serveur que la reponse au choix est positive
    **/
    public void OnButtonClickOui() {
        choixAction.SetActive(false);
        NetworkManager.Vote( NetworkManager.id, 1);
        // envoyer au serveur OUI
    }

    /**
        La fonction permet de modifier le message qui indique que c'est le tour de ce rôle de jouer
        Args:   - msg, une chaine de caractuères, le message à afficher
                - tour, un entier représentant le role qui joue
    **/
    public void affiche_tour_role(string msg,int tour)
    {
        
        if (p.GetRoleId() != tour && tour!= 1 && tour!=255 && tour != 254 && tour !=253)
        {
            GO_tourRoles.SetActive(true);
        }
        else if((tour == 254 || tour == 253) && !p.GetIsMaire()){
            GO_tourRoles.SetActive(true);
        }
        else
        {
            GO_tourRoles.SetActive(false);
        }
        
        TextMeshProUGUI text_role = GO_tourRoles.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        text_role.text = msg;
        
    }

    /**
        met a jour le texte indiquant si c'est la nuit, le jour et si c'est election du maire
    **/
    public void AfficherJour()
    {
        
        if (electionMaire)
        {
            banderoleMaire.enabled = true;
            text_day.text = "Mayor";
            text_day.color = colorBlack;
            player_role.text = "elections";
            player_role.color = colorWhite;
            if (!sestPresente) sePresenter.gameObject.SetActive(true);
            else sePresenter.gameObject.SetActive(false);
        }

        else if (isNight != sceneNight)
        {
            sceneNight=isNight;
            if(sceneNight==false){
                ChangeChat(sceneNight);
                Debug.Log("it's day");
                banderoleMaire.enabled = false;
                text_day.text = "Day " + tour;
                text_day.color = colorWhite;
                player_role.color = colorWhite;
                player_role.text = p.GetRole();
                sePresenter.gameObject.SetActive(false);
            }else{
                ChangeChat(sceneNight);
                banderoleMaire.enabled = false;
                text_day.text = "Night " + tour;
                text_day.color = colorRed;
                player_role.color = colorRed;
                player_role.text = p.GetRole();
                sePresenter.gameObject.SetActive(false);
            }
            
        }
    }

    /**
        Affiche et met a jour la nouvelle valeur du timer dans le jeu
    **/
    public void AfficheTimer(){
        if (NetworkManager.time != 0)
        {
            value_timer = NetworkManager.time;
            NetworkManager.time = 0;
        }
        if (value_timer > 0)
        {
            value_timer -= Time.deltaTime; 
            timer.text = "" + Mathf.Round(value_timer); 
            if(Mathf.Round(value_timer) <= 5){
                timer.color = colorYellow;
            }
            else{
                timer.color = colorWhite;
            }
        }
    }

    /**
        Ajoute un objet "text" dans le conteneur du win-screen,
        Ecrit dans l'objet text: le pseudo du joueur et son role, indique s'il est en vie ou mort
        Args:   - panel, objet dans le quel ajouter l'objet "text"
                - num, le numéro du joueur, dans la liste des joueurs, à afficher avec le texte
    **/
    public void AjoutTextWin(GameObject panel, int num)
    {
        TextMeshProUGUI newText;
        GameObject newPlayer = Instantiate(textWinPlayer, panel.transform);
        newText = newPlayer.GetComponent<TextMeshProUGUI>();
        newText.text = listPlayer[num].GetPseudo() + ": " + listPlayer[num].GetRole();
        Debug.Log(listPlayer[num].GetRole());
        if (listPlayer[num].GetRoleId() == 4)
        {
            
            newText.color = colorRed;
        }
        if (listPlayer[num].GetIsAlive() == false)
        {
            FontStyles currentStyle = newText.fontStyle;
            newText.fontStyle = currentStyle | FontStyles.Strikethrough;
        }
        listTextwin.Add(newText);
    }

    /**
        Affiche le win screen, les joueurs, leur role et leur état (mort/vivant)
        et qui a gagné
    **/
    public void AfficheWinScreen()
    {
        GO_dead_bg.SetActive(false);
        foreach (TextMeshProUGUI text in listTextwin)
        {
            Destroy(text.gameObject);
        }
        listTextwin.Clear();

        switch(isVillageWin) {
            case 2 : // Loup-garou
                groupWin.text = "Werewolves won";
                groupWin.color = colorRed;
                break;
            
            case 1 : // villageois
                groupWin.text = "Villagers won";
                groupWin.color = colorWhite;
                break;
            
            case 4 : // draw
                groupWin.text = "Draw";
                groupWin.color = colorWhite;
                break;

            case 3 : // amoureux
                groupWin.text = "Lovers won";
                groupWin.color = colorPink;
                break;
        }
        
        for (int i = 0; i < nbPlayer; i++)
        {
            AjoutTextWin(winPanel, i);
        }

    }

    /**
        Modifie l'affiche pour indiquer les 2 joueurs amoureux
    **/
    public void AfficheAmoureux(){
         if(lover1_id==-1||lover2_id==-1)return;
        if(p.GetRoleId() == 2||p.GetId()==lover1_id||p.GetId()==lover2_id){
            GameObject[]  textpseudos= GameObject.FindGameObjectsWithTag("Pseudos");
            foreach(GameObject go in textpseudos){
                TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
                if(texto!=null){
                    int id1 = chercheIndiceJoueurId(lover1_id), id2 = chercheIndiceJoueurId(lover2_id);
                    Debug.Log("id1= " + id1 + "id2 = " + id2+"lvid= "+ lover1_id+" lvid= "+ lover2_id);
                    if(texto.text==listPlayer[id1].GetPseudo()||texto.text==listPlayer[id2].GetPseudo()){
                        texto.color=colorPink;
                        };
                }
            }
        }
    }

    /**
        Affiche dans le chat un message
        Args:   - text, le message à afficher dans le chat
                - type, le type du message
    **/
    public void SendMessageToChat(string text, Message.MsgType type)
    {
        GameObject newText = Instantiate(textComponent, chatPanel.transform);
        Message newMsg = new Message();

        newMsg.msg = text;
        newMsg.textComponent = newText.GetComponent<TextMeshProUGUI>();
        newMsg.textComponent.text = newMsg.msg;
        newMsg.textComponent.color = MessageColor(type);

        msgList.Add(newMsg);

        // affiche si necessaire l'icone de notification de nouveaux messages
        if(ButtonClick.isHide && type == Message.MsgType.player){
            chatNotification.SetActive(true);
        }
        else{
            chatNotification.SetActive(false);
        }

    }

    /**
        Affiche dans le chat des loup-garou un message
        Args:   - text, le message à afficher dans le chat
                - type, le type du message
    **/
    public void SendMessageToChatLG(string text, Message.MsgType type)
    {
        GameObject newText = Instantiate(textComponent, chatPanelLG.transform);
        Message newMsg = new Message();
        Debug.Log(text);
        newMsg.msg = text;
        newMsg.textComponent = newText.GetComponent<TextMeshProUGUI>();
        newMsg.textComponent.text = newMsg.msg;
        newMsg.textComponent.color = MessageColor(type);

        msgList.Add(newMsg);

        // affiche si necessaire l'icone de notification de nouveaux messages
        if(ButtonClick.isHide && type == Message.MsgType.loup){
            chatNotification.SetActive(true);
        }
        else{
            chatNotification.SetActive(false);
        }

    }

    /**
        Renvoie une couleur en fonction du type de message
        Arg: type, le type du message
    **/
    private Color MessageColor(Message.MsgType type)
    {
        Color color;
        switch (type)
        {
            case Message.MsgType.player:
                color = playerC;
                break;
            case Message.MsgType.loup:
                color = loupC;
                break;
            default:
                color = systemC;
                break;
        }
        return color;
    }

    /**
        la fonction permet de changer de chat (géneral/loup)
        si l'utilisateur est loup-garou
    **/
    public void ChangeChat(bool night){
        if(p.GetRoleId() == 4){
            chat.SetActive(!night);
            chatLG.SetActive(night);
        }
        inputChat.text = "";
        inputChatLG.text = "";
    }

    /**
        change/charge une scene
        Arg: sceneName, le nom de la scene a charger
    **/
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /**
        ajout une carte dans le conteneur des cartes des joueurs,
        et affiche les informations du joueur
        Arg: id, le numéro du joueur dont appartient la carte
    **/
    public void AjoutCarte(int id)
    {
        GameObject newCard = Instantiate(cardComponent, cardContainer.transform);
        Toggle toggleCard = newCard.transform.Find("Toggle-Card").GetComponent<Toggle>();
        toggleCard.onValueChanged.AddListener(delegate {OnToggleValueChanged(toggleCard);});
        TextMeshProUGUI text = newCard.transform.Find("Text-Card").GetComponent<TextMeshProUGUI>();
	    text.tag="Pseudos";
        Image roleImg = toggleCard.transform.Find("Image-Card").GetComponent<Image>();
        Debug.Log("id == " + id);
        text.text = listPlayer[id].GetPseudo();
        switch(listPlayer[id].GetRoleId()) {
            case 1:
                roleImg.sprite = VillageoisSprite;
                break;
            case 2:
                roleImg.sprite = CupidonSprite;
                break;
            case 3:
                roleImg.sprite = VoyanteSprite;
                break;
            case 4:
                roleImg.sprite = LoupSprite;
                break;
            case 5:
                roleImg.sprite = SorciereSprite;
                break;
            case 6:
                roleImg.sprite = ChasseurSprite;
                break;
            case 7:
                roleImg.sprite = DictateurSprite;
                break;
            case 8:
                roleImg.sprite = GardeSprite;
                break;
            default:
                roleImg.sprite = VillageoisSprite;
                break;
        }
        if (listPlayer[id].GetId() == p.GetId())
        {
            roleImg.enabled = true;
        }
        else
        {
            roleImg.enabled = false;
        }
        
        listCard.Add(newCard);
    }

    /**
        affiche les cartes des joueurs dans le jeu
    **/
    public void AfficheCard()
    {
        Debug.Log("nbp="+nbPlayer);
        if (nbPlayer > 10){
            cardContainer.transform.localPosition = new Vector3(0, -93f, 0);
        }
        for (int i = 0; i < nbPlayer; i++)
        {
            AjoutCarte(i);
        }

    }

    /**
        Fonction executee lorsque un toggle change de valeur (on/off)
        La fonction vérifie s'il y a trop de toggle selectionne, si oui 
        elle desactive le plus ancien toggle selectionné
        Arg: change, le toggle qui a change de valeur
    **/
    public void OnToggleValueChanged(Toggle change){
        bool value = change.isOn;

        if (value){
            Debug.Log("_a marche pas");

            toggleOn.Add(change);
            if(p.GetRoleId() == 2 && action){
                Debug.Log("_a marche");
                if (toggleOn.Count > 2){
                    toggleOn[0].isOn = false;
                    Debug.Log(toggleOn.Count);
                }
            }
            else{
                if (toggleOn.Count > 1){
                    toggleOn[0].isOn = false;
                }
            }
        }
        else{
            toggleOn.RemoveAll(Toggle => Toggle == change);
        }
    }

    /**
        La fonction permet d'obtenir l'indice dans la liste du toggle selectionne
        indice valide pour les listes: listCard, listPlayer
    **/
    public int GetIndiceToggleOn(){
        int indice = -1;
        for(int i = 0; i < nbPlayer; i++) {
            if(listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn) {
                indice = i;
            }
        }
        return indice;
    }

    /**
        La fonction met la valeur, de tous les toggles des cartes des joueurs, a false
    **/
    public void AllToggleOff(){
        for(int i = 0; i < nbPlayer; i++) {
            listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = false;
        }
    }

    /**
        Permet de faire tomber deux joueurs amoureux
        (action du cupidon)
    **/
    public void actionCupidon(){
        int indice1, indice2, id1=-1, id2=-1;
        string msg;

        indice1 = GetIndiceToggleOn();
        if (indice1 != -1){
            listCard[indice1].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = false;
            id1 = listPlayer[indice1].GetId();
            lover1_id= indice1;
        }

        indice2 = GetIndiceToggleOn();
        if (indice2 != -1){
            id2 = listPlayer[indice2].GetId();
            msg = listPlayer[indice1].GetPseudo() + " and " + listPlayer[indice2].GetPseudo() + " fell in love each other";
            lover2_id= indice2;
            SendMessageToChat(msg, Message.MsgType.system);
            NetworkManager.ChooseLovers( NetworkManager.id, id1, id2);


        }
        else {
            SendMessageToChat("Chose two players to marry...", Message.MsgType.system);
        }
    }

    /**
        La fonction rend les deux joueurs amoureux
        Args:   - id1, l'id de l'un des deux joueurs tombe amoureux
                - id2, l'id de l'autre joueur tombe amoureux
    **/
    public void setAmoureux(int id1, int id2){
        int indice1 = chercheIndiceJoueurId(id1);
        int indice2 = chercheIndiceJoueurId(id2);
        p.SetIsMarried(true);
        listPlayer[indice1].SetIsMarried(true);
        listPlayer[indice2].SetIsMarried(true);
    }

    /**
        la fonction permet de voter pour un joueur
        et envoie au serveur le resultat du vote
    **/
    public void Vote()
    {
        if(p.GetRoleId() ==2 && action){
            actionCupidon();
        }
        else{
            int selectedId = GetIndiceToggleOn();
            if(selectedId != -1){
                SendMessageToChat("You have voted for "+listPlayer[selectedId].GetPseudo(), Message.MsgType.system);
                p.SetVote(listPlayer[selectedId].GetId());
                NetworkManager.Vote( NetworkManager.id, listPlayer[selectedId].GetId());
                Debug.Log($"joueur {NetworkManager. id} vote pour {listPlayer[selectedId].GetId()}");
            } else{
                SendMessageToChat("You didn't vote for anyone...", Message.MsgType.system);
            } 
        }

        AllToggleOff();
    }

    /**
        la fonction rend le chat interactif (ou non)
    **/
    public void chatInteractable() {

        /*if(isNight && p.GetIsAlive() && p.GetRole() == "Loup-Garou" && turn == 4) {
            sendChat.interactable = true;
            inputChat.interactable = true;
        }
        else */
        if(isNight || !p.GetIsAlive()) {
            sendChat.interactable = false;
            inputChat.interactable = false;
        }
        else if(p.GetIsAlive()) {
            sendChat.interactable = true;
            inputChat.interactable = true;
        }       
    }

    /**
        la fonction met a jour les informations de la carte d'un joueur
        Arg: indice, l'indice du joueur dans listPlayer
    **/
    public void MiseAJourCarte(int indice)
    {

        Toggle toggleCard = listCard[indice].transform.Find("Toggle-Card").GetComponent<Toggle>();
        TextMeshProUGUI text =  listCard[indice].transform.Find("Text-Card").GetComponent<TextMeshProUGUI>();
        Image roleImg = toggleCard.transform.Find("Image-Card").GetComponent<Image>();
        Image eyeImg = toggleCard.transform.Find("eye").GetComponent<Image>();
        Image heart = toggleCard.transform.Find("heart").GetComponent<Image>();
        Image maire = toggleCard.transform.Find("maire").GetComponent<Image>();

        GameObject skull = toggleCard.transform.Find("skull").gameObject;


        eyeImg.enabled = false;
        heart.enabled = false;
        maire.enabled = false;
        skull.SetActive(false);

        if(p.GetRoleId() == 3 && listPlayer[indice].GetSeen()) {
            roleImg.enabled = true;
            eyeImg.enabled = true;            
        }

        if(p.GetRoleId() == 4 && listPlayer[indice].GetRoleId() == 4) {
            roleImg.enabled = true;
        }

        if(p.GetRoleId() == 2 || p.GetIsMarried()){
            if(listPlayer[indice].GetIsMarried()){
                heart.enabled = true;
            }
        }

        if(listPlayer[indice].GetIsMaire()) {
            maire.enabled = true;
        }else{
            maire.enabled = false;

        }

        if (!listPlayer[indice].GetIsAlive())
        {
            text.text = listPlayer[indice].GetPseudo() + "\n" + listPlayer[indice].GetRole();
            text.color = colorRed;
            toggleCard.interactable = false;
            roleImg.enabled = true;
            eyeImg.enabled = false;
            skull.SetActive(true);

            // changer la couleur de la carte
            ColorBlock colors = toggleCard.colors;
            colors.disabledColor = colorBlack;
            toggleCard.colors = colors;
        }
    }

    /**
        Met a jour l'affiche des cartes des joueurs
    **/
    public void MiseAJourAffichage()
    {
        for (int i = 0; i < listPlayer.Count; i++)
        {
            MiseAJourCarte(i);
        }
    }

    /**
        Envoie au serveur quelle est la carte que la voyante veut voir
    **/
    public void AfficheVoyante() {
        int selectedId = GetIndiceToggleOn();
        if(player_role.text == "Fortune teller" && isNight) {
            if(selectedId != -1 && selectedId < nbPlayer)
            {
                NetworkManager.Vote( NetworkManager.id, listPlayer[selectedId].GetId());
            }
        } 
    }

    /**
        Affiche dans le jeu un choix (oui/non) avec comme texte le message donné en argument
        Arg: msg, le texte a afficher pour le choix a faire
    **/
    public void affiche_choix_action(string msg){
        choixAction.SetActive(true);
        TextMeshProUGUI text_action =  choixAction.transform.Find("Text-action").GetComponent<TextMeshProUGUI>();
        text_action.text = msg;
    }

    /**
        La fonction indique a la sorciere quelle personne est mort cette nuit
        et demande si elle veut utilise sa potion de vie
        Arg: id, l'id du joueur mort dans la nuit
    **/
    public void ActionSorciere(int id){
        int indice = chercheIndiceJoueurId(id);
        if (indice == -1) return;   // erreur l'id du joueur ne correspond a aucun joueur
        string msg = "" + listPlayer[indice].GetPseudo() + " is dead. Wanna use your life potion ?";
        affiche_choix_action(msg);
    }

    /**
        La fonction affiche au dictateur un choix
        s'il veut faire son action 
    **/
    public void ChoixDictateur() {
        affiche_choix_action("Wanna do a putsch ?");
    }

    /*public void ActionDictateur() {
        // Lancer le vote
        Vote(); // Fin du vote
    }*/

    /**
        la fonction permet d'obtenir l'indice du joueur dans la liste
        des joueurs avec son id
        Arg: id, l'id du joueur
    **/
    public int chercheIndiceJoueurId (int id){
        for(int i=0; i<listPlayer.Count; i++){
            if(listPlayer[i].GetId() == id){
                return i;
            }
        }
        return -1;
    }

    /**
        affiche le texte donné en argument dans l'objet text_screen et 
        lance le timer d'affichage pour l'objet text_screen
        Arg: msg, le texte a afficher dans l'objet
    **/
    public void Change_text_screen(string msg) {
        timer_text_screen = 2;
        text_screen.text = msg;
        text_screen_active = true;
    }

    /**
        met a jour la valeur du timer du text_screen et 
        desactive l'objet text_screen lorsque le timer est ecoule
    **/
    public void Timer_text_screen(){
        if(text_screen_active){
            if(timer_text_screen > 0){
                timer_text_screen -= Time.deltaTime;
            }
            else{
                text_screen.text = "";
                panel_text_screen.SetActive(false);
                text_screen_active = false;
            }
        }
    }

    /**
        la fonction permet d'afficher un texte à l'écran indiquant le role du joueur
        Args:   - id, l'id du joueur
                - idrole, le numéro correspondant au role du joueur
    **/
    public void affiche_text_role(int id, int idrole)
    {
        GO_buttonAfficheCarte.SetActive(false);
        panel_text_screen.SetActive(true);
        int indice = chercheIndiceJoueurId(id);
        string msg = listPlayer[indice].GetPseudo() + " is " + idRoleToStringRole(idrole);
        listPlayer[indice].SetSeen(true);
        SendMessageToChat(msg, Message.MsgType.system);
        Change_text_screen(msg);
    }

    /**
        
    **/
    public void affiche_egalite(int[] id)
    {
        AllToggleOff();
        for(int i = 0; i<id.Length; i++) {
            listCard[chercheIndiceJoueurId(id[i])].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = true;

        }
        panel_text_screen.SetActive(true);
        Change_text_screen("It's a draw...\n You have to settle now..");

        /*
        AllToggleOff();
        
        for(int i = 0; i<id.Length; i++) {
            if(id.Contains(i)) listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = true;
        }*/

        // Lancer le vote

        //Vote(); // Confirmation de vote
    }
    

    /**
        La fonction permet d'obtenir le nom d'un role avec son numero
        Arg: idRole, le numéro du role
    **/
    public string idRoleToStringRole(int idRole)
    {
        string role="";
        switch (idRole)
        {
            case 1:
                role = "Villageois";
                break;
            case 2:
                role = "Cupidon";
                break;
            case 3:
                role = "Voyante";
                break;
            case 4:
                role = "Loup-garou";
                break;
            case 5:
                role = "Sorciere";
                break;
            case 6:
                role = "Chasseur";
                break;
            case 7:
                role = "Dictateur";
                break;
            case 8:
                role = "Garde";
                break;

        }
        return role;
    }

    /**
        modifie l'affichage du jeu si le joueur est mort
    **/
    public void LITTERALLYDIE() {
        GO_dead_bg.SetActive(true);
    }

    /**
        la fonction permet de compter combien il y a de joueurs a chaque role 
        et initialiser les roles restants
    **/
    public void listerRoles() {
        SendMessageToChat("Liste des roles:", Message.MsgType.system);
        for(int i = 0; i<roleRestant.Count; i++) {
            if(roleRestant[i].get_role_count() > 0){
                SendMessageToChat("" + roleRestant[i].get_role_count() + " " + roleRestant[i].get_role(), Message.MsgType.system);

                //créer les objets pour l'affichage des roles
                GameObject newRole = Instantiate(roleComponent, containerRole.transform);
                newRole.name = "role" + i;
                Image imageRole = newRole.transform.Find("ImageRole").GetComponent<Image>();
                GameObject rond = newRole.transform.Find("RondNombre").gameObject;
                TextMeshProUGUI textNombre = rond.transform.Find("TextNombre").GetComponent<TextMeshProUGUI>();
                GameObject info = newRole.transform.Find("InfoRole").gameObject;
                TextMeshProUGUI textInfo = info.transform.Find("Text").GetComponent<TextMeshProUGUI>();

                Debug.Log("role name : "+ roleRestant[i].get_role());

                imageRole.sprite = roleRestant[i].get_image();
                textNombre.text = roleRestant[i].get_role_count().ToString();
                textInfo.text = roleRestant[i].get_description();
            }
        }
    }

    /**
        La fonction permet de décrémenter de 1 le nombre de joueurs en vie jouant ce role
        et met à jour l'affichage
        Arg: idrole, le nombre associé au role
    **/
    public void RemoveRoleRestant(int idrole){
        for(int i=0; i<roleRestant.Count; i++){
            if(roleRestant[i].get_idrole() == idrole){
                roleRestant[i].set_role_count(roleRestant[i].get_role_count()-1);
                
                GameObject role_GO = containerRole.transform.Find("role" + i).gameObject;
                GameObject rond = role_GO.transform.Find("RondNombre").gameObject;
                TextMeshProUGUI textNombre = rond.transform.Find("TextNombre").GetComponent<TextMeshProUGUI>();
                textNombre.text = roleRestant[i].get_role_count().ToString();
                if(roleRestant[i].get_role_count() <= 0){
                    Image imageRole = role_GO.transform.Find("ImageRole").GetComponent<Image>();
                    imageRole.color = new Color(0.25f, 0.25f, 0.25f, 1f);
                }
            }
        }
    }

    /**
        la fonction permet d'afficher les potions au debut de la partie (si sorciere)
    **/
    public void InitPotion(){
        if (p.GetRoleId() == 5){
            Image potionVideV = GO_potion.transform.Find("Potion_videV").GetComponent<Image>();
            Image potionVideM = GO_potion.transform.Find("Potion_videM").GetComponent<Image>();
            potionVideV.enabled = false;
            potionVideM.enabled = false;
        }
        else{
            GO_potion.SetActive(false);
        }
    }

    /**
        la fonction met a jour l'affichage de la potion de vie
    **/
    public void UseHealthPotion(){
        Image potionVie = GO_potion.transform.Find("Potion_vie").GetComponent<Image>();
        Image potionVideV = GO_potion.transform.Find("Potion_videV").GetComponent<Image>();
        GameObject help = GO_potion.transform.Find("Text-PotionVie").gameObject;
        TextMeshProUGUI textPotion = help.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        textPotion.text = "Empty health potion";
        potionVie.enabled = false;
        potionVideV.enabled = true;
        SendMessageToChat("You used your life potion!", Message.MsgType.system);
    }

    /**
        la fonction met a jour l'affichage de la potion de mort
    **/
    public void UseDeathPotion(){
        Image potionMort = GO_potion.transform.Find("Potion_mort").GetComponent<Image>();
        Image potionVideM = GO_potion.transform.Find("Potion_videM").GetComponent<Image>();
        GameObject help = GO_potion.transform.Find("Text-PotionMort").gameObject;
        TextMeshProUGUI textPotion = help.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        textPotion.text = "Empty death potion";
        potionMort.enabled = false;
        potionVideM.enabled = true;
        SendMessageToChat("You used your death potion!", Message.MsgType.system);
    }

    /**
        la fonction réinitilise les compteurs de votes
    **/
    public void EndVote(){
        for(int i=0; i<nbPlayer; i++){
            Toggle toggleCard = listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>();
            GameObject cardVote = toggleCard.transform.Find("NbVote").gameObject;
            TextMeshProUGUI nbVote = cardVote.transform.Find("TextNb").GetComponent<TextMeshProUGUI>();
            nbVote.text = "0";

            cardVote.SetActive(false);
            listPlayer[i].SetVote(-1);
        }
    }

    /**
        la fonction met a jour les compteurs de votes de chaque joueur
    **/
    public void UpdateVote(){
        for(int i=0; i<nbPlayer; i++){
            Toggle toggleCard = listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>();
            GameObject cardVote = toggleCard.transform.Find("NbVote").gameObject;
            TextMeshProUGUI nbVote = cardVote.transform.Find("TextNb").GetComponent<TextMeshProUGUI>();
            nbVote.text = "0";
            cardVote.SetActive(false);
        }
        for(int i=0; i<nbPlayer; i++){
            int vote = listPlayer[i].GetVote();
            if(vote != -1){
                int indice = chercheIndiceJoueurId(vote);
                Toggle toggleCard = listCard[indice].transform.Find("Toggle-Card").GetComponent<Toggle>();
                GameObject cardVote = toggleCard.transform.Find("NbVote").gameObject;
                TextMeshProUGUI nbVote = cardVote.transform.Find("TextNb").GetComponent<TextMeshProUGUI>();
                nbVote.text = "" + (int.Parse(nbVote.text)+1);

                cardVote.SetActive(true);
            }
        }
    }

    /**
        la fonction affiche le nombre de points gagnés par chaque joueur
        a la fin de la partie sur le win-screen
    **/
    public void afficheScore(int score) {
        nbPoints.text = "+ " + score + " Points"; 
    }

}



[System.Serializable]
public class Message
{

    public string msg;
    public TextMeshProUGUI textComponent;
    public MsgType type;

    public enum MsgType
    {
        player,
        system,
        loup
    }

}

[System.Serializable]
public class Player
{
    private string role = "Villageois";
    private string pseudo = "Pseudo";
    private bool isAlive = true;
    private bool isMarried = false;
    private int id;
    private int roleId;
    private bool seen = false;
    private int vote = -1;
    private bool isMaire;

    public Player() { }

    public Player(string p, string r, int rid, int id, bool alive = true, bool seen = false)
    {
        pseudo = p;
        role = r;
        this.id = id;
        roleId = rid;
        isAlive = alive;
    }

    public string GetRole()
    {
        return role;
    }
    public int GetRoleId()
    {
        return roleId;
    }
    public string GetPseudo()
    {
        return pseudo;
    }
    public bool GetIsAlive()
    {
        return isAlive;
    }

    public void SetIsMaire(bool m) {
        isMaire = m;
    }

    public bool GetIsMaire() {
        return isMaire;
    }

    public bool GetIsMarried(){
        return isMarried;
    }
    public int GetId()
    {
        return id;
    }
    public bool GetSeen(){
        return seen;
    }
    public int GetVote(){
        return vote;
    }
    public void SetSeen(bool s){
        seen = s;
    }
    public void SetIsAlive(bool alive)
    {
        isAlive = alive;
    }
    public void SetIsMarried(bool m){
        isMarried = m;
    }
    public void SetVote(int idvote){
        vote = idvote;
    }
    public void SetRole(int rid)
    {
        roleId = rid;
        switch (rid)
        {
            case 1:
                role = "Villageois";
                break;
            case 2:
                role = "Cupidon";
                break;
            case 3:
                role = "Voyante";
                break;
            case 4:
                role = "Loup-garou";
                break;
            case 5:
                role = "Sorciere";
                break;
            case 6:
                role = "Chasseur";
                break;
            case 7:
                role = "Dictateur";
                break;
            case 8:
                role = "Garde";
                break;

        }
    }
}

