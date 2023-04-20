using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;


public class GameManager : MonoBehaviour
{
    // joueur
    public Player p;
    // jeu
    private int nbPlayer;
    public List<Player> listPlayer = new List<Player>();
    public List<GameObject> listCard = new List<GameObject>();
    private List<Toggle> toggleOn = new List<Toggle>();
    public GameObject cardContainer, cardComponent, GO_dead_bg, GO_rolesRestant, GO_tourRoles;
    public static bool isNight = true,action=false;
    public static int tour = 0,turn; 
    public TextMeshProUGUI timer;
    public static float value_timer;
    private bool sestPresente = false, electionMaire = false;
    // timer pour le texte qui s'affiche a l'ecran
    private float timer_text_screen = 2f;
    private bool text_screen_active = false;

    public Color colorRed, colorWhite, colorBlack, colorYellow, colorPink, colorBlue;
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
    public TextMeshProUGUI groupWin;

    public GameObject gamePage, winScreenPage;
    //Cupidon
    public int lover1_id=-1;
    public int lover2_id=-1;

    // variable pour le chat
    private int maxMsg = 100;
    List<Message> msgList = new List<Message>();
    public GameObject chatPanel, textComponent, chatNotification;
    public TMP_InputField inputChat;
    public Button sendChat;
    public Color playerC, systemC;

    // choix pendant l'action
    public GameObject choixAction;

    // options page
    public Button buttonLeaveGame;

    

    // Start is called before the first frame update
    void Start()
    {
        nbPlayer = NetworkManager.nbplayeres;
        Image dead_bg = GO_dead_bg.GetComponent<Image>();
        dead_bg.enabled = false;
        Button buttonAfficheCarte = GO_buttonAfficheCarte.GetComponent<Button>();
        Button buttonOui = choixAction.transform.Find("Button-Oui").GetComponent<Button>();
        Button buttonNon = choixAction.transform.Find("Button-Non").GetComponent<Button>();
        sendChat.onClick.AddListener(OnButtonClickSendMsg);
        buttonNon.onClick.AddListener(OnButtonClickNon);
        buttonOui.onClick.AddListener(OnButtonClickOui);
        buttonRole.onClick.AddListener(OnButtonClickRole);
        buttonLeaveGame.onClick.AddListener(OnButtonClickLeave);
        buttonValiderVote.onClick.AddListener(OnButtonClickVote);
        buttonAfficheCarte.onClick.AddListener(OnButtonClickAffiche);
        buttonLeave.onClick.AddListener(OnButtonClickLeaveGame);
        buttonPlayAgain.onClick.AddListener(OnButtonClickPlayAgain);
        sePresenter.onClick.AddListener(OnButtonClickSePresenter);

        NetworkManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        isNight = true;
        tour = 0;
        foreach (WPlayer p in NetworkManager.players)
        {
            switch (p.GetRole())
            {
                case 0:
                    listPlayer.Add(new Player(p.GetUsername(), "Villageois", 0, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Villageois", 0, p.GetId(), true);
                        player_role.text = "Villageois";
                    }
                    break;
                case 1:
                    listPlayer.Add(new Player(p.GetUsername(), "Villageois", 1, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Villageois", 1, p.GetId(), true);
                        player_role.text = "Villageois";
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

                        player_role.text = "Voyante";
                    }
                    break;
                case 4:
                    listPlayer.Add(new Player(p.GetUsername(), "Loup-garou", 4, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Loup-garou", p.GetRole(), p.GetId(), true);

                        player_role.text = "Loup-garou";
                    }
                    break;
                case 5:
                    listPlayer.Add(new Player(p.GetUsername(), "Sorciere", 5, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p=new Player(p.GetUsername(), "Sorciere", p.GetRole(), p.GetId(), true);
                        player_role.text = "Sorciere";

                    }
                    break;
                case 6:
                    listPlayer.Add(new Player(p.GetUsername(), "Chasseur", 6, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Chasseur", p.GetRole(), p.GetId(), true);
                        player_role.text = "Chasseur";

                    }
                    break;
                case 8:
                    listPlayer.Add(new Player(p.GetUsername(), "Garde", 8, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Garde", p.GetRole(), p.GetId(), true);
                        player_role.text = "Garde";

                    }
                    break;
                case 7:
                    listPlayer.Add(new Player(p.GetUsername(), "Dictateur", 7, p.GetId(), true));
                    if (NetworkManager.id == p.GetId())
                    {
                        this.p = new Player(p.GetUsername(), "Dictateur", p.GetRole(), p.GetId(), true);
                        player_role.text = "Dictateur";

                    }
                    break;
            }
        }
        
        AfficherJour();
        AfficheCard();
        listerRoles();
        MiseAJourAffichage();
        InitPotion();
        EndVote();

    }

    // Update is called once per frame
    void Update()
    {
        NetworkManager.listener();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadScene("jeu");
        }
        if (inputChat.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                
                if (inputChat.text != "")
                {
                    string msg = p.GetPseudo() + ": " + inputChat.text.ToString();
                    NetworkManager.sendchatMessage( msg);
                    inputChat.text = "";
                    inputChat.ActivateInputField();
                }
            }
        }
        if (gameover)
        {
            gamePage.transform.gameObject.SetActive(false);
            winScreenPage.transform.gameObject.SetActive(true);
            AfficheWinScreen();
            gameover = false;
        }
        
        AfficheTimer();
        AfficherJour();
        Timer_text_screen();
        changeTurn();
        

    }
    private void OnButtonClickSePresenter()
    {
        SendMessageToChat("" + p.GetPseudo() + " se présente en tant que Maire !", Message.MsgType.system);
        sestPresente = true;
    }
    private void OnButtonClickLeaveGame()
    {
        GameManagerApp.client = NetworkManager.client;
        GameManagerApp.scene = 1;
        LoadScene("Jeu");


    }
    private void OnButtonClickPlayAgain()
    {
        GameManagerApp.client = NetworkManager.client;
        GameManagerApp.scene = 2;
        LoadScene("Jeu");
    }
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

    private void OnButtonClickLeave()
    {
        LoadScene("jeu");
    }

    private void OnButtonClickVote()
    {
        Vote();
        
    }
    public void changeTurn()
    {
        if (turn!=0)
        {
            action = false;
            electionMaire = false;
            switch (turn)
            {

                case 1:
                    affiche_tour_role("C'est le tour du village", turn);
                    break;
                case 2:
                    affiche_tour_role("C'est le tour du Cupidon", turn);
                    if (p.GetRoleId() == 2)
                    {
                        action=true;
                        Debug.Log("tour du cupi");
                        actionCupidon();
                    }
                    break;
                case 3:
                    affiche_tour_role("C'est le tour de la Voyante", turn);
                    break;
                case 4:
                    affiche_tour_role("C'est le tour du loup", turn);
                    break;
                case 5:
                    affiche_tour_role("C'est le tour de la sorciere", turn);
                    if (p.GetRoleId() == 5)
                    {
                        Debug.Log("je demande a la sorciere si elle veut utiliser sa posion de mort ou non");
                        affiche_choix_action("Veux tu tuer une personne?");
                    }
                    break;
                case 6:
                    affiche_tour_role("C'est le tour du chasseur", turn);
                    break;
                case 8:
                    affiche_tour_role("C'est le tour du Garde", turn);
                    break;
                case 7:
                    affiche_tour_role("C'est le tour du Dictateur", turn);
                    break;
                case 255:
                    affiche_tour_role("C'est le tour du maire", turn);
                    electionMaire = true;
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
    private void OnButtonClickAffiche(){
        AfficheVoyante();
    }

    private void OnButtonClickRole(){
        bool active = GO_rolesRestant.activeSelf;
        GO_rolesRestant.SetActive(!active);
    }

    private void OnButtonClickNon() {
        choixAction.SetActive(false);
        NetworkManager.Vote( NetworkManager.id,0);
        // envoyer au serveur NON
    }

    private void OnButtonClickOui() {
        choixAction.SetActive(false);
        NetworkManager.Vote( NetworkManager.id, 1);
        // envoyer au serveur OUI
    }

    public void affiche_tour_role(string msg,int tour)
    {
        
        if (p.GetRoleId() != tour && tour!= 1 && tour!=255)
        {
            GO_tourRoles.SetActive(true);
        }
        else
        {
            GO_tourRoles.SetActive(false);
        }
        
        TextMeshProUGUI text_role = GO_tourRoles.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        text_role.text = msg;
        
    }
    public void AfficherJour()
    {
        if (electionMaire)
        {
            text_day.text = "Election du maire";
            text_day.color = colorBlue;
            player_role.text = "Depot des candidatures";
            player_role.color = colorBlue;
            if (!sestPresente) sePresenter.gameObject.SetActive(true);
            else sePresenter.gameObject.SetActive(false);
        }

        else if (isNight == false)
        {
            text_day.text = "Day " + tour;
            text_day.color = colorWhite;
            player_role.color = colorWhite;
            player_role.text = p.GetRole();
            sePresenter.gameObject.SetActive(false);
        }
        else
        {
            text_day.text = "Night " + tour;
            text_day.color = colorRed;
            player_role.color = colorRed;
            player_role.text = p.GetRole();
            sePresenter.gameObject.SetActive(false);
        }
    }

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

    public void SendMessageToChat(string text, Message.MsgType type)
    {
        if (msgList.Count > maxMsg)
        {
            Destroy(msgList[0].textComponent.gameObject);
            msgList.Remove(msgList[0]);
        }
        GameObject newText = Instantiate(textComponent, chatPanel.transform);
        Message newMsg = new Message();

        newMsg.msg = text;
        newMsg.textComponent = newText.GetComponent<TextMeshProUGUI>();
        newMsg.textComponent.text = newMsg.msg;
        newMsg.textComponent.color = MessageColor(type);

        msgList.Add(newMsg);

        if(ButtonClick.isHide && type == Message.MsgType.player){
            chatNotification.SetActive(true);
        }
        else{
            chatNotification.SetActive(false);
        }

    }

    private Color MessageColor(Message.MsgType type)
    {
        Color color;
        switch (type)
        {
            case Message.MsgType.player:
                color = playerC;
                break;
            default:
                color = systemC;
                break;
        }
        return color;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

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

    public int GetIndiceToggleOn(){
        int indice = -1;
        for(int i = 0; i < nbPlayer; i++) {
            if(listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn) {
                indice = i;
            }
        }
        return indice;
    }

    public void AllToggleOff(){
        for(int i = 0; i < nbPlayer; i++) {
            listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = false;
        }
    }

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
            msg = listPlayer[indice1].GetPseudo() + " et " + listPlayer[indice2].GetPseudo() + " sont tombes amoureux l'un de l'autre";
            lover2_id= indice2;
            SendMessageToChat(msg, Message.MsgType.system);
            NetworkManager.ChooseLovers( NetworkManager.id, id1, id2);


        }
        else {
            SendMessageToChat("Tu dois choisir deux personnes a marier!", Message.MsgType.system);
        }
    }

    public void setAmoureux(int id1, int id2){
        int indice1 = chercheIndiceJoueurId(id1);
        int indice2 = chercheIndiceJoueurId(id2);
        p.SetIsMarried(true);
        listPlayer[indice1].SetIsMarried(true);
        listPlayer[indice2].SetIsMarried(true);
    }

    public void Vote()
    {
        if(p.GetRoleId() ==2 && action){
            actionCupidon();
        }
        else{
            int selectedId = GetIndiceToggleOn();
            if(selectedId != -1){
                SendMessageToChat("Tu as voté pour "+listPlayer[selectedId].GetPseudo(), Message.MsgType.system);
                p.SetVote(listPlayer[selectedId].GetId());
                NetworkManager.Vote( NetworkManager.id, listPlayer[selectedId].GetId());
                Debug.Log($"joueur {NetworkManager. id} vote pour {listPlayer[selectedId].GetId()}");
            } else{
                SendMessageToChat("Tu as voté pour personne, pitié vote >:(", Message.MsgType.system);
            } 
        }

        AllToggleOff();
    }

    public void OnOff()
    {
        for(int i=0; i<nbPlayer; i++){
            listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn = false;
            listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().interactable = !listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().interactable;
        }
        sendChat.interactable = !sendChat.interactable;
        inputChat.interactable = !inputChat.interactable;
    }

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
        }

        if (!listPlayer[indice].GetIsAlive())
        {
            text.text = listPlayer[indice].GetPseudo() + " - mort\n" + listPlayer[indice].GetRole();
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

    public void MiseAJourAffichage()
    {
        for (int i = 0; i < listPlayer.Count; i++)
        {
            MiseAJourCarte(i);
        }
    }

    public void AfficheVoyante() {
        int selectedId = GetIndiceToggleOn();
        if(player_role.text == "Voyante" && isNight) {
            if(selectedId != -1 && selectedId < nbPlayer)
            {
                NetworkManager.Vote( NetworkManager.id, listPlayer[selectedId].GetId());
            }
        } 
    }

    public void affiche_choix_action(string msg){
        choixAction.SetActive(true);
        TextMeshProUGUI text_action =  choixAction.transform.Find("Text-action").GetComponent<TextMeshProUGUI>();
        text_action.text = msg;
    }

    public void ActionSorciere(int id){
        int indice = chercheIndiceJoueurId(id);
        if (indice == -1) return;   // erreur l'id du joueur ne correspond a aucun joueur
        string msg = "" + listPlayer[indice].GetPseudo() + " est mort. Veux-tu utiliser ta potion de vie?";
        affiche_choix_action(msg);
    }

    public int chercheIndiceJoueurId (int id){
        for(int i=0; i<listPlayer.Count; i++){
            if(listPlayer[i].GetId() == id){
                return i;
            }
        }
        return -1;
    }

    public void Change_text_screen(string msg) {
        timer_text_screen = 2;
        text_screen.text = msg;
        text_screen_active = true;
    }

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

    public void affiche_text_role(int id, int idrole)
    {
        GO_buttonAfficheCarte.SetActive(false);
        panel_text_screen.SetActive(true);
        int indice = chercheIndiceJoueurId(id);
        string msg = listPlayer[indice].GetPseudo() + " est " + idRoleToStringRole(idrole);
        listPlayer[indice].SetSeen(true);
        SendMessageToChat(msg, Message.MsgType.system);
        Change_text_screen(msg);
    }

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

    public void LITTERALLYDIE() {
        Image dead_bg = GO_dead_bg.GetComponent<Image>();
        if(p.GetIsAlive() == false) dead_bg.enabled = true;
    }

    public void listerRoles() {
        List<string> roleList = new List<string>();
        List<int> nb = new List<int>();
        List<string> nbRoleList = new List<string>();
        int count = 0;

        for(int i = 0; i<nbPlayer; i++) {
            if(!roleList.Contains(listPlayer[i].GetRole()) && listPlayer[i].GetIsAlive()) {
                roleList.Add(listPlayer[i].GetRole());
                for(int j = 0; j<nbPlayer; j++) {
                    if(listPlayer[j].GetRole() == listPlayer[i].GetRole()) count ++;
                }
                nb.Add(count);
                count = 0;
            }
        }


        //SendMessageToChat("Il y a :", Message.MsgType.system);
        for(int i = 0; i<roleList.Count; i++) {
            SendMessageToChat("" + nb[i] + " " + roleList[i], Message.MsgType.system);
            nbRoleList.Add("" + nb[i] + " " + roleList[i]);
        }
        string txt = "";
        for(int i = 0; i<nbRoleList.Count; i++) {
            txt += nbRoleList[i];
            txt += "\n";
        }

        TextMeshProUGUI textRolesRestant =  GO_rolesRestant.transform.Find("TexteRole").GetComponent <TextMeshProUGUI>();
        Debug.Log("text role restant = "+GO_rolesRestant.GetComponent<TextMeshProUGUI>());
        
        textRolesRestant.text = txt;
    }

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

    public void UseHealthPotion(){
        Image potionVie = GO_potion.transform.Find("Potion_vie").GetComponent<Image>();
        Image potionVideV = GO_potion.transform.Find("Potion_videV").GetComponent<Image>();
        potionVie.enabled = false;
        potionVideV.enabled = true;
        SendMessageToChat("Tu as utilisé ta potion de vie!", Message.MsgType.system);
    }

    public void UseDeathPotion(){
        Image potionMort = GO_potion.transform.Find("Potion_mort").GetComponent<Image>();
        Image potionVideM = GO_potion.transform.Find("Potion_videM").GetComponent<Image>();
        potionMort.enabled = false;
        potionVideM.enabled = true;
        SendMessageToChat("Tu as utilisé ta potion de mort!", Message.MsgType.system);
    }

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
        system
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
    private bool isMaire = false;

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
            case 8:
                role = "Dictateur";
                break;
            case 7:
                role = "Garde";
                break;

        }
    }
}
