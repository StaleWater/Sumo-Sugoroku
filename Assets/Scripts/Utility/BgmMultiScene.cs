using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BgmMultiScene : MonoBehaviour {
    [SerializeField] AudioSource bgm;

    void Start() {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void Play() {
        bgm.Play();
    }

    public void Stop() {
        bgm.Stop();
    }

}
