using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour
{
    public static bool isHide = false;
    public GameObject chat, chatNotification;
    public Image imageSeeHide;
    public Button buttonSeeHide;
    
    void Start()
    {
        buttonSeeHide = GameObject.Find("chatSeeHide").GetComponent<Button>();
        buttonSeeHide.onClick.AddListener(OnButtonClickSeeHideChat);
    }

    /**
        masquer/afficher le chat du jeu
    **/
    public void OnButtonClickSeeHideChat()
    {
        if (!isHide){
            imageSeeHide.rectTransform.anchoredPosition = new Vector2(255, 250);
            imageSeeHide.transform.localScale = new Vector3(-1,1,1);
        }
        else {
            imageSeeHide.rectTransform.anchoredPosition = new Vector2(-206, 250);
            chatNotification.SetActive(false);
            imageSeeHide.transform.localScale = new Vector3(1,1,1);
        }
        chat.gameObject.SetActive(!chat.activeSelf);
        isHide = !isHide;
    }
}
