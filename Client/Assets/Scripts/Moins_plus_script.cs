using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Moins_plus_script : MonoBehaviour
{
    public Button buttonMoins, buttonPlus;
    public TMP_InputField input;
    
    // Start is called before the first frame update
    void Start()
    {
        buttonMoins.onClick.AddListener(OnButtonClickMoins);
        buttonPlus.onClick.AddListener(OnButtonClickPlus);
    }

    void Update() {
        int valeur= int.Parse(input.text);
        if (valeur < 0){
            input.text = "" + 0;
        }
        else if (valeur > 12){
            input.text = "" + 12;
        }
    }

    private void OnButtonClickMoins(){
        int valeur= int.Parse(input.text);
        //on verife si la valeur est egal ou inferieur a 0
        if (valeur > 0)
            input.text = "" + (valeur-1);

    }

    private void OnButtonClickPlus(){
        int valeur= int.Parse(input.text);
        if (valeur < 12)
            input.text = "" + (valeur+1);
    }
}

