using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class EventPopup : MonoBehaviour {
    
    [SerializeField] TMP_Text eventText;
    [SerializeField] float fadeDurationSEC;
    [SerializeField] AnimationCurve fadeCurve;

    UnityAction onExit;
    CanvasGroup cg;

    public void Init() {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0.0f;
        gameObject.SetActive(false);
    }

    public void SetText(string text) {
        eventText.text = text;
    }

    public void Show() {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    public void Hide() {
        StartCoroutine(OnExit());
    }

    IEnumerator FadeIn() {
        yield return StartCoroutine(Fade(0.0f, 1.0f));
    }

    IEnumerator FadeOut() {
        yield return StartCoroutine(Fade(1.0f, 0.0f));
    }

    public void RegisterOnExit(UnityAction e) {
        onExit += e;
    }

    public void OnExitButton() {
        StartCoroutine(OnExit());
    }

    IEnumerator OnExit() {
        yield return StartCoroutine(FadeOut());
        onExit?.Invoke();
        gameObject.SetActive(false);
    }

    IEnumerator Fade(float startOpacity, float endOpacity) {
        float timePassed = 0.0f;

        while(timePassed <= fadeDurationSEC) {
            float curveX = timePassed / fadeDurationSEC;
            float curveY = fadeCurve.Evaluate(curveX);
            cg.alpha = Mathf.Lerp(startOpacity, endOpacity, curveY);

            yield return null;

            timePassed += Time.deltaTime;
        }

        cg.alpha = endOpacity;
    }

}
