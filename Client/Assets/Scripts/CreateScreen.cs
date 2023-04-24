using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateScreen : MonoBehaviour
{
    public TMP_InputField inputPlayers, inputLG, inputName;
    public TextMeshProUGUI textVillager;
    public Toggle toggleSeer, toggleWitch, toggleCupidon, toggleHunter, toggleGuardian, toggleDictator;
    private List<Toggle> listToggle = new List<Toggle>();
    public Button buttonCreate;


    // Start is called before the first frame update
    void Start()
    {
        buttonCreate.onClick.AddListener(OnButtonClickCreate);
        AddToggleInList();
        foreach(Toggle t in listToggle){
            t.onValueChanged.AddListener(delegate {OnToggleValueChanged(t);});
        }

    }

    // Update is called once per frame
    void Update()
    {
        // mise a jour des roles
        int nbLG = int.Parse(inputLG.text);
        int nbPlayer = int.Parse(inputPlayers.text);

        // impossible d'avoir plus de la moitie des joueurs loups
        if(nbLG > 1 && nbLG > nbPlayer/2){
            nbLG = (int) nbPlayer/2;
            inputLG.text = "" + nbLG;
        }
        // mise a jour du nombre de loup si le nombre de roles restant == 0
        else if(nbLG > 1 && nbLG > (nbPlayer-CountToggleOn())){
            nbLG = nbPlayer-CountToggleOn();
            inputLG.text = "" + nbLG;
        }
        // mise jour des toggle on s'il y a trop de roles par rapport au nb de joueur
        for(int i = listToggle.Count-1; i >= 0 && nbPlayer < (nbLG + CountToggleOn()); i--){
            listToggle[i].isOn = false;
        }

        int nbVillager = GetNbPlayerRest();
        textVillager.text = "" + nbVillager;

    }

    public int CountToggleOn(){
        int count = 0;
        foreach (Toggle t in listToggle){
            if(t.isOn){
                count ++;
            }
        }
        return count;
    }

    public int GetNbPlayerRest(){
        return int.Parse(inputPlayers.text) - (int.Parse(inputLG.text) + CountToggleOn());
    }

    private void AddToggleInList(){
        listToggle.Add(toggleWitch);
        listToggle.Add(toggleSeer);
        listToggle.Add(toggleCupidon);
        listToggle.Add(toggleHunter);
        listToggle.Add(toggleDictator);
        listToggle.Add(toggleGuardian);
    }

    private void OnButtonClickCreate(){
        string name;
        int nbLG, nbPlayer;
        bool seer, witch, cupidon, hunter, guardian, dictator;

        name = inputName.text;
        nbPlayer = int.Parse(inputPlayers.text);
        nbLG = int.Parse(inputLG.text);

        seer = toggleSeer.isOn;
        witch = toggleWitch.isOn;
        cupidon = toggleCupidon.isOn;
        hunter = toggleHunter.isOn;
        guardian = toggleGuardian.isOn;
        dictator = toggleDictator.isOn;

        // envoyer au serveur les données
        NetworkManager.createGame(NetworkManager.id, NetworkManager.username, name, nbPlayer, nbLG, witch, seer, cupidon,hunter,guardian,dictator);
        Debug.Log("envoyé");
    }

    public void OnToggleValueChanged(Toggle change){
        int nbPlayerRest = GetNbPlayerRest();
        if(change.isOn){
            if (nbPlayerRest < 0){
                change.isOn = false;
            }
        }
    }
}
