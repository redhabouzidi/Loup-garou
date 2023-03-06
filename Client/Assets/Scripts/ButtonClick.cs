using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    [SerializeField]
    private bool hide = false;
    private GameObject chat;
    private Image seeHide;
    
    void Start()
    {
        chat = GameObject.Find("Chat");
        seeHide = GameObject.Find("Button-see-hide-chat").GetComponent<Image>();
    }

    public void ButtonSeeHideChat()
    {
        // Code pour l'action à effectuer lorsque le bouton est cliqué
        if (!hide){
            seeHide.rectTransform.anchoredPosition = new Vector2(-30, 315);
        }
        else {
            seeHide.rectTransform.anchoredPosition = new Vector2(-557, 315);
        }
        chat.gameObject.SetActive(!chat.gameObject.activeSelf);
        hide = !hide;
    }
}
