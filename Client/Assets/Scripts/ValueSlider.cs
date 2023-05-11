using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ValueSlider : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI textValue;
    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener(delegate {ChangeValueSlider(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
        affiche la valeur du slider dans le jeu à coté d'un slider
    **/
    public void ChangeValueSlider(){
        textValue.text = slider.value.ToString();
    }
}
