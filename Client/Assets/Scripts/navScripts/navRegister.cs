using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class navRegister : MonoBehaviour
{
    public Button register, settings, quit;
    public TMP_InputField username, password, mail, confirmPassword;
    public int selected = -1;

    void Update()
    {
        if((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)) || (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Tab)) || Input.GetKeyDown(KeyCode.UpArrow)) {
            selected--;
            if(selected < 0) selected = 6;
        }

        else if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.DownArrow)) {
            selected++;
            if(selected > 6) selected = 0;
        }

        else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            if(selected != 6) selected = 5;
            else selected = 0;
        }

        else if(Input.GetKeyDown(KeyCode.RightArrow)) {
            if(selected != 5) selected = 6;
            else selected = 4;
        }
        
        SelectField();
    }

    void SelectField() {
        switch(selected) {
            case 0: username.Select();
                break;
            case 1: mail.Select();
                break;
            case 2: password.Select();
                break;
            case 3: confirmPassword.Select();
                break;
            case 4: register.Select();
                break;
            case 5: quit.Select();
                break;
            case 6 : settings.Select();
                break;
        }
    }

    public void selectUsername() => selected = 0;
    public void selectMail() => selected = 1;
    public void selectPassword() => selected = 2;
    public void selectConfirm() => selected = 3;
    public void selectRegister() => selected = 4;
    public void selectSettings() => selected = 6;
    public void selectQuit() => selected = 5;

}
