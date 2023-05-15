using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class navChangePassword : MonoBehaviour
{
    public Button settings, cross, change;
    public TMP_InputField old_, new_, new2;
    public int selected = 0;

    void Update()
    {
        if((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)) || (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Tab)) || Input.GetKeyDown(KeyCode.UpArrow)) {
            selected--;
            if(selected < 0) selected = 5;
        }

        else if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.DownArrow)) {
            selected++;
            if(selected > 5) selected = 0;
        }
        
        SelectField();
    }

    void SelectField() {
        switch(selected) {
            case 0: old_.Select();
                break;
            case 1: new_.Select();
                break;
            case 2: new2.Select();
                break;
            case 3: change.Select();
                break;
            case 4: cross.Select();
                break;
            case 5: settings.Select();
                break;
        }
    }

    public void selectOld() => selected = 0;
    public void selectNew() => selected = 1;
    public void selectNew2() => selected = 2;
    public void selectChange() => selected = 3;
    public void selectCross() => selected = 4;
}

