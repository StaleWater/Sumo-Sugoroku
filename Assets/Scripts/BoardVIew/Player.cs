using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] float movementDurationSEC;
    [SerializeField] AnimationCurve movementCurve;

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

}
