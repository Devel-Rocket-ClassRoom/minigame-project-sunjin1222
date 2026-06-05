using UnityEngine;
using UnityEngine.UI;

public class Settingmanager : MonoBehaviour
{
    public GameObject settingPanel;

    public Button Quit;
    public Button Close;

    public Slider sound;

    public Toggle muteToggle;
    public Toggle soundOnToggle;

    private float previousVolume = 1f;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bool isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;

        previousVolume = savedVolume;
        sound.value = savedVolume;

        muteToggle.isOn = isMuted;
        soundOnToggle.isOn = !isMuted;

        AudioListener.volume = isMuted ? 0f : savedVolume;

        sound.onValueChanged.AddListener(OnChangeVolume);
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        soundOnToggle.onValueChanged.AddListener(OnSoundOnToggleChanged);

        Quit.onClick.AddListener(OnQuitGame);
        Close.onClick.AddListener(OnCloseSetting);
    }

    public void OnChangeVolume(float value)
    {
        previousVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);

        if (soundOnToggle.isOn)
        {
            AudioListener.volume = value;
        }
    }

    public void OnMuteToggleChanged(bool isOn)
    {
        if (!isOn) return;

        AudioListener.volume = 0f;
        PlayerPrefs.SetInt("Muted", 1);
    }

    public void OnSoundOnToggleChanged(bool isOn)
    {
        if (!isOn) return;

        AudioListener.volume = previousVolume;
        PlayerPrefs.SetInt("Muted", 0);
    }

    public void OnQuitGame()
    {
        Application.Quit();
    }

    public void OnOpenSetting()
    {
        settingPanel.SetActive(true);
    }

    public void OnCloseSetting()
    {
        settingPanel.SetActive(false);
    }
}