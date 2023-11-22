using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinLoseText : MonoBehaviour
{
    [SerializeField] CanvasGroup winContainer;
    [SerializeField] CanvasGroup loseContainer;
    [SerializeField] TMP_Text winText;
    [SerializeField] TMP_Text loseText;
    [SerializeField] AnimationCurve scaleCurve;
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] float fadeDurationSEC;
    [SerializeField] Vector3 fadeInStartScale;
    [SerializeField] Vector3 fadeInEndScale;
    [SerializeField] Vector3 fadeOutEndScale;


    public IEnumerator FadeIn(bool win) {
        CanvasGroup cg = win ? winContainer : loseContainer;

        Hide();
        cg.transform.localScale = fadeInStartScale;

        yield return Fade(cg, 1.0f, fadeInEndScale);
    }

    public IEnumerator FadeOut(bool win) {
        CanvasGroup cg = win ? winContainer : loseContainer;

        yield return Fade(cg, 0.0f, fadeOutEndScale);
    }

    public IEnumerator Fade(CanvasGroup cg, float endAlpha, Vector3 endScale) {
        Transform t = cg.transform;
        float startAlpha = cg.alpha;
        Vector3 startScale = t.localScale;

        float timePassed = 0.0f;
        while(timePassed < fadeDurationSEC) {
            timePassed += Time.deltaTime;

            float curveX = timePassed / fadeDurationSEC;
            float fadeY = fadeCurve.Evaluate(curveX);
            float scaleY = scaleCurve.Evaluate(curveX);

            float alpha = Mathf.Lerp(startAlpha, endAlpha, fadeY);
            Vector3 scale = Vector3.Lerp(startScale, endScale, scaleY);

            cg.alpha = alpha;
            t.localScale = scale;

            yield return null;
        }
    }

    public void Hide() {
        winContainer.alpha = 0.0f;
        loseContainer.alpha = 0.0f;
    }

    public void SetWinText(string text) {
        winText.text = text;
    }

    public void SetLoseText(string text) {
        loseText.text = text;
    }

}
