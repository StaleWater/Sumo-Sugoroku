using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] float movementDurationSEC;
    [SerializeField] AnimationCurve movementCurve;
    [SerializeField] SpriteRenderer colorFilter;
    [SerializeField] SpriteRenderer icon;

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

    public void SetColor(Color c) {
        colorFilter.color = c;
    }

    public void SetColor(int ci) {
        Color c;
        switch(ci) {
            case 0:
                c = new Color(187, 80, 80, 157);
                break;
            case 1:
                c = new Color(231, 139, 58, 157);
                break;
            case 2:
                c = new Color(245, 228, 43, 157);
                break;
            case 3:
                c = new Color(80, 217, 88, 157);
                break;
            case 4:
                c = new Color(80, 143, 187, 157);
                break;
            case 5:
                c = new Color(117, 82, 183, 157);
                break;
            case 6:
                c = new Color(183, 82, 170, 157);
                break;
            case 7:
                c = new Color(127, 231, 215, 157);
                break;
            case 8:
                c = new Color(214, 241, 241, 157);
                break;

            default:
                c = Color.red;
                break;
        }
        Debug.Log($"SETCOLOR: {ci} -> {c}");

        colorFilter.color = c;
    }

    public void SetIcon(Sprite s) {
        icon.sprite = s;
    }

}
