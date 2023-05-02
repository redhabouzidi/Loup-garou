using UnityEngine;
using UnityEngine.UI;

public class SoundSlider_GameScene : MonoBehaviour
{
    public Slider sliderMenu;
    public Slider sliderButton;
    public Toggle muteToggle;
    public AudioSource audioSourceMenu1;
    public AudioSource audioSourceMenu2;
    public AudioSource audioSourceButton;
    private bool isMuted = false;

    private void Start()
    {
        sliderMenu.onValueChanged.AddListener(delegate { ValueChangeCheckMenu(); });
        sliderButton.onValueChanged.AddListener(delegate { ValueChangeCheckButton(); });
        muteToggle.onValueChanged.AddListener(delegate { MuteToggleCheck(); });

    }

    public void ValueChangeCheckMenu()
    {
        audioSourceMenu1.volume = sliderMenu.value;
        audioSourceMenu2.volume = sliderMenu.value;
    }
    public void ValueChangeCheckButton(){
        audioSourceButton.volume=sliderButton.value;
    }
    public void MuteToggleCheck()
    {
        audioSourceMenu1.mute = muteToggle.isOn;
        audioSourceMenu2.mute = muteToggle.isOn;
        audioSourceButton.mute=muteToggle.isOn;
    }
}

