using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    [SerializeField] Vector3 distToPlayer;
    [SerializeField] AnimationCurve shakeCurve;
    [SerializeField] float shakeDist;
    [SerializeField] float shakeDurationSEC;

    Coroutine shakeRoutine; 
    Vector3 originPos;

    public void Init(SpriteRenderer player) {
        var pos = player.transform.position;
        pos.x += player.bounds.extents.x;
        pos += distToPlayer;
        transform.position = pos;
        originPos = pos;
    }

    public void Shake() {
        if(shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeHelper());
    }

    IEnumerator ShakeHelper() {
        transform.position = originPos;
        var startPos = originPos;
        var endPos = startPos;
        endPos.x += shakeDist;

        float timePassed = 0.0f;
        while(timePassed < shakeDurationSEC) {
            timePassed += Time.deltaTime;

            float curveX = timePassed / shakeDurationSEC;
            float curveY = shakeCurve.Evaluate(curveX);
            var pos = Vector3.Lerp(startPos, endPos, curveY);
            transform.position = pos;

            yield return null; 
        }

        transform.position = startPos;
    }
}
