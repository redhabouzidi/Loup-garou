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
    public TextMeshProUGUI status;
    //public bool isStart = false;

    public int nbjoueur_rest;
    public bool newGame;
    public int max_player;
    private int index_desc = 0;

    // description des roles
    public Image image_carte;
    public Button right_button, left_button,button_ready;
    public TextMeshProUGUI role_name, descripts;
    private List<InfoRole> infoRole = new List<InfoRole>();

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
        // crée la liste des roles pour l'affichage des infos
        for(int i=1; i<=8; i++)
        {
            string desc;
            InfoRole newRole = new InfoRole();
            switch (i)
            {
            case 1:
                desc = "Les villageois sont les gentils du jeu. Ils doivent découvrir qui sont les loups-garous et les éliminer" +
                    " avant qu'ils ne les tuent tous. Ils n'ont pas de pouvoirs spéciaux.";
                newRole = new InfoRole(i, "Villageois", desc, "villageois");
                break;
            case 2:
                desc = "Cupidon choisit deux joueurs à lier l'un à l'autre. Si l'un des deux joueurs meurt," +
                    " l'autre joueur meurt immédiatement de chagrin.";
                newRole = new InfoRole(i, "Cupidon", desc, "cupidon");
                break;
            case 3:
                desc = "La voyante peut voir le rôle d'un joueur chaque nuit. Cela peut aider les villageois" +
                    " à découvrir qui sont les loups-garous.";
                newRole = new InfoRole(i, "Voyante", desc, "voyante");
                break;
            case 4:
                desc = "Les loups-garous sont les méchants du jeu. Ils se réveillent la nuit pour décider qui ils vont tuer." +
                    " Pendant le jour, ils essaient de se faire passer pour des villageois pour ne pas être découverts.";
                newRole = new InfoRole(i, "Loup-garou", desc, "loup");

                break;
            case 5:
                desc = "La sorcière a une potion de guérison qui peut sauver un joueur de la mort." +
                    " Elle a également une potion de poison qui peut tuer un joueur.";
                newRole = new InfoRole(i, "Sorciere", desc, "sorciere");
                break;
            case 6:
                desc = "Lorsque le chasseur meurt, il peut choisir un joueur à emporter avec lui dans la mort.";
                newRole = new InfoRole(i, "Chasseur", desc, "chasseur");
                break;
            case 7:
                desc = "Le dictateur peut choisir un joueur à tuer chaque jour. S'il est découvert par les villageois," +
                    "ils peuvent voter pour l'éliminer. S'il est découvert par les loups-garous, il est éliminé immédiatement";
                newRole = new InfoRole(i, "Dictateur", desc, "dictateur");
                break;
            case 8:
                desc = "Le garde peut choisir un joueur chaque nuit à protéger." + 
                    " Si les loups-garous essaient de tuer ce joueur, il ne mourra pas.";
                newRole = new InfoRole(i, "Garde", desc, "garde");
                break;

            }
            infoRole.Add(newRole);
        }

        descripts.text = infoRole[index_desc].GetDescription();
        role_name.text = infoRole[index_desc].GetRole();
        change_image();//Charger le premier image
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
    }
    public void toggleReady()
    {
        NetworkManager.sendReady();
    }
    //Fonction permettant d'obtenir le role suivant en cliquant le button droite
    void right_next()
    {
        index_desc = (index_desc+1)%infoRole.Count;
        descripts.text = infoRole[index_desc].GetDescription();
        role_name.text = infoRole[index_desc].GetRole();
        change_image();
    }
    //Fonction permettant d'obtenir le role precedent en cliquant le button gauche
    void left_previous()
    {
        index_desc = (index_desc-1)%infoRole.Count;
        descripts.text = infoRole[index_desc].GetDescription();
        role_name.text = infoRole[index_desc].GetRole();
        change_image();
    }
    public void initialize()
    {
        no_players = 0;
        players_waiting = new List<WPlayer>();
        max_player = NetworkManager.nbplayeres;
        nbjoueur_rest = max_player;
        AfficheCard();

        //isStart = true;
        foreach (WPlayer p in NetworkManager.players)
        {
            addplayer(p.GetUsername(), p.GetId());
        }
    }
    //Fonction permettant de ajouter un joueur avec son nom d'utilisateur
    public void addplayer(string username, int id)
    {
        if(players_waiting.Count >= max_player){
            return;
        }
        players_waiting.Add(new WPlayer(username, id));
        no_players++;

        if (nbjoueur_rest != 0) nbjoueur_rest--;

        Debug.Log(no_players.ToString()+"Fonction addplayer " +username);
        AffichageUsernameText(); 
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
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorCard;
            listCard[i].transform.Find("text-pseudo").GetComponent<TextMeshProUGUI>().text = players_waiting[i].GetUsername();
        }
        for (int i=no_players; i<max_player; i++){
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorNone;
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

    void change_image()
    {
        Texture2D texture = LoadPNG(Application.dataPath + "/Cartes/" + infoRole[index_desc].GetChemin() + ".png");//Trouver l'image des roles

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//creer un sprite pour visualiser

        image_carte.sprite = sprite;//attribuer
    }

    //Fonction retournant une image png qui se situe dans le chemin 'path' passÃ© en parametre
    Texture2D LoadPNG(string path)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(path))
        {
            fileData = File.ReadAllBytes(path);
            tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
        }
        else
        {
            Debug.LogError("No file found: " + path);
        }

        return tex;
    }
}

//Class pour des joueurs
public class WPlayer
{
    private string username;
    private int id;
    private int role;
    //....

    public WPlayer(string uname, int id)
    {
        this.username = uname;
        this.id = id;
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

public class InfoRole
{
    private int roleId;
    private string role;
    private string description;
    private string cheminImage;

    public InfoRole(){}

    public InfoRole(int roleId, string role, string desc, string chemin){
        this.roleId = roleId;
        this.role = role;
        description = desc;
        cheminImage = chemin;
    }

    public int GetRoleId(){
        return roleId;
    }
    public string GetRole(){
        return role;
    }
    public string GetDescription(){
        return description;
    }
    public string GetChemin(){
        return cheminImage;
    }
}