using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animated : MonoBehaviour
{
    [SerializeField] AnimatedSettings settings;

    public IEnumerator ScaleTo(Vector3 endScale, AnimationCurve curve = null, float durationSEC = -1.0f) {
        if(curve == null) curve = settings.scaleCurve;
        if(durationSEC < 0.0f) durationSEC = settings.scaleDurationSEC;

        float timePassed = 0.0f;
        Vector3 startScale = transform.localScale;

        while(timePassed < durationSEC) {
            timePassed += Time.deltaTime;
            float curveX = timePassed / durationSEC;
            float curveY = curve.Evaluate(curveX);
            var scale = Vector3.Lerp(startScale, endScale, curveY);
            transform.localScale = scale;
            yield return null;
        }
    }

}
