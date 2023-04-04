using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;// Required when using Event data.

public class ButtonHome : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
    public Button button1, button2;
    private ColorBlock normalColors;
    private ColorBlock highlightedColors;
    public GameObject ligne;

    void Start() {
        normalColors = button1.colors;
        highlightedColors = normalColors;
        highlightedColors.normalColor = normalColors.highlightedColor;
        ligne.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        button2.colors = highlightedColors;
        button1.colors = highlightedColors;
        ligne.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        button2.colors = normalColors;
        button1.colors = normalColors;
        ligne.SetActive(false);
    }
    public void OnSelect(BaseEventData eventData){
        button2.colors = normalColors;
        button1.colors = normalColors;
        ligne.SetActive(false);
    }
}
