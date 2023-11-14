using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Sound {
    public string name;
    public AudioClip clip;
    public bool loop;
    public bool playOnAwake;
    [Range(0.0f, 1.0f)]
    public float volume = 0.5f;
    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour {

    [SerializeField] Sound[] sounds;

    void Awake() {
        foreach(Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.clip = s.clip;

            if(s.playOnAwake) Play(s.name);
        }
    }

    public void Play(string soundName, float volume = -1.0f) {
        Sound s = Array.Find(sounds, x => x.name == soundName);
        if(s == null) Debug.Log($"Sound {soundName} does not exist");
        else {
            if(volume < 0.0f || volume > 1.0f) s.source.volume = s.volume;
            else s.source.volume = volume;
            s.source.Play();
        }
    }

    public void Stop(string soundName) {
        Sound s = Array.Find(sounds, x => x.name == soundName);
        if(s == null) Debug.Log($"Sound {soundName} does not exist");
        else s.source.Stop();

    }


}
