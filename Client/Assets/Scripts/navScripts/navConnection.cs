using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class navConnection : MonoBehaviour
{
    public Button connection, settings, quit, seePassword, hidePassword, forgotPassword;
    public TMP_InputField username, password;
    public int selected = 0;

    void Update()
    {
        if((Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab)) || (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Tab)) || Input.GetKeyDown(KeyCode.UpArrow)) {
            selected--;
            if(selected < 0) selected = 6;
        }

        else if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.DownArrow) || (Input.GetKey(KeyCode.Return))) {
            selected++;
            if(selected > 6) selected = 0;
        }

        else if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            if(selected != 6) selected = 5;
            else selected = 0;
        }

        else if(Input.GetKeyDown(KeyCode.RightArrow)) {
            if(selected != 5) selected = 6;
            else selected = 3;
        }
        
        SelectField();
    }

    void SelectField() {
        switch(selected) {
            case 0: username.Select();
                break;
            case 1: password.Select();
                break;
            case 3: connection.Select();
                break;
            case 2: 
                if(seePassword.gameObject.activeSelf) seePassword.Select();
                else hidePassword.Select();
                break;
            case 4: forgotPassword.Select();
                break;
            case 5: quit.Select();
                break;
            case 6: settings.Select();
                break;
        }
    }

    public void selectUsername() => selected = 0;
    public void selectPassword() => selected = 1;
    public void selectConnection() => selected = 3;
    public void selectSeePassword() => selected = 2;
    public void selectHidePassword() => selected = 2;
    public void selectSettings() => selected = 6;
    public void selectQuit() => selected = 5;
    public void selectForgotPassword() => selected = 4;

}

