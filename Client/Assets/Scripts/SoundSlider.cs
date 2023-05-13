using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SoundSlider : MonoBehaviour
{
    public Slider sliderMenu;
    public Slider sliderButton;
    public Toggle muteToggle;
    public AudioSource audioSourceMenu;
    public AudioSource audioSourceButton;
    private bool isMuted = false;
    public static float musicVolume=25,effectVolume=25;
    public static bool mute;
    public TextMeshProUGUI textValue1,textValue2;
    private void Start()
    {
        Debug.Log(effectVolume+"   "+musicVolume);
        muteToggle.isOn=mute;
        sliderMenu.value=musicVolume;
        sliderButton.value=effectVolume;
        ValueChangeCheckMenu();
        ValueChangeCheckButton();
        MuteToggleCheck();
        sliderMenu.onValueChanged.AddListener(delegate { ValueChangeCheckMenu(); });
        sliderButton.onValueChanged.AddListener(delegate { ValueChangeCheckButton(); });
        muteToggle.onValueChanged.AddListener(delegate { MuteToggleCheck(); });

    }

    public void ValueChangeCheckMenu()
    {
        audioSourceMenu.volume = sliderMenu.value/100;
        musicVolume=sliderMenu.value;
        SoundSlider_GameScene.musicVolume=sliderMenu.value;
        ChangeValueSlider(textValue1,sliderMenu);
    }
    public void ValueChangeCheckButton(){
        audioSourceButton.volume=sliderButton.value/100;
        effectVolume=sliderButton.value;
        SoundSlider_GameScene.effectVolume=sliderButton.value;
        ChangeValueSlider(textValue2,sliderButton);
    }
    public void MuteToggleCheck()
    {
        audioSourceMenu.mute = muteToggle.isOn;
        audioSourceButton.mute=muteToggle.isOn;
        mute=muteToggle.isOn;
        SoundSlider_GameScene.mute=muteToggle.isOn;
    }
    public void ChangeValueSlider(TextMeshProUGUI textValue,Slider slider){
        textValue.text = slider.value.ToString();
    }
}

