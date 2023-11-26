using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmMultiScene : MonoBehaviour {
    [SerializeField] AudioSource bgm;
    public bool playing;

    public static BgmMultiScene instance;

    void Awake() {
        if(instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    public void Play() {
        playing = true;
        bgm.Play();
    }

    public void Stop() {
        playing = false;
        bgm.Stop();
    }

}
