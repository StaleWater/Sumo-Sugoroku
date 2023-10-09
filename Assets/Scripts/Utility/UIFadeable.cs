using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIFadeable : MonoBehaviour {

    [SerializeField] float fadeDurationSEC;
    [SerializeField] AnimationCurve fadeInCurve;
    [SerializeField] AnimationCurve fadeOutCurve;

    CanvasGroup cg;

    public void Init() {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0.0f;
    }

    public void Show() {
        cg.alpha = 1.0f;
    }

    public void Hide() {
        cg.alpha = 0.0f;
    }

    public IEnumerator FadeIn() {
        if(cg.alpha < 1.0f) yield return StartCoroutine(Fade(0.0f, 1.0f, fadeInCurve));
    }

    public IEnumerator FadeOut() {
        if(cg.alpha > 0.0f) yield return StartCoroutine(Fade(1.0f, 0.0f, fadeOutCurve));
    }

    public IEnumerator Fade(float startOpacity, float endOpacity, AnimationCurve curve) {
        float timePassed = 0.0f;

        while(timePassed <= fadeDurationSEC) {
            float curveX = timePassed / fadeDurationSEC;
            float curveY = curve.Evaluate(curveX);
            cg.alpha = Mathf.Lerp(startOpacity, endOpacity, curveY);

            yield return null;

            timePassed += Time.deltaTime;
        }

        cg.alpha = endOpacity;
    }
}