using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
public class WaitingScreen : MonoBehaviour
{
    public bool isStart=false;
    private int nbjoueur_rest=2;
    public TextMeshProUGUI status;
    public int max_player;
    public int index_desc=0;
    public Image image_carte;
    public Button right_button;
    public Button left_button;
    public TextMeshProUGUI role_name;
    public TextMeshProUGUI descripts;
    private string []roles={"Loup","Villageois","Cupidon","Sorciere","Voyante"};
    private string []description={"Desc Loup","Desc Villageois","Desc Cupidon","Desc Sorciere","Desc Voyante"};
    private List<TextMeshProUGUI> textObjects;
    public List<WPlayer> players_waiting;
    private int no_players=0;
    // Start is called before the first frame update
    void Start()
    {
        textObjects = new List<TextMeshProUGUI>();
        players_waiting = new List<WPlayer>();
        Debug.Log("waitingscren");
        left_button.onClick.AddListener(left_previous);
        right_button.onClick.AddListener(right_next);
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        change_image();//Charger le premier image
                        //Liste de TextMeshProUGUI pour stocker les cartes
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("waitinguname");
        foreach (GameObject obj in allObjects)//pour chaque objet trouve avec ce tag:
        {
            //recuperer le composant TextMeshpro
            TextMeshProUGUI textObj = obj.GetComponent<TextMeshProUGUI>();

            if (textObj != null && textObj.name =="uname")
            {
                textObjects.Add(textObj);
            }
        }
        Debug.Log(textObjects[0].text);
        isStart = true;
        //quitplayer("Tt");
    }

    //Fonction permettant d'obtenir le role suivant en cliquant le button droite
    void right_next(){
        if(index_desc==roles.Length-1)index_desc=0;
        else index_desc++;
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        change_image();
    }
    //Fonction permettant d'obtenir le role precedent en cliquant le button gauche
    void left_previous(){
        if(index_desc==0)index_desc=roles.Length-1;
        else index_desc--;
        descripts.text=description[index_desc];
        role_name.text=roles[index_desc];
        change_image();
    }
    // Update is called once per frame
    void Update()
    {
        if(nbjoueur_rest!=0)status.text=nbjoueur_rest+" remaining...";//chaque frame, verifier nombre de joeurs reste pour commencer la partie
        else status.text="Game ready to start.";
    }

    //Fonction permettant de ajouter un joueur avec son nom d'utilisateur
    public void addplayer(string username,int id){
        if(players_waiting.Count==max_player)return; //si lobby plein, rien fait
        players_waiting.Add(new WPlayer(username,id));
        no_players++;
        if(nbjoueur_rest!=0)nbjoueur_rest--;
        foreach(TextMeshProUGUI el in textObjects){//pour chaque textmeshpro dans cartes, trouver un champs vide et ecrire le nom d'utilisateur
            Debug.Log(el.text=="");
            if(el.text==""){
                el.text=username;
                Image parent = el.transform.parent.GetComponent<Image>();
                if(parent!=null)parent.color=Color.red;
                return;
            }
        }
            
    }
    //Fonction permettant de supprimer un joueur avec son nom d'utilisateur
    void quitplayer(string username){
        if(players_waiting.Count==0)return;
        players_waiting.RemoveAll(player => player.GetUsername()==username);//trouver un utilisateur avec le nom d'utilisateur = 'username' et le supprimer
        no_players--;
        nbjoueur_rest++;
        Debug.Log(no_players.ToString());
        foreach(TextMeshProUGUI el in textObjects){//pour chaque textmeshpro dans cartes, trouver un champs avec un nom d'utilisateur = 'username' et remplacer par ""
            if(el.text==username){
                el.text="";
                Image parent = el.transform.parent.GetComponent<Image>();
                if(parent!=null)parent.color=Color.white;
            }
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
}

//Class pour des joueurs
public class WPlayer{
    private string username;
    private int id;
    private int role;
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

}

