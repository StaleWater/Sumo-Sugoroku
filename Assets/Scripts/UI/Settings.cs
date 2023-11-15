using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    [SerializeField] AudioMixerGroup musicGroup;
    [SerializeField] AudioMixerGroup sfxGroup;

    public void OnMusicSliderChange(float val) {
        float logval = Mathf.Log10(val) * 20.0f;
        musicGroup.audioMixer.SetFloat("musicvol", logval);
    }

    public void OnSFXSLiderChange(float val) {
        float logval = Mathf.Log10(val) * 20.0f;
        sfxGroup.audioMixer.SetFloat("sfxvol", logval);
    }

}
