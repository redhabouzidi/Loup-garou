using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonSeeHidePassword : MonoBehaviour
{
    public GameObject see, hide;
    public TMP_InputField text_password;
    private TMP_InputField.ContentType contentTypePassword, contentTypeStandard;
    
    // Start is called before the first frame update
    void Start()
    {
        Button button_see = see.GetComponent<Button>();
        Button button_hide = hide.GetComponent<Button>();
        contentTypePassword = TMP_InputField.ContentType.Password;
        contentTypeStandard = TMP_InputField.ContentType.Standard;

        button_see.onClick.AddListener(OnButtonClickSee);
        button_hide.onClick.AddListener(OnButtonClickHide);
    }

    private void OnButtonClickSee()
    {
        see.SetActive(false);
        hide.SetActive(true);
        text_password.contentType = contentTypePassword;
        text_password.ForceLabelUpdate();
        text_password.DeactivateInputField();
    }

    private void OnButtonClickHide()
    {
        hide.SetActive(false);
        see.SetActive(true);
        text_password.contentType = contentTypeStandard;
        text_password.ForceLabelUpdate();
        text_password.DeactivateInputField();
    }
}
