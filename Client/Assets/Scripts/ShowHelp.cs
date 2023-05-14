using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShowHelp : MonoBehaviour, IPointerDownHandler // IPointerEnterHandler, IPointerExitHandler, 
{
    public GameObject info;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 2.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if(info.activeSelf){
            timer -= Time.deltaTime;
            if(timer <= 0){
                info.SetActive(false);
            }
        }
    }
    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        info.SetActive(true);
    }

    // Méthode appelée lorsque la souris sort de la zone du Panel
    public void OnPointerExit(PointerEventData eventData)
    {
       info.SetActive(false);
    }
    */
    public void OnPointerDown(PointerEventData eventData)
    {
        info.SetActive(true);
        timer = 2.5f;
    }
}

