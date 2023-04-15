using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Linq;
public class WaitingScreen : MonoBehaviour
{
    public TextMeshProUGUI status;
    public bool isStart=false;

    [SerializeField]
    private int nbjoueur_rest=2;
    public int max_player;
    public int index_desc=0;

    public Image image_carte;
    public Button right_button;
    public Button left_button;
    public TextMeshProUGUI role_name;
    public TextMeshProUGUI descripts;
    private string []roles={"Loup","Villageois","Cupidon","Sorciere","Voyante"};
    private string []description={"Desc Loup","Desc Villageois","Desc Cupidon","Desc Sorciere","Desc Voyante"};
    
    public List<WPlayer> players_waiting;
    private int no_players=0;
    public GameObject cardContainer, cardComponent;
    private List<GameObject> listCard = new List<GameObject>();
    public Color colorNone, colorCard;

    // Start is called before the first frame update
    void Start()
    {
        AfficheCard();

        left_button.onClick.AddListener(left_previous);
        right_button.onClick.AddListener(right_next);
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        
        change_image();//Charger le premier image
            
        //addplayer("pseudo", 2);
        //quitplayer("Tt");
    }

    // Update is called once per frame
    void Update()
    {
        //chaque frame, verifier nombre de joueurs restant pour commencer la partie
        if(nbjoueur_rest!=0)
        {
            status.text=nbjoueur_rest+" remaining...";
        }
        else {
            status.text="Game ready to start.";
            SceneManager.LoadScene("game_scene");
        }
    }

    //Fonction permettant d'obtenir le role suivant en cliquant le button droite
    void right_next(){
        if(index_desc==roles.Length-1)index_desc=0;
        else index_desc++;
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        change_image();

        addplayer("paper", 3);
    }

    //Fonction permettant d'obtenir le role precedent en cliquant le button gauche
    void left_previous(){
        if(index_desc==0)index_desc=roles.Length-1;
        else index_desc--;
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        change_image();
    }

    //Fonction permettant de ajouter un joueur avec son nom d'utilisateur
    public void addplayer(string username,int id){
        //si lobby plein, rien fait
        if(players_waiting.Count==max_player)
            return; 

        players_waiting.Add(new WPlayer(username,id));
        no_players++;

        if(nbjoueur_rest!=0) nbjoueur_rest--;

        Debug.Log(no_players.ToString()+"Fonction addplayer " +username);
        AffichageUsernameText(); 
    }

    //Fonction permettant de supprimer un joueur avec son nom d'utilisateur
    void quitplayer(string username){
        if(players_waiting.Count==0)
            return;
        //trouver un utilisateur avec le nom d'utilisateur = 'username' et le supprimer
        players_waiting.RemoveAll(player => player.GetUsername()==username);

        no_players--;
        nbjoueur_rest++;

        Debug.Log(no_players.ToString());
        
        AffichageUsernameText();
    }

    public void AffichageUsernameText(){
        for (int i=0; i<no_players; i++){
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorCard;
            listCard[i].transform.Find("text-pseudo").GetComponent<TextMeshProUGUI>().text = players_waiting[i].GetUsername();
            /*
            if(players_waiting[i].isReady) listCard[i].transform.Find("image-check").GetComponent<Image>().enabled = true;
            else listCard[i].transform.Find("image-check").GetComponent<Image>().enabled = false;
            */
        }
        for (int i=no_players; i<max_player; i++){
            listCard[i].transform.Find("image-card").GetComponent<Image>().color = colorNone;
            listCard[i].transform.Find("text-pseudo").GetComponent<TextMeshProUGUI>().text = "";
        }

    }
    
    void change_image(){
        Texture2D texture = LoadPNG(Application.dataPath+"/Cartes/"+roles[index_desc]+".png");//Trouver l'image des roles

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));//creer un sprite pour visualiser

        image_carte.sprite=sprite;//attribuer
}
    //Fonction retournant une image png qui se situe dans le chemin 'path' passé en parametre
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

    public void AjoutCarte(int id){
        GameObject newCard = Instantiate(cardComponent, cardContainer.transform);
        listCard.Add(newCard);
    }

    public void AfficheCard(){
        for (int i=0; i<max_player; i++){
            AjoutCarte(i);
        }
    }
}

//Class pour des joueurs
public class WPlayer{
    private string username;
    private int id;
    private int role;
    private bool isReady = false;
    //....

    public WPlayer(string uname,int id){
        this.username=uname;
        this.id=id;
    }
    public string GetUsername(){
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
    public bool GetReady() {
        return isReady;
    }
    public void SetReady(bool r) {
        isReady = r;
    }
}

