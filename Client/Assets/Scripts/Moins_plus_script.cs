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

    private void OnButtonClickMoins(){
        input.text = "" + (int.Parse(input.text)-1);
    }

    private void OnButtonClickPlus(){
        input.text = "" + (int.Parse(input.text)+1);
    }
}

