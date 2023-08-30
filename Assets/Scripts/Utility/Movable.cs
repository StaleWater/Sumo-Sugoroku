using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Movable : MonoBehaviour
{
    [SerializeField] MovableSettings defaultSettings;

    Coroutine currentMoveTo;

    public bool IsMoving() {
        return currentMoveTo != null;
    }

    public void StopMoving() {
        if(currentMoveTo != null) StopCoroutine(currentMoveTo);
        currentMoveTo = null;
    }

    public void MoveTo(Vector3 end, Action onComplete) {
        MoveTo(end, null, false, onComplete);
    }

    public void MoveTo(Vector3 end, MovableSettings settings = null, bool returnToStart = false, 
    Action onComplete = null, Action onEveryFrame = null) {

        if(settings == null) settings = defaultSettings;

        StopMoving();
        currentMoveTo = StartCoroutine(MoveToHelper(end, settings, returnToStart, onComplete, onEveryFrame));
    }

    IEnumerator MoveToHelper(Vector3 end, MovableSettings settings, bool returnToStart, 
    Action onComplete, Action onEveryFrame) {

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, end);
        if(distance < 0.01) yield break;

        float durationSEC = distance / settings.movementSpeed; 
        float timePassed = 0.0f;

        while(timePassed <= durationSEC) {
            timePassed += Time.deltaTime;

            float curveX = timePassed / durationSEC;
            float curveY = settings.curve.Evaluate(curveX);

            Vector3 pos = Vector3.Lerp(start, end, curveY);
            transform.position = pos;

            onEveryFrame?.Invoke();

            yield return null;
        }

        if(returnToStart) transform.position = start;

        currentMoveTo = null;
        onComplete?.Invoke();

    }

}
