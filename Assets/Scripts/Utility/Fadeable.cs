using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fadeable : MonoBehaviour {

    [SerializeField] FadeableSettings defaultSettings;

    Coroutine currentFade;

    public List<(SpriteRenderer, Color)> SelfAndKids() {
        var spr = GetComponent<SpriteRenderer>();
        var children = new List<(SpriteRenderer, Color)>();

        children.Add((spr, spr.color));

        foreach(Transform child in transform) {
            SpriteRenderer s = child.GetComponent<SpriteRenderer>();
            if(s != null) {
                children.Add((s, s.color));
            }
        }

        return children;
    }

    public bool IsVisible() {
        var spr = GetComponent<SpriteRenderer>();
        return spr.color.a > 0.001f;
    }

    public void Show() {
        var children = SelfAndKids();

        foreach((var s, var c) in children) {
            Color cn = c;
            cn.a = 1.0f; 
            s.color = cn;
        }
    }

    public void Hide() {
        var children = SelfAndKids();

        foreach((var s, var c) in children) {
            Color cn = c;
            cn.a = 0.0f; 
            s.color = cn;
        }
    }

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
        var children = SelfAndKids();

        float opacity = spr.color.a;

        while(opacity < 1.0f) {
            opacity += Time.deltaTime * settings.fadeInSpeed;
            foreach((SpriteRenderer s, Color c) in children) {
                Color cn = c;
                cn.a = opacity; 
                s.color = cn;
            }
            yield return null;
        }

        currentFade = null;

        onComplete?.Invoke();
    }

    IEnumerator FadeOutHelper(FadeableSettings settings, Action onComplete) {
        var spr = GetComponent<SpriteRenderer>();

        var children = SelfAndKids();

        float opacity = spr.color.a;

        while(opacity > 0.0f) {
            opacity -= Time.deltaTime * settings.fadeOutSpeed;

            foreach((SpriteRenderer s, Color c) in children) {
                Color cn = c;
                cn.a = opacity; 
                s.color = cn;
            }

            yield return null;
        }

        currentFade = null;

        onComplete?.Invoke();
    }

}
