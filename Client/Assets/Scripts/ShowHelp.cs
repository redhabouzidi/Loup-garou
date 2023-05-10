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

    public void OnPointerEnter(PointerEventData eventData)
    {
        info.SetActive(true);
    }

    // Méthode appelée lorsque la souris sort de la zone du Panel
    public void OnPointerExit(PointerEventData eventData)
    {
       info.SetActive(false);
    }
}

