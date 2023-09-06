using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fadeable : MonoBehaviour {

    [SerializeField] FadeableSettings defaultSettings;

    Coroutine currentFade;

    public bool IsFading() {
        return currentFade != null;
    }

    public void StopFading() {
        if(currentFade != null) StopCoroutine(currentFade);
        currentFade = null;
    }

    public void FadeIn(FadeableSettings settings = null, Action onComplete = null) {
        if(settings == null) settings = defaultSettings;

        StopFading();
        currentFade = StartCoroutine(FadeInHelper(settings, onComplete));
    }

    public void FadeOut(FadeableSettings settings = null, Action onComplete = null) {
        if(settings == null) settings = defaultSettings;

        StopFading();
        currentFade = StartCoroutine(FadeOutHelper(settings, onComplete));
    }

    IEnumerator FadeInHelper(FadeableSettings settings, Action onComplete) {
        var spr = GetComponent<SpriteRenderer>();
        Color oldColor = spr.color;
        float opacity = 0.0f;

        while(opacity < 1.0f) {
            opacity += Time.deltaTime * settings.fadeInSpeed;
            spr.color = new Color(oldColor.r, oldColor.g, oldColor.b, opacity);
            yield return null;
        }

        onComplete?.Invoke();
    }

    IEnumerator FadeOutHelper(FadeableSettings settings, Action onComplete) {
        var spr = GetComponent<SpriteRenderer>();
        Color oldColor = spr.color;
        float opacity = 1.0f;

        while(opacity > 0.0f) {
            opacity -= Time.deltaTime * settings.fadeOutSpeed;
            spr.color = new Color(oldColor.r, oldColor.g, oldColor.b, opacity);
            yield return null;
        }

        onComplete?.Invoke();
    }

}
