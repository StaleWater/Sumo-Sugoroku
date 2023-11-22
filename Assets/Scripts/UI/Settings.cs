using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] AudioMixerGroup musicGroup;
    [SerializeField] AudioMixerGroup sfxGroup;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    void Awake() {
        float val;
        musicGroup.audioMixer.GetFloat("musicvol", out val);
        val = MixerToSlider(val);
        musicSlider.value = val;

        sfxGroup.audioMixer.GetFloat("sfxvol", out val);
        val = MixerToSlider(val);
        sfxSlider.value = val;
    }

    float MixerToSlider(float val) {
        return Mathf.Pow(10, val / 20.0f);
    }

    float SliderToMixer(float val) {
        return Mathf.Log10(val) * 20.0f;
    }

    public void OnMusicSliderChange(float val) {
        val = SliderToMixer(val);
        musicGroup.audioMixer.SetFloat("musicvol", val);
    }

    public void OnSFXSLiderChange(float val) {
        val = SliderToMixer(val);
        sfxGroup.audioMixer.SetFloat("sfxvol", val);
    }

}
