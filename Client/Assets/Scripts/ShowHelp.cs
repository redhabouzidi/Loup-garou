using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShowHelp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject info;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
        affiche l'info bulle quand le pointeur de la souris entre dans l'element
    **/
    public void OnPointerEnter(PointerEventData eventData)
    {
        info.SetActive(true);
    }

    /**
        masque l'info bulle quand le pointeur de la souris sort de l'element
    **/
    public void OnPointerExit(PointerEventData eventData)
    {
       info.SetActive(false);
    }
}

