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

    // description des roles
    public Image image_carte;
    public Sprite SpriteLoup, SpriteVillageois, SpriteSorciere, SpriteVoyante, SpriteChasseur, SpriteCupidon, SpriteDictateur, SpriteGarde;
    public Button right_button, left_button,button_ready;
    public Color colorWhite, colorGreen;
    public TextMeshProUGUI role_name, descripts;
    public List<Roles> roles_presents=new List<Roles>();
    public TextMeshProUGUI nbPlayerP;
    private string []roles=new string[8]{"Loup","Villageois","Cupidon","Sorciere","Voyante","Chasseur","Dictateur","Garde"};
    private string []description=new string[8]{"<color=#ff0000ff><size=22> Les Loups-Garous doivent prendre le dessus sur le village!</size></color>\r\n\n Chaque nuit, ils dévorent un Villageois. Le jour, ils essaient de masquer leur identité nocturne pour échapper à la vindicte  populaire. Leur nombre peut varier suivant le nombre de joueurs. En aucun cas un loup-garou ne peut dévorer un autre loup-garou."
    ,"<color=#ff0000ff> Les Villageois doivent gagner avec le village!</size></color>\r\n\n Il n’a aucune compétence particulière. Ses seules armes sont lacapacité d’analyse des comportements pour identifierles Loups-Garous, et la force de conviction pour empêcher l’exécution de l’innocent qu’il est."
    ,"<color=#ff0000ff> Le Cupidon aide le village à vaincre les Loups-Garous!</size></color>\r\n\n En décrochant ses célèbres flèches magiques, Cupidon a le pouvoir de rendre 2 personnes amoureuses à jamais. La première nuit, il désigne les 2 joueurs amoureux. Cupidon peut, s’il le veut se désigner comme l’un des deux Amoureux. Si l’un des Amoureux est éliminé, l’autre meurt de chagrin immédiatement. Un Amoureux ne doit jamais éliminer son aimé, ni lui porter aucun préjudice."
    ,"<color=#ff0000ff> La Sorcière aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Elle sait concocter 2 potions extrêmement puissantes : Une potion de Guérison, pour ressusciter le joueur dévoré par les Loups-Garous Une potion d’Empoisonnement, utilisé la nuit pour éliminer un joueur. La Sorcière doit utiliser chaque potion une seul fois dans la partie. Elle ne peut pas se servir de ses deux potions la même nuit."
    ,"<color=#ff0000ff> La Voyante aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Chaque nuit, elle voit la carte d’un joueur de son choix. Elle doit aider les autres Villageois, mais rester discrète pour ne pas être démasquée par les Loups-Garous."
    ,"<color=#ff0000ff> Le Chasseur aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Lorsque le chasseur meurt, il peut choisir un joueur à emporter avec lui dans la mort."
    ,"<color=#ff0000ff> Le Dictateur aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Le dictateur peut choisir un joueur à tuer chaque jour. S'il est découvert par les villageois, ils peuvent voter pour l'éliminer. S'il est découvert par les loups-garous, il est éliminé immédiatement"
    ,"<color=#ff0000ff> Le Garde aide le village à vaincre les Loups-Garous!</size></color>\r\n\n Le garde peut choisir un joueur chaque nuit à protéger. Si les loups-garous essaient de tuer ce joueur, il ne mourra pas."
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
        left_button.onClick.AddListener(left_previous);
        right_button.onClick.AddListener(right_next);
        button_ready.onClick.AddListener(toggleReady);

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
            status.text = nbjoueur_rest + " remaining...";
        }
        else
        {
            status.text = " ready yourselves";
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
        if(roles_presents[index_desc].get_role_count()>1)role_count+=" Players";
        else role_count+=" Player";
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
        if(roles_presents[index_desc].get_role_count()>1)role_count+=" Players";
        else role_count+=" Player";
        nbPlayerP.text=role_count;
        role_name.text=roles_presents[index_desc].get_role();
        change_image();
    }
    public void initialize()
    {

        textName.text = "Waiting screen: " + name;
        no_players = 0;
        players_waiting = new List<WPlayer>();
        max_player = NetworkManager.nbplayeres;
        nbjoueur_rest = max_player;
        AfficheCard();

        //le nombre des roles presents dans la partie


        descripts.text = roles_presents[index_desc].get_description();
        string role_count = " " + roles_presents[index_desc].get_role_count();
        if (roles_presents[index_desc].get_role_count() > 1) role_count += " Players";
        else role_count += " Player";
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
            listCard[i].transform.Find("image-card").GetComponent<Image>().enabled = false;
            listCard[i].transform.Find("card").GetComponent<Image>().enabled = false;
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
}