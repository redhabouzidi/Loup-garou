using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Linq;

public class WaitingScreen : MonoBehaviour
{
    public string name;
    public TextMeshProUGUI status, textName;
    //public bool isStart = false;

    public int nbjoueur_rest;
    public bool newGame;
    public int max_player;
    private int index_desc = 0;
    private bool isfr=false;
    public Button buten,butfr;
    // description des roles
    public Image image_carte;
    public Sprite SpriteLoup, SpriteVillageois, SpriteSorciere, SpriteVoyante, SpriteChasseur, SpriteCupidon, SpriteDictateur, SpriteGarde;
    public Button right_button, left_button,button_ready;
    public Color colorWhite, colorGreen;
    public TextMeshProUGUI role_name, descripts;
    public List<Roles> roles_presents=new List<Roles>();
    public TextMeshProUGUI nbPlayerP;
    private string []roles=new string[8]{"Loup","Villageois","Cupidon","Sorciere","Voyante","Chasseur","Dictateur","Garde"};
    private string []roles_en=new string[8]{"Werewolf", "Villager", "Cupid", "Witch", "Seer", "Hunter", "Dictator", "Guard"};
    private string []description=new string[8]{"<color=#ff0000ff><size=22> Les Loups-Garous doivent prendre le dessus sur le village!</size></color>\r\n\n Chaque nuit, ils dévorent un Villageois. Le jour, ils essaient de masquer leur identité nocturne pour échapper à la vindicte  populaire. Leur nombre peut varier suivant le nombre de joueurs. En aucun cas un loup-garou ne peut dévorer un autre loup-garou."
    ,"<color=#ff0000ff> Les Villageois doivent gagner avec le village!</size></color>\r\n\n Il n’a aucune compétence particulière. Ses seules armes sont lacapacité d’analyse des comportements pour identifierles Loups-Garous, et la force de conviction pour empêcher l’exécution de l’innocent qu’il est."
    ,"<color=#ff0000ff> Le Cupidon aide le village à vaincre les Loups-Garous!</size></color>\r\n\n En décrochant ses célèbres flèches magiques, Cupidon a le pouvoir de rendre 2 personnes amoureuses à jamais. La première nuit, il désigne les 2 joueurs amoureux. Cupidon peut, s’il le veut se désigner comme l’un des deux Amoureux. Si l’un des Amoureux est éliminé, l’autre meurt de chagrin immédiatement. Un Amoureux ne doit jamais éliminer son aimé, ni lui porter aucun préjudice."
    ,"<color=#ff0000ff> La Sorcière aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Elle sait concocter 2 potions extrêmement puissantes : Une potion de Guérison, pour ressusciter le joueur dévoré par les Loups-Garous Une potion d’Empoisonnement, utilisé la nuit pour éliminer un joueur. La Sorcière doit utiliser chaque potion une seul fois dans la partie. Elle ne peut pas se servir de ses deux potions la même nuit."
    ,"<color=#ff0000ff> La Voyante aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Chaque nuit, elle voit la carte d’un joueur de son choix. Elle doit aider les autres Villageois, mais rester discrète pour ne pas être démasquée par les Loups-Garous."
    ,"<color=#ff0000ff> Le Chasseur aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Lorsque le chasseur meurt, il peut choisir un joueur à emporter avec lui dans la mort."
    ,"<color=#ff0000ff> Le Dictateur aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Le dictateur peut choisir un joueur à tuer chaque jour. S'il est découvert par les villageois, ils peuvent voter pour l'éliminer. S'il est découvert par les loups-garous, il est éliminé immédiatement"
    ,"<color=#ff0000ff> Le Garde aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Le garde peut choisir un joueur chaque nuit à protéger. Si les loups-garous essaient de tuer ce joueur, il ne mourra pas."
    };
     private string []description_en=new string[8]{"<color=#ff0000ff><size=22> The Werewolves must take over the village!</size></color>\r\n\n Every night they devour a Villager. During the day, they try to hide their nocturnal identity to escape popular vindication. Their number can vary according to the number of players. In no case may a werewolf devour another werewolf."
    ,"<color=#ff0000ff>The Villagers must win with the village!</size></color>\r\n\n He has no special skills. His only weapons are the ability to analyze behavior to identify werewolves, and the strength of conviction to prevent the execution of the innocent man that he is."
    ,"<color=#ff0000ff> Cupid helps the village defeat the Werewolves!</size></color>\r\n\n By unhooking his famous magic arrows, Cupid has the power to make 2 people fall in love forever. On the first night, he designates the two players in love. Cupid can, if he wants, designate himself as one of the two lovers. If one of the lovers is eliminated, the other dies of grief immediately. A lover must never eliminate his beloved, nor harm him in any way."
    ,"<color=#ff0000ff> The Witch helps the village defeat the Werewolves!</size></color>\r\n\n She can concoct 2 extremely powerful potions: A potion of Healing, to resurrect the player devoured by the werewolves A potion of Poisoning, used at night to eliminate a player. The Witch must use each potion only once in the game. She cannot use both potions at the same time."
    ,"<color=#ff0000ff> The Seer helps the village defeat the Werewolves!</size></color>\r\n\n Each night, she sees the card of a player of her choice. She must help the other Villagers, but remain discreet so as not to be unmasked by the Werewolves."
    ,"<color=#ff0000ff> The Hunter helps the village defeat the Werewolves!</size></color>\r\n\n When the Hunter dies, he can choose one player to take with him in death."
    ,"<color=#ff0000ff> The Dictator helps the village defeat the Werewolves!</size></color>\r\n\n The Dictator can choose one player to kill each day. If he is discovered by the villagers, they can vote to eliminate him. If he is discovered by the werewolves, he is eliminated immediately."
    ,"<color=#ff0000ff> The Guard helps the village defeat the Werewolves!</size></color>\r\n\n The Guard can choose one player each night to protect. If the werewolves try to kill that player, he will not die."
    };
    public List<WPlayer> players_waiting;
    private int no_players = 0;
    // affichage des cartes des joueurs
    public GameObject cardContainer, cardComponent;
    private List<GameObject> listCard = new List<GameObject>();
    public Color colorNone, colorCard;
    // Start is called before the first frame update
    public void Start()
    {
        isfr=Traduction.fr;
        if(isfr)refresh_to_fr();
        else refresh_to_en();
        left_button.onClick.AddListener(left_previous);
        right_button.onClick.AddListener(right_next);
        button_ready.onClick.AddListener(toggleReady);
        buten.onClick.AddListener(refresh_to_en);
        butfr.onClick.AddListener(refresh_to_fr);

        AfficheCard();
    }

    // Update is called once per frame
    void Update()
    {
        if (newGame)
        {
            newGame = false;
            initialize();
        }
        //verifier nombre de joeurs reste pour commencer la partie
        if (nbjoueur_rest >= 0)
        {
            if(isfr){
                status.text = nbjoueur_rest + " restants...";
            }
            else{
                status.text = nbjoueur_rest + " remaining...";
            }
            
        }
        else
        {
            if(isfr){
                status.text = " préparez-vous";
            }
            else{
                status.text = " ready yourselves";
            }
        }

    }
    public void toggleReady()
    {
        NetworkManager.sendReady();
    }
    //Fonction permettant d'obtenir le role suivant en cliquant le button droite
    void right_next()
    {   Debug.Log(""+roles_presents.Count+" index "+index_desc);
        if(roles_presents.Count==0)return;
        if(index_desc==roles_presents.Count-1)index_desc=0;
        else index_desc++;
        Debug.Log(""+roles_presents.Count+" index "+index_desc);
        descripts.text=roles_presents[index_desc].get_description();
        string role_count=" "+roles_presents[index_desc].get_role_count();
        if(roles_presents[index_desc].get_role_count()>1){
            if(isfr){
                role_count+=" Joueurs";
            }
            else{
                role_count+=" Players";
            }
        }
        else{
            if(isfr){
                role_count+=" Joueur";
            }
            else{
                role_count+=" Player";
            }
        }
        nbPlayerP.text=role_count;
        role_name.text=roles_presents[index_desc].get_role();
        change_image();
    }
    //Fonction permettant d'obtenir le role precedent en cliquant le button gauche
    void left_previous()
    {
        Debug.Log(""+roles_presents.Count+" index "+index_desc);
        if(roles_presents.Count==0)return;
        if(index_desc==0)index_desc=roles_presents.Count-1;
        else index_desc--;
        descripts.text=roles_presents[index_desc].get_description();
        string role_count=" "+roles_presents[index_desc].get_role_count();
        if(roles_presents[index_desc].get_role_count()>1){
            if(isfr){
                role_count+=" Joueurs";
            }
            else{
                role_count+=" Players";
            }
        }
        else{
            if(isfr){
                role_count+=" Joueur";
            }
            else{
                role_count+=" Player";
            }
        }
        nbPlayerP.text=role_count;
        role_name.text=roles_presents[index_desc].get_role();
        change_image();
    }
    void refresh_to_en(){//"Loup","Villageois","Cupidon","Sorciere","Voyante","Chasseur","Dictateur","Garde"
        textName.text = "Waiting screen: " + name;
        for(int i=0;i<roles_presents.Count;i++){
            switch(roles_presents[i].get_role()){
                case "Loup":
                    roles_presents[i].set_description(description_en[0]);
                    roles_presents[i].set_role(roles_en[0]);
                    Debug.Log("Loup ");
                    break;
                case "Villageois":
                    roles_presents[i].set_description(description_en[1]);
                    roles_presents[i].set_role(roles_en[1]);
                    break;
                case "Cupidon":
                    roles_presents[i].set_description(description_en[2]);
                    roles_presents[i].set_role(roles_en[2]);
                    break;
                case "Sorciere":
                    roles_presents[i].set_description(description_en[3]);
                    roles_presents[i].set_role(roles_en[3]);
                    Debug.Log("Sorc ");
                    break;
                case "Voyante":
                    roles_presents[i].set_description(description_en[4]);
                    roles_presents[i].set_role(roles_en[4]);
                    Debug.Log("Voyan ");
                    break;
                case "Chasseur":
                    roles_presents[i].set_description(description_en[5]);
                    roles_presents[i].set_role(roles_en[5]);
                    Debug.Log("Chasseur ");
                    break;
                case "Dictateur":
                    roles_presents[i].set_description(description_en[6]);
                    roles_presents[i].set_role(roles_en[6]);
                    Debug.Log("Dictateur ");
                    break;
                case "Garde":
                    roles_presents[i].set_description(description_en[7]);
                    roles_presents[i].set_role(roles_en[7]);
                    Debug.Log("Garde ");
                    break;
            }
        }
        role_name.text=roles_presents[index_desc].get_role();
        descripts.text=roles_presents[index_desc].get_description();
        string role_count=" "+roles_presents[index_desc].get_role_count();
        if(roles_presents[index_desc].get_role_count()>1){
            role_count+=" Players";
        }
        else{
            role_count+=" Player";
        }
        nbPlayerP.text=role_count;
        isfr=false;
    }
    void refresh_to_fr(){//"Wolf", "Villager", "Cupid", "Witch", "Seer", "Hunter", "Dictator", "Guard"
        textName.text = "Écran d'attente: " + name;
        for(int i=0;i<roles_presents.Count;i++){
            switch(roles_presents[i].get_role()){
                case "Werewolf":
                    roles_presents[i].set_description(description[0]);
                    roles_presents[i].set_role(roles[0]);
                    Debug.Log("Loup ");
                    break;
                case "Villager":
                    roles_presents[i].set_description(description[1]);
                    roles_presents[i].set_role(roles[1]);
                    break;
                case "Cupid":
                    roles_presents[i].set_description(description[2]);
                    roles_presents[i].set_role(roles[2]);
                    break;
                case "Witch":
                    roles_presents[i].set_description(description[3]);
                    roles_presents[i].set_role(roles[3]);
                    Debug.Log("Sorc ");
                    break;
                case "Seer":
                    roles_presents[i].set_description(description[4]);
                    roles_presents[i].set_role(roles[4]);
                    Debug.Log("Voyan ");
                    break;
                case "Hunter":
                    roles_presents[i].set_description(description[5]);
                    roles_presents[i].set_role(roles[5]);
                    Debug.Log("Chasseur ");
                    break;
                case "Dictator":
                    roles_presents[i].set_description(description[6]);
                    roles_presents[i].set_role(roles[6]);
                    Debug.Log("Dictateur ");
                    break;
                case "Guard":
                    roles_presents[i].set_description(description[7]);
                    roles_presents[i].set_role(roles[7]);
                    Debug.Log("Garde ");
                    break;
            }
        }
        role_name.text=roles_presents[index_desc].get_role();
        descripts.text=roles_presents[index_desc].get_description();
        string role_count=" "+roles_presents[index_desc].get_role_count();
        if(roles_presents[index_desc].get_role_count()>1){
            role_count+=" Joueurs";
        }
        else{
            role_count+=" Joueur";
        }
        nbPlayerP.text=role_count;
        isfr=true;
    }
    public void initialize()
    {
        if(isfr){
            textName.text = "Écran d'attente: " + name;
        }
        else{
            textName.text = "Waiting screen: " + name;
        }
        no_players = 0;
        players_waiting = new List<WPlayer>();
        max_player = NetworkManager.nbplayeres;
        nbjoueur_rest = max_player;
        AfficheCard();

        //le nombre des roles presents dans la partie


        descripts.text = roles_presents[index_desc].get_description();
        string role_count = " " + roles_presents[index_desc].get_role_count();
        if (roles_presents[index_desc].get_role_count() > 1){
            if(isfr){
                role_count+=" Joueurs";
            }
            else{
                role_count+=" Players";
            }
        }
        else{
            if(isfr){
                role_count+=" Joueur";
            }
            else{
                role_count+=" Player";
            }
        }
        Debug.Log(role_count);
        nbPlayerP.text = role_count;
        role_name.text = roles_presents[index_desc].get_role();
        change_image();//Charger le premier image



        //isStart = true;
        foreach (WPlayer p in NetworkManager.players)
        {
            addplayer(p.GetUsername(), p.GetId(),p.GetReady());
        }
    }
    //Fonction permettant de ajouter un joueur avec son nom d'utilisateur
    public void addplayer(string username, int id,bool readyp)
    {
        if(players_waiting.Count >= max_player){
            return;
        }
        players_waiting.Add(new WPlayer(username, id,readyp));
        no_players++;

        if (nbjoueur_rest != 0) nbjoueur_rest--;

        Debug.Log(no_players.ToString()+"Fonction addplayer " +username);
        AffichageUsernameText(); 
    }
    public void add_role(int[] rolp,int[] number){
        roles_presents.Clear();
        for(int i=0;i<rolp.Length;i++){
            switch(rolp[i]){
                case 4:
                    roles_presents.Add(new Roles(rolp[i], roles[0],description[0],number[i], SpriteLoup));
                    Debug.Log("Loup ");
                    break;
                case 1:
                    roles_presents.Add(new Roles(rolp[i], roles[1],description[1],number[i], SpriteVillageois));
                    Debug.Log("Vill ");
                    break;
                case 2:
                    roles_presents.Add(new Roles(rolp[i], roles[2],description[2],number[i], SpriteCupidon));
                    Debug.Log("Cupi ");
                    break;
                case 5:
                    roles_presents.Add(new Roles(rolp[i], roles[3],description[3],number[i], SpriteSorciere));
                    Debug.Log("Sorc ");
                    break;
                case 3:
                    roles_presents.Add(new Roles(rolp[i], roles[4],description[4],number[i], SpriteVoyante));
                    Debug.Log("Voyan ");
                    break;
                case 6:
                    roles_presents.Add(new Roles(rolp[i], roles[5],description[5],number[i], SpriteChasseur));
                    Debug.Log("Chasseur ");
                    break;
                case 7:
                    roles_presents.Add(new Roles(rolp[i], roles[6],description[6],number[i], SpriteDictateur));
                    Debug.Log("Dictateur ");
                    break;
                case 8:
                    roles_presents.Add(new Roles(rolp[i], roles[7],description[7],number[i], SpriteGarde));
                    Debug.Log("Garde ");
                    break;
            }
        }
        GameManager.roleRestant = roles_presents;
    }
    //Fonction permettant de supprimer un joueur avec son nom d'utilisateur
    public void quitplayer(string username)
    {
        if (players_waiting.Count == 0) return;

        //trouver un utilisateur avec son nom et le supprimer
        players_waiting.RemoveAll(player => player.GetUsername() == username);
        no_players--;
        nbjoueur_rest++;

        Debug.Log(no_players.ToString());

        AffichageUsernameText();
    }
    public void quitplayer(int id)
    {
        if (players_waiting.Count == 0) return;

        //trouver un utilisateur avec son nom et le supprimer
        players_waiting.RemoveAll(player => player.GetId() == id);
        no_players--;
        nbjoueur_rest++;

        Debug.Log(no_players.ToString());

        AffichageUsernameText();
    }

    public void AffichageUsernameText(){
        for (int i=0; i<no_players; i++){
            Image check = listCard[i].transform.Find("image-card").GetComponent<Image>();
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorCard;
            listCard[i].transform.Find("text-pseudo").GetComponent<TextMeshProUGUI>().text = players_waiting[i].GetUsername();
            listCard[i].transform.Find("card").GetComponent<Image>().enabled = true;
            if(NetworkManager.ready) check.enabled = true;
            else check.enabled = false;

            if(players_waiting[i].readyp) {
                listCard[i].transform.Find("image-card").GetComponent<Image>().enabled = true;
            }
            else listCard[i].transform.Find("image-card").GetComponent<Image>().enabled = false;
        }
        for (int i=no_players; i<max_player; i++){
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorNone;
            listCard[i].transform.Find("card").GetComponent<Image>().enabled = false;
            listCard[i].transform.Find("image-card").GetComponent<Image>().enabled=false;
            listCard[i].transform.Find("text-pseudo").GetComponent<TextMeshProUGUI>().text = "";
        }

    }

    public void AjoutCarte(int id){
        GameObject newCard = Instantiate(cardComponent, cardContainer.transform);
        listCard.Add(newCard);
    }

    public void AfficheCard(){
        foreach(Transform child in cardContainer.transform)
        {
            
            GameObject.Destroy(child.gameObject);
        }
        listCard.Clear();
        for (int i=0; i<max_player; i++){
            AjoutCarte(i);
        }
    }

    public void ChangeReady(int id,bool ready) {
        button_ready.transform.Find("check").GetComponent<Image>().color = colorGreen;
        
        foreach (WPlayer p in players_waiting) {
            if(p.GetId() == id) {
                p.readyp = ready;
                
                if(NetworkManager.id == id) {
                    if(ready) {
                        
                        button_ready.transform.Find("check").GetComponent<Image>().enabled = true;
                        button_ready.GetComponent<Image>().color = colorGreen;
                        button_ready.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = colorGreen;
                        button_ready.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "   Ready!";
                    }

                    else {
                        button_ready.transform.Find("check").GetComponent<Image>().enabled = false;
                        button_ready.GetComponent<Image>().color = colorWhite;
                        button_ready.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().color = colorWhite;
                        button_ready.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Ready?";
                    }
                }
                // mettre le check
                
                if(ready) cardContainer.transform.Find("WaitUserCard(Clone)").transform.Find("image-card").GetComponent<Image>().enabled = true;
                else cardContainer.transform.Find("WaitUserCard(Clone)").transform.Find("image-card").GetComponent<Image>().enabled = false;
            }
        }
        AffichageUsernameText();
        
    }

    void change_image()
    {
        image_carte.sprite = roles_presents[index_desc].get_image();//attribuer
    }
}

//Class pour des joueurs
public class WPlayer
{
    private string username;
    private int id;
    private int role;
    public bool readyp;
    //....

    public WPlayer(string uname, int id,bool readyp)
    {
        this.username = uname;
        this.id = id;
        this.readyp=readyp;
    }
    public bool GetReady(){
        return readyp;
    }
    public string GetUsername()
    {
        return username;
    }
    public int GetId()
    {
        return id;
    }
    public int GetRole()
    {
        return role;
    }
    public void SetRole(int role)
    {
        this.role = role;
    }
}


public class Roles{
    private int idrole;
    private string role;
    private string description;
    private int role_count=1;
    private Sprite image;
    
    public Roles(int idrole, string role,string description,int role_count, Sprite image){
        this.idrole=idrole;
        this.role=role;
        this.description=description;
        this.role_count=role_count;
        this.image=image;
    }
    public int get_idrole(){
        return idrole;
    }
    public string get_role(){
        return role;
    }
    public string get_description(){
        return description;
    }
    public int get_role_count(){
        return role_count;
    }
    public Sprite get_image(){
        return image;
    }
    public void set_role_count(int count){
        role_count = count;
    }
    public void set_role(string role){
        this.role=role;
    }
    public void set_description(string description){
        this.description=description;
    }
}