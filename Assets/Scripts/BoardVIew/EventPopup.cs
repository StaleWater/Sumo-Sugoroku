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
    [SerializeField] TermDictionary dictionary;

    UnityAction onExit;
    CanvasGroup cg;

    public void Init() {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0.0f;
        gameObject.SetActive(false);
    }

    void Update() {
        if(Input.GetMouseButtonDown(0)) CheckLinkClick();
    }

    void CheckLinkClick() {
        Vector2 screenPos = Input.mousePosition;
        int index = TMP_TextUtilities.FindIntersectingLink(eventText, screenPos, null);
        if(index == -1) return;
        TMP_LinkInfo link = eventText.textInfo.linkInfo[index];
        string text = link.GetLinkText();
        string desc = dictionary.Lookup(text);
        Debug.Log($"{text}: {desc}");
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
