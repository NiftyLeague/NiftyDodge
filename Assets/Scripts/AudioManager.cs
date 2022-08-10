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

    private string lastSoundIDPlayed;
    private float lastSoundTimer;

    private void Start()
    {
        Initialize();
    }

    void Update()
    {
        if (lastSoundTimer < 1)
        {
            lastSoundTimer += Time.unscaledDeltaTime;
        }
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

    public void PlaySound(string soundID, float volume = 1f, float addedPitch = 0)
    {
        if (soundID == lastSoundIDPlayed)
        {
            if (lastSoundTimer < 0.05f)
            {
                return;
            }
        }

        lastSoundTimer = 0;
        lastSoundIDPlayed = soundID;

        List<AudioSource> audioSourcesInSound = new List<AudioSource>();
        audioSourcesInSound = soundDictionary[soundID].audioSources;
        AudioSource soundEffectSource = audioSourcesInSound[UnityEngine.Random.Range(0, audioSourcesInSound.Count)];
        AudioSource currentEffectSource = soundEffectAudioSources[lastAudioSourceUsed];
        currentEffectSource.clip = soundEffectSource.clip;
        currentEffectSource.pitch = soundEffectSource.pitch + addedPitch + GetPitch(soundDictionary[soundID].pitchVariance);
        currentEffectSource.volume = volume;
        currentEffectSource.Play();

        lastAudioSourceUsed++;
        if (lastAudioSourceUsed >= 50)
        {
            lastAudioSourceUsed = 0;
        }
    }

    private float GetPitch(float variance)
    {
        return UnityEngine.Random.Range(-variance, variance);
    }

    [Serializable]
    public class Sound
    {
        public List<AudioSource> audioSources;
        public float pitchVariance;
        public string soundID;
    }
}
