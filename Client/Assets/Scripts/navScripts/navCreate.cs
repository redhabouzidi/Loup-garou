using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;


public class navCreate : MonoBehaviour
{
    public GameObject moinsVillageois, moinsLG, plusLG;
    public TMP_InputField inputVillageName, nbLoup, nbVillageois;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == inputVillageName.gameObject) {
            if(Input.GetKeyDown(KeyCode.DownArrow)) {
                EventSystem.current.SetSelectedGameObject(moinsLG);
            }

        }

        if (EventSystem.current.currentSelectedGameObject == nbLoup.gameObject) {
            if(Input.GetKeyDown(KeyCode.DownArrow)) {
                EventSystem.current.SetSelectedGameObject(moinsLG);
            }

            else if(Input.GetKeyDown(KeyCode.DownArrow)) {
                EventSystem.current.SetSelectedGameObject(plusLG);
            }

        }

        if (EventSystem.current.currentSelectedGameObject == nbVillageois.gameObject) {
            if(Input.GetKeyDown(KeyCode.DownArrow)) {
                EventSystem.current.SetSelectedGameObject(plusLG);
            }
        }

    }

}

