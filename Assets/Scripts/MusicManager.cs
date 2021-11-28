using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip MainTheme;
    public AudioClip MenuTheme;


    void Start()
    {
        AudioManager.Instance.PlayMusic(MenuTheme, 5);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlayMusic(MainTheme, 2); // Test
        }
    }
}
