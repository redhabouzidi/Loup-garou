using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonFriendAddWait : MonoBehaviour
{
    public GameObject componentAddWait;
    public Button buttonAdd, buttonCancel;
    public TextMeshProUGUI friendPseudo;
    private GameObject GO_buttonAdd, GO_buttonCancel;


    // Start is called before the first frame update
    void Start()
    {
        buttonAdd.onClick.AddListener(onButtonClickAdd);  
        buttonCancel.onClick.AddListener(onButtonClickCancel);   

        GO_buttonAdd = componentAddWait.transform.Find("Button-add").gameObject;
        GO_buttonCancel = componentAddWait.transform.Find("Button-cancel").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
        change l'affichage du component apres avoir appuyé sur le bouton ajouter en ami
    **/
    private void onButtonClickAdd(){
        // modifier le status dans la bdd

        GO_buttonAdd.SetActive(false);
        GO_buttonCancel.SetActive(true); 
    }

    /**
        change l'affichage du component apres avoir appuyé 
        sur le bouton annuler la demande d'ami
    **/
    private void onButtonClickCancel(){
        // modifier le status dans la bdd

        GO_buttonAdd.SetActive(false);
        GO_buttonCancel.SetActive(true);
    }
}
