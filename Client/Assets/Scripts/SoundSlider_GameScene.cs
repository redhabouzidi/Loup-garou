using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SoundSlider_GameScene : MonoBehaviour
{
    public Slider sliderMenu;
    public Slider sliderButton;
    public Toggle muteToggle;
    public AudioSource audioSourceMenu1;
    public AudioSource audioSourceMenu2;
    public AudioSource audioSourceButton;
    private bool isMuted = false;
    public static float musicVolume=25,effectVolume=25;
    public static bool mute=true;
    public TextMeshProUGUI textValue1,textValue2;
    private void Start()
    {
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
        audioSourceMenu1.volume = sliderMenu.value/100;
        audioSourceMenu2.volume = sliderMenu.value/100;
        musicVolume=sliderMenu.value;
        SoundSlider.musicVolume=sliderMenu.value;
        ChangeValueSlider(textValue1,sliderMenu);
    }
    public void ValueChangeCheckButton(){
        audioSourceButton.volume=sliderButton.value/100;
        effectVolume=sliderButton.value;
        SoundSlider.effectVolume=sliderButton.value;
        ChangeValueSlider(textValue2,sliderButton);
    }
    public void MuteToggleCheck()
    {
        audioSourceMenu1.mute = muteToggle.isOn;
        audioSourceMenu2.mute = muteToggle.isOn;
        audioSourceButton.mute=muteToggle.isOn;
        mute=muteToggle.isOn;
        SoundSlider.mute=muteToggle.isOn;
    }
    public void ChangeValueSlider(TextMeshProUGUI textValue,Slider slider){
        textValue.text = slider.value.ToString();
    }
}

