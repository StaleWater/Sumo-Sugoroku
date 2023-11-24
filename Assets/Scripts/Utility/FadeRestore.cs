using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeRestore : MonoBehaviour
{
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] float fadeDurationSEC;

    List<float> saved;

    public void Init() {
        SaveAlpha();
    }

    public void SaveAlpha() {
        var all = FadeTargets();
        saved = new List<float>();

        foreach((_, Color c) in all) {
            saved.Add(c.a);
        }
    }


    public void Hide() {
        var all = FadeTargets();
        foreach((var spr, var c) in all) {
            spr.color = new Color(c.r, c.g, c.b, 0.0f);
        }
    }

    public void Show() {
        var all = FadeTargets();

        foreach((var spr, var c) in all) {
            spr.color = new Color(c.r, c.g, c.b, 1.0f);
            spr.color = c;
        }
    }

    public IEnumerator FadeIn() {
        yield return StartCoroutine(RestoreFade());
    }

    public IEnumerator FadeOut() {
        yield return StartCoroutine(FadeTowards(0.0f));
    }

    public List<(SpriteRenderer, Color)> FadeTargets() {
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

    public IEnumerator RestoreFade() {
        var all = FadeTargets();

        float timePassed = 0.0f;
        while(timePassed < fadeDurationSEC) {
            timePassed += Time.deltaTime;

            for(int i=0; i < all.Count; i++) {
                (var spr, Color c) = all[i];
                float startAlpha = c.a;
                float endAlpha = saved[i];

                float curveX = timePassed / fadeDurationSEC;
                float curveY = fadeCurve.Evaluate(curveX);
                float alpha = Mathf.Lerp(startAlpha, endAlpha, curveY);

                c.a = alpha;
                spr.color = c;
            }

            yield return null;
        }
    }

    public IEnumerator FadeTowards(float endAlpha) {
        var all = FadeTargets();

        float timePassed = 0.0f;
        while(timePassed < fadeDurationSEC) {
            timePassed += Time.deltaTime;

            for(int i=0; i < all.Count; i++) {
                (var spr, Color c) = all[i];
                float startAlpha = c.a;
                float curveX = timePassed / fadeDurationSEC;
                float curveY = fadeCurve.Evaluate(curveX);
                float alpha = Mathf.Lerp(startAlpha, endAlpha, curveY);

                c.a = alpha;
                spr.color = c;
            }

            yield return null;
        }

    }
}
