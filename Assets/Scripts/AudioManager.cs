using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    public float masterVolumePercent = 1;
    public float sfxVolumePercent = 1;
    public float musicVolumePercent = 1;
    
    AudioSource[] MusicSources;
    int activeMusicSourceIndex = 0;
    Transform audioListener;
    Transform player;



    void Awake()
    {
        Instance = this;
        MusicSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            var newMusicSource = new GameObject($"Music source {i + 1}");
            var audioSource = newMusicSource.AddComponent<AudioSource>();
            MusicSources[i] = audioSource;
            newMusicSource.transform.parent = transform;
        }

        audioListener = GetComponent<AudioListener>().transform;
        player = GetComponent<Player>().transform;
    }

    void Update()
    {
        if (player != null)
        {
            audioListener.position = player.position;
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 2)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex; // Flip between the two;

        var activeMusicSource = MusicSources[activeMusicSourceIndex];
        activeMusicSource.clip = clip;
        activeMusicSource.Play();

        StartCoroutine(AnimateMusicCrossFade(fadeDuration));
        
    }


    IEnumerator AnimateMusicCrossFade(float duration)
    {
        float percent = 0;
        var activeMusicSource = MusicSources[activeMusicSourceIndex];
        var deactiveMusicSource = MusicSources[1- activeMusicSourceIndex];


        while (percent<1)
        {
            percent += Time.deltaTime * 1 / duration;
            var volume = musicVolumePercent * masterVolumePercent;
            activeMusicSource.volume = Mathf.Lerp(0, volume, percent);
            deactiveMusicSource.volume = Mathf.Lerp(volume, 0, percent);
            yield return null;

        }
    }



    public void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            return;
        }
        var volume = sfxVolumePercent * masterVolumePercent;
        AudioSource.PlayClipAtPoint(clip, position, volume);    
    }
}
