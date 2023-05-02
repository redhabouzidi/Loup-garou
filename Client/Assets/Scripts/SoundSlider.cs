using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    public Slider sliderMenu;
    public Slider sliderButton;
    public Toggle muteToggle;
    public AudioSource audioSourceMenu;
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
        audioSourceMenu.volume = sliderMenu.value;
    }
    public void ValueChangeCheckButton(){
        audioSourceButton.volume=sliderButton.value;
    }
    public void MuteToggleCheck()
    {
        audioSourceMenu.mute = muteToggle.isOn;
        audioSourceButton.mute=muteToggle.isOn;
    }
}

