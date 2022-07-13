using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> soundEffectAudioSources;
    public List<Sound> soundList;
    private Dictionary<string, Sound> soundDictionary;
    private bool hasInitializedSoundDictionary;
    private int lastAudioSourceUsed;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (hasInitializedSoundDictionary)
        {
            return;
        }
            
        soundDictionary = new Dictionary<string, Sound>();
            
        foreach (Sound sound in soundList)   
        {
            soundDictionary.Add(sound.soundID, sound);        
        }
    
        hasInitializedSoundDictionary = true;
    }

    public void PlaySound(string soundID, float volume = 1f)
    {
        List<AudioSource> audioSourcesInSound = new List<AudioSource>();
        audioSourcesInSound = soundDictionary[soundID].audioSources;
        AudioSource soundEffectSource = audioSourcesInSound[UnityEngine.Random.Range(0, audioSourcesInSound.Count)];
        AudioSource currentEffectSource = soundEffectAudioSources[lastAudioSourceUsed];
        currentEffectSource.clip = soundEffectSource.clip;
        currentEffectSource.pitch = soundEffectSource.pitch;
        currentEffectSource.volume = volume;
        currentEffectSource.Play();

        lastAudioSourceUsed++;
        if (lastAudioSourceUsed >= 50)
        {
            lastAudioSourceUsed = 0;
        }
    }

    [Serializable]
    public class Sound
    {
        public List<AudioSource> audioSources;
        public string soundID;
    }
}
