using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class GameManager : MonoBehaviour
{
    // joueur
    private Player p;

    // jeu
    private int nbPlayer = 2;
    public List<Player> listPlayer = new List<Player>();
    public List<GameObject> listCard = new List<GameObject>();
    public GameObject cardContainer, cardComponent;
    [SerializeField]
    private bool isNight = false;
    [SerializeField]
    private int tour = 0;

    public Color colorRed, colorWhite;
    public TextMeshProUGUI text_day;
    public TextMeshProUGUI player_role;

    // voter
    public Button buttonValiderVote;

    // win screen
    public bool isVillageWin = true;
    public GameObject winPanelLeft, winPanelRight, textWinPlayer;
    List<TextMeshProUGUI> listTextwin = new List<TextMeshProUGUI>();
    public TextMeshProUGUI groupWin;


    // variable pour le chat
    private int maxMsg = 30;
    List<Message> msgList = new List<Message>();
    public GameObject chatPanel, textComponent;
    public TMP_InputField inputChat;
    public Button sendChat;
    public Color playerC, systemC;

    // options page
    public Button buttonLeaveGame;

    // Start is called before the first frame update
    void Start()
    {
        sendChat.onClick.AddListener(OnButtonClickSendMsg);
        buttonLeaveGame.onClick.AddListener(OnButtonClickLeave);
        buttonValiderVote.onClick.AddListener(OnButtonClickVote);

        NetworkManager.gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        foreach (WPlayer p in NetworkManager.players)
        {
            switch (p.GetRole())
            {
                case 1:
                    listPlayer.Add(new Player(p.GetUsername(), "Villageois", 1, true));
                    break;
                case 2:
                    listPlayer.Add(new Player(p.GetUsername(), "Cupidon", 2, true));
                    break;
                case 3:
                    listPlayer.Add(new Player(p.GetUsername(), "Voyante", 3, true));
                    break;
                case 4:
                    listPlayer.Add(new Player(p.GetUsername(), "Loup-Garou", 4, true));
                    break;
                case 5:
                    listPlayer.Add(new Player(p.GetUsername(), "Sorciere", 5, true));
                    break;
            }

        }

        AfficheCard();
        AfficheWinScreen();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadScene("jeu");
        }
        if (inputChat.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageToChat(p.GetPseudo() + ": " + inputChat.text, Message.MsgType.player);
                inputChat.text = "";
                inputChat.ActivateInputField();
            }
        }
        AfficherJour();
        //AffichageMort();
    }

    /*public void AffichageMort() {

    }*/


    public void AfficherJour()
    {
        if (isNight == false)
        {
            text_day.text = "Day " + tour;
            text_day.color = colorWhite;
            player_role.color = colorWhite;
        }
        else
        {
            text_day.text = "Night " + tour;
            text_day.color = colorRed;
            player_role.color = colorRed;
        }
    }

    public void AjoutTextWin(GameObject panel, int num)
    {
        TextMeshProUGUI newText;
        GameObject newPlayer = Instantiate(textWinPlayer, panel.transform);
        newText = newPlayer.GetComponent<TextMeshProUGUI>();
        newText.text = listPlayer[num].GetPseudo() + ": " + listPlayer[num].GetRole();
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

        int playerGauche = nbPlayer / 2;
        for (int i = 0; i < playerGauche; i++)
        {
            AjoutTextWin(winPanelLeft, i);
        }
        for (int i = playerGauche; i < nbPlayer; i++)
        {
            AjoutTextWin(winPanelRight, i);
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

    private void OnButtonClickSendMsg()
    {
        if (inputChat.text != "")
        {
            SendMessageToChat(p.GetPseudo() + ": " + inputChat.text, Message.MsgType.player);
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
        toggleCard.group = cardContainer.GetComponent<ToggleGroup>();
        TextMeshProUGUI text = newCard.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        text.text = listPlayer[id].GetPseudo();
        if (!listPlayer[id].GetIsAlive())
        {
            text.text += " - mort\n" + listPlayer[id].GetRole();
            text.color = colorRed;
            toggleCard.interactable = false;
        }
        listCard.Add(newCard);
    }

    public void AfficheCard()
    {
        for (int i = 0; i < nbPlayer; i++)
        {
            AjoutCarte(i);
        }

    }

    public void Vote()
    {
        int selectedId = -1;
        if (cardContainer.GetComponent<ToggleGroup>().AnyTogglesOn())
        {
            for (int i = 0; i < nbPlayer; i++)
            {
                if (listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().isOn)
                {
                    selectedId = i;
                }
            }
            //if(listPlayer[selectedId].id == p.id && p.id.getRole() != "Cupidon") SendMessageToChat("Tu ne peux pas voter pour toi meme", Message.MsgType.system);
            SendMessageToChat("Tu as voté pour " + listPlayer[selectedId].GetPseudo(), Message.MsgType.system);
            NetworkManager.Vote(NetworkManager.client, NetworkManager.id, listPlayer[selectedId].GetId());
        }
        else
        {
            SendMessageToChat("Tu as voté pour personne, pitié vote >:(", Message.MsgType.system);
        }
        // id du joueur sur le client -> p.GetId();
        // id du joueur voté listPlayer[selectedId].GetId();
        // Envoyer l'id du joueur vote et du votant au reseau        
    }

    public void OnOff()
    {
        cardContainer.GetComponent<ToggleGroup>().SetAllTogglesOff();
        for (int i = 0; i < nbPlayer; i++)
        {
            listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().interactable = !listCard[i].transform.Find("Toggle-Card").GetComponent<Toggle>().interactable;
        }
        sendChat.interactable = !sendChat.interactable;
        inputChat.interactable = !inputChat.interactable;
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
    public Player() { }

    public Player(string p, string r, int rid, bool alive = true)
    {
        pseudo = p;
        role = r;
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
    public void SetIsAlive(bool alive)
    {
        isAlive = alive;
    }
    public void SetRole(int rid)
    {
        roleId = rid;
        switch(rid)
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
