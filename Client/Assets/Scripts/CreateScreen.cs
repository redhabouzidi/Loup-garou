using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateScreen : MonoBehaviour
{
    public int nbPlayers;
    
    public TMP_InputField inputPlayers, inputLG, inputVillage, inputName;
    public Toggle toggleSeer, toggleWitch, toggleCupidon, toggleHunter, toggleGuardian, toggleDictator;
    public Button buttonCreate;


    // Start is called before the first frame update
    void Start()
    {
        buttonCreate.onClick.AddListener(OnButtonClickCreate);
        togglePrivate.onValueChanged.AddListener(OnToggleChangedPrivate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnButtonClickCreate(){
        string name;
        int nbLG, nbVillage, nbPlayer;
        bool seer, witch, cupidon, hunter, guardian, dictator;

        name = inputName.text;
        nbPlayer = int.Parse(inputPlayers.text);
        nbLG = int.Parse(inputLG.text);
        nbVillage = int.Parse(inputVillage.text);

        seer = toggleSeer.isOn;
        witch = toggleWitch.isOn;
        cupidon = toggleCupidon.isOn;
        hunter = toggleHunter.isOn;
        guardian = toggleGuardian.isOn;
        dictator = toggleDictator.isOn;

        // envoyer au serveur les données
        NetworkManager.createGame(NetworkManager.client, NetworkManager.id, NetworkManager.username, name, nbPlayer, nbLG, seer, witch, cupidon);
    }
}
