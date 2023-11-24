using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    [SerializeField] float thinPlayerDistXRatio;
    [SerializeField] float fatPlayerDistXRatio;
    [SerializeField] float playerDistY;
    [SerializeField] AnimationCurve shakeCurve;
    [SerializeField] float shakeDist;
    [SerializeField] float shakeDurationSEC;

    Coroutine shakeRoutine; 
    Vector3 originPos;

    public void Init(SpriteRenderer player, bool fatPlayerSprite) {
        var pos = player.transform.position;

        if(fatPlayerSprite) pos.x += player.bounds.extents.x * fatPlayerDistXRatio;
        else pos.x += player.bounds.extents.x * thinPlayerDistXRatio;
        pos.y += playerDistY;
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
