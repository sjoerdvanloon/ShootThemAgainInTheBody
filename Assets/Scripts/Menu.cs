using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static AudioManager;

public class Menu : MonoBehaviour
{
    public GameObject MainMenuHolder;
    public GameObject OptionsMenuHolder;
    public Slider[] VolumeSliders;
    public Toggle[] ResolutionToggles;
    public Toggle FullScreenToggle;
    public int[] ScreenWidths;

    int activeScreenResolutionIndex;
    bool sfxPlaying = false;

    //void Start()
    //{

    //}

    void Start()
    {
        activeScreenResolutionIndex = PlayerPrefs.GetInt("screen res index");

        VolumeSliders[0].value = AudioManager.Instance.masterVolumePercent;
        VolumeSliders[1].value = AudioManager.Instance.sfxVolumePercent;
        VolumeSliders[2].value = AudioManager.Instance.musicVolumePercent;

        for (int i = 0; i < ResolutionToggles.Length; i++)
        {
            var toggle = ResolutionToggles[i];
            toggle.isOn = i == activeScreenResolutionIndex;

        }

        bool isFullScreen = PlayerPrefs.GetInt("fullscreen") == 1;
        FullScreenToggle.isOn = isFullScreen;
    }


    public void Play()
    {
        SceneManager.LoadScene("Game");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GoToSettings()
    {
        MainMenuHolder.SetActive(false);
        OptionsMenuHolder.SetActive(true);
    }

    public void GoToMainMenu()
    {
        MainMenuHolder.SetActive(true);
        OptionsMenuHolder.SetActive(false);
    }

    public void SetScreenResolution(int index)
    {
        var toggle = ResolutionToggles[index];
        if (toggle.isOn)
        {
            print($"{toggle.name} on index {index} is on");
            activeScreenResolutionIndex = index;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(ScreenWidths[index], (int)(ScreenWidths[index] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenResolutionIndex);
            PlayerPrefs.Save();

        }
    }

    public void SetFullScreen(bool isFullScreen)
    {
        foreach (var toggle in ResolutionToggles)
        {
            toggle.interactable = !isFullScreen;
        }
        if (isFullScreen)
        {
            var allResolutions = Screen.resolutions;
            var maxResolution = allResolutions.Last();
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        }
        else
        {
            SetScreenResolution(activeScreenResolutionIndex);
        }

        PlayerPrefs.SetInt("fullscreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();

    }

    public void SetMasterVolume(float volume)
    {
        print(volume);
        AudioManager.Instance.SetVolume(Channel.Master, volume);
    }
    public void SetSfxVolume(float volume)
    {
        print(volume);
        AudioManager.Instance.SetVolume(Channel.Sfx, volume);

        if (!sfxPlaying)
            StartCoroutine(PlayOnce("Impact"));

    }

    IEnumerator PlayOnce(string soundName)
    {
        sfxPlaying = true;
        AudioManager.Instance.PlaySound(soundName);
        yield return new WaitForSeconds(0.5f);
        sfxPlaying = false;

    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetVolume(Channel.Music, volume);

    }

}
