using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class navHome : MonoBehaviour
{
    public GameObject ranking, rules, create, settings, profile;
    public int selected = -1;

    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == ranking) {
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                EventSystem.current.SetSelectedGameObject(rules);
            }

            else if(Input.GetKeyDown(KeyCode.RightArrow)) {
                EventSystem.current.SetSelectedGameObject(settings);
            }
        }

        else if (EventSystem.current.currentSelectedGameObject == rules) {
            if(Input.GetKeyDown(KeyCode.LeftArrow)) {
                EventSystem.current.SetSelectedGameObject(profile);
            }

            else if(Input.GetKeyDown(KeyCode.RightArrow)) {
                EventSystem.current.SetSelectedGameObject(ranking);
            }
        }

        else if (EventSystem.current.currentSelectedGameObject == profile) {
            if(Input.GetKeyDown(KeyCode.RightArrow)) {
                EventSystem.current.SetSelectedGameObject(rules);
            }
        }
        
    }

}
