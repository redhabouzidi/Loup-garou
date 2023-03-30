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
    bool finished = false;
    // jeu
    private int nbPlayer = NetworkManager.nbplayeres;
    public List<Player> listPlayer = new List<Player>();
    public List<GameObject> listCard = new List<GameObject>();
    private List<Toggle> toggleOn = new List<Toggle>();
    public GameObject cardContainer, cardComponent, GO_dead_bg, GO_rolesRestant, GO_tourRoles;
    public static bool isNight = true;
    public static int tour = 0,turn; 
    public TextMeshProUGUI timer;
    public static float value_timer;
    // timer pour le texte qui s'affiche a l'ecran
    private float timer_text_screen = 2f;
    private bool text_screen_active = false;

    public Color colorRed, colorWhite, colorBlack;
    public TextMeshProUGUI text_day, player_role, text_screen;
    public GameObject panel_text_screen;
    public Sprite VoyanteSprite, VillageoisSprite, LoupSprite, CupidonSprite, SorciereSprite;


    public Button buttonValiderVote;
    public GameObject GO_buttonAfficheCarte;

    // win screen
    public bool gameover = false;
    public bool isVillageWin = true;
    public GameObject winPanel, textWinPlayer;
    List<TextMeshProUGUI> listTextwin = new List<TextMeshProUGUI>();
    public TextMeshProUGUI groupWin;

    public GameObject gamePage, winScreenPage;
    //Cupidon
    public int lover1_id=-1;
    public int lover2_id=-1;

    // variable pour le chat
    private int maxMsg = 50;
    List<Message> msgList = new List<Message>();
    public GameObject chatPanel, textComponent;
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
        Image dead_bg = GO_dead_bg.GetComponent<Image>();
        dead_bg.enabled = false;
        Button buttonAfficheCarte = GO_buttonAfficheCarte.GetComponent<Button>();
        Button buttonOui = choixAction.transform.Find("Button-Oui").GetComponent<Button>();
        Button buttonNon = choixAction.transform.Find("Button-Non").GetComponent<Button>();
        sendChat.onClick.AddListener(OnButtonClickSendMsg);
        buttonNon.onClick.AddListener(OnButtonClickNon);
        buttonOui.onClick.AddListener(OnButtonClickOui);
        buttonLeaveGame.onClick.AddListener(OnButtonClickLeave);
        buttonValiderVote.onClick.AddListener(OnButtonClickVote);
        buttonAfficheCarte.onClick.AddListener(OnButtonClickAffiche);


        NetworkManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        foreach (WPlayer p in NetworkManager.players)
        {
            switch (p.GetRole())
            {
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
            }
        }
        
        AfficherJour();
        AfficheCard();
        listerRoles();
        MiseAJourAffichage();
        finished = true;

    }

    // Update is called once per frame
    void Update()
    {

        if(finished)
        {

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
                    NetworkManager.sendchatMessage(NetworkManager.client, msg);
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
        if (turn != 0)
        {
                
                switch (turn)
            {
                
                case 1:
                    affiche_tour_role("C'est le tour du village", turn);
                    break;
                case 2:
                    affiche_tour_role("C'est le tour du Cupidon", turn);
                        if (p.GetRoleId() == 2)
                        {
                            actionCupidon();
                        }
                        break;
                case 3:
                    affiche_tour_role("C'est le tour du Voyante", turn);
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
            }
                turn = 0;

            }
        AfficheTimer();
        Timer_text_screen();
        
        }

    }

    private void OnButtonClickSendMsg()
    {
        if (inputChat.text != "")
        {
            string msg = p.GetPseudo() + ": " + inputChat.text.ToString();
            NetworkManager.sendchatMessage(NetworkManager.client, msg);
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

    private void OnButtonClickAffiche(){
        AfficheVoyante();
    }

    private void OnButtonClickNon() {
        choixAction.SetActive(false);
        NetworkManager.Vote(NetworkManager.client, NetworkManager.id,0);
        // envoyer au serveur NON
    }

    private void OnButtonClickOui() {
        choixAction.SetActive(false);
        NetworkManager.Vote(NetworkManager.client, NetworkManager.id, 1);
        // envoyer au serveur OUI
    }

    public void affiche_tour_role(string msg,int tour)
    {
        if (p.GetRoleId() != tour && tour!= 1)
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
        
        if (isNight == false)
        {
            choixAction.SetActive(false);
            text_day.text = "Day " + tour;
            text_day.color = colorWhite;
            player_role.color = colorWhite;
        }
        else
        {
            text_day.text = "Night " + tour;
            text_day.color = colorRed;
            player_role.color = colorRed;

            // quand c'est au tour de la voyante
            if(player_role.text == "Voyante"){
                GO_buttonAfficheCarte.SetActive(true);
            }
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
        }
    }

    public void AjoutTextWin(GameObject panel, int num)
    {
        TextMeshProUGUI newText;
        GameObject newPlayer = Instantiate(textWinPlayer, panel.transform);
        newText = newPlayer.GetComponent<TextMeshProUGUI>();
        newText.text = listPlayer[num].GetPseudo() + ": " + listPlayer[num].GetRole();
        Debug.Log(listPlayer[num].GetRole());
        if (listPlayer[num].GetRole() == "Loup-garou")
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

        if (isVillageWin == false)
        {
            groupWin.text = "Loup-garou win";
            groupWin.color = colorRed;
        }
        
        for (int i = 0; i < nbPlayer; i++)
        {
            AjoutTextWin(winPanel, i);
        }

    }
    public void AfficheAmoureux(){
         if(lover1_id==-1||lover2_id==-1)return;
        if(p.GetRole()=="Cupidon"||p.GetId()==lover1_id||p.GetId()==lover2_id){
            GameObject[]  textpseudos= GameObject.FindGameObjectsWithTag("Pseudos");
            foreach(GameObject go in textpseudos){
                TextMeshProUGUI texto = go.GetComponent<TextMeshProUGUI>();
                if(texto!=null){
                    int id1 = chercheIndiceJoueurId(lover1_id), id2 = chercheIndiceJoueurId(lover2_id);
                    Debug.Log("id1= " + id1 + "id2 = " + id2+"lvid= "+ lover1_id+" lvid= "+ lover2_id);
                    if(texto.text==listPlayer[id1].GetPseudo()||texto.text==listPlayer[id2].GetPseudo()){
                        texto.color=Color.magenta;
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
        text.text = listPlayer[id].GetPseudo();
        switch(listPlayer[id].GetRole()) {
            case "Loup-garou":
                roleImg.sprite = LoupSprite;
                break;
            case "Voyante":
                roleImg.sprite = VoyanteSprite;
                break;
            case "Cupidon":
                roleImg.sprite = CupidonSprite;
                break;
            case "Sorciere":
                roleImg.sprite = SorciereSprite;
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
        for (int i = 0; i < nbPlayer; i++)
        {
            AjoutCarte(i);
        }

    }

    public void OnToggleValueChanged(Toggle change){
        bool value = change.isOn;
        if(value){
            toggleOn.Add(change);
            if(p.GetRole() == "Cupidon" && isNight){
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
            NetworkManager.ChooseLovers(NetworkManager.client, NetworkManager.id, id1, id2);


        }
        else {
            SendMessageToChat("Tu dois choisir deux personnes a marier!", Message.MsgType.system);
        }
    }

    public void Vote()
    {
        if(p.GetRole() == "Cupidon" && isNight){
            actionCupidon();
        }
        else{
            int selectedId = GetIndiceToggleOn();
            if(selectedId != -1){
                SendMessageToChat("Tu as voté pour "+listPlayer[selectedId].GetPseudo(), Message.MsgType.system);
                NetworkManager.Vote(NetworkManager.client, NetworkManager.id, listPlayer[selectedId].GetId());
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
        Image lgIcon = toggleCard.transform.Find("lg-icon").GetComponent<Image>();

        eyeImg.enabled = false;
        lgIcon.enabled = false;

        if (!listPlayer[indice].GetIsAlive())
        {
            text.text = listPlayer[indice].GetPseudo() + " - mort\n" + listPlayer[indice].GetRole();
            text.color = colorRed;
            toggleCard.interactable = false;
            roleImg.enabled = true;


            // changer la couleur de la carte
            ColorBlock colors = toggleCard.colors;
            colors.disabledColor = colorBlack;
            toggleCard.colors = colors;
        }

        if(p.GetRole() == "Voyante" && listPlayer[indice].GetSeen()) {
            roleImg.enabled = true;
            eyeImg.enabled = true;            
        }

        if(p.GetRole() == "Loup-garou" && listPlayer[indice].GetRole() == "Loup-garou") {
            lgIcon.enabled = true;
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
                NetworkManager.Vote(NetworkManager.client, NetworkManager.id, listPlayer[selectedId].GetId());
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
    private int id;
    private int roleId;
    private bool seen = false;

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
    public int GetId()
    {
        return id;
    }
    public bool GetSeen(){
        return seen;
    }
    public void SetSeen(bool s){
        seen = s;
    }
    public void SetIsAlive(bool alive)
    {
        isAlive = alive;
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
        }
    }
}
