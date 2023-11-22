using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] float movementDurationSEC;
    [SerializeField] AnimationCurve movementCurve;
    [SerializeField] SpriteRenderer colorFilter;
    [SerializeField] SpriteRenderer icon;
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] float fadeDurationSEC;

    float colorFilterAlpha;
    List<float> saved;

    void Start() {
        colorFilterAlpha = colorFilter.color.a;
    }

    public void ResetAlpha() {
        Color c = colorFilter.color;
        c.a = colorFilterAlpha;
        colorFilter.color = c;
    }

    public IEnumerator MoveTo(Vector3 end) {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, end);
        if(distance < 0.01) yield break;

        float timePassed = 0.0f;

        while(timePassed <= movementDurationSEC) {
            timePassed += Time.deltaTime;

            float curveX = timePassed / movementDurationSEC;
            float curveY = movementCurve.Evaluate(curveX);

            Vector3 pos = Vector3.Lerp(start, end, curveY);
            transform.position = pos;

            yield return null;
        }
    }

    public void SetColor(int ci) {
        Vector4 n;
        switch(ci) {
            case 0:
                n = new Vector4(187, 80, 80, 157);
                break;
            case 1:
                n = new Vector4(231, 139, 58, 157);
                break;
            case 2:
                n = new Vector4(245, 228, 43, 157);
                break;
            case 3:
                n = new Vector4(80, 217, 88, 157);
                break;
            case 4:
                n = new Vector4(80, 143, 187, 157);
                break;
            case 5:
                n = new Vector4(117, 82, 183, 157);
                break;
            case 6:
                n = new Vector4(183, 82, 170, 157);
                break;
            case 7:
                n = new Vector4(127, 231, 215, 157);
                break;
            case 8:
                n = new Vector4(214, 241, 241, 157);
                break;

            default:
                n = Vector4.zero;
                break;
        }
        n /= 255.0f;

        colorFilter.color = new Color(n.x, n.y, n.z, n.w);
    }

    public void Hide() {
        var fade = GetComponent<Fadeable>();
        fade.Hide();
    }

    public void FadeIn() {
        StartCoroutine(FadeRestore());
    }

    public void FadeOut() {
        StartCoroutine(FadeTowards(0.0f));
    }

    IEnumerator FadeTowards(float endAlpha) {
        var all = FadeTargets();
        saved = new List<float>();
        foreach((_, var c) in all) {
            saved.Add(c.a);
        }

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

    IEnumerator FadeRestore() {
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

    public void SetIcon(Sprite s) {
        icon.sprite = s;
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


}
