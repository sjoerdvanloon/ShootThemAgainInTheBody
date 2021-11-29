using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip MainTheme;
    public AudioClip MenuTheme;

    string currentSceneName;


    void Start()
    {
        OnLevelWasLoaded(0);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnLevelWasLoaded(int sceneIndex)
    {
        var newSceneName = SceneManager.GetActiveScene().name;

        if (newSceneName != currentSceneName)
        {
            currentSceneName = newSceneName;
            // Switched
            Invoke("PlayMusic", .2f);  // Because of object destruction
        }
    }

    void PlayMusic()
    {
        AudioClip clipToPlay = null;
        if (currentSceneName == "Menu")
        {
            clipToPlay = MenuTheme;
        }
        else
        {
            clipToPlay = MainTheme;
        }

        if (clipToPlay != null)
        {

            AudioManager.Instance.PlayMusic(clipToPlay, 2);
            Invoke("PlayMusic", clipToPlay.length);
        }

    }

}
