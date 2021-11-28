using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SoundLibrary : MonoBehaviour
{
    public SoundGroup[] Groups;

    Dictionary<string, AudioClip[]> AudioClipLookup;

    System.Random random = new System.Random();


    void Awake()
    {
        AudioClipLookup = Groups.ToDictionary(x => x.GroupId.ToLower(), x => x.AudioClips);
    }

   public AudioClip GetAudioClip(string groupId)
    {
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentNullException(nameof(groupId));

        groupId = groupId.ToLower();

        if (!AudioClipLookup.ContainsKey(groupId))
            throw new ArgumentException($"Group {groupId} not found", nameof(groupId));


        var clips = AudioClipLookup[groupId];

        if (clips.Length == 1)
        {
            return clips[0];
        }
        else
        {
            return clips[random.Next(clips.Length)];
        }
    }

    [System.Serializable]
    public class SoundGroup
    {
        public string GroupId;
        public AudioClip[] AudioClips;
    }
}
