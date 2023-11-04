using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChankoItem : MonoBehaviour {

    int id;
    ChankoManager man;
    Vector3 originPos;
    Vector3 originScale;
    Quaternion originRot;
    public bool moving;
    public Fadeable fade;

    [SerializeField] ChankoItemSettings settings;

    public void Init(int id, ChankoManager man) {
        this.id = id;
        this.man = man; 
        originPos = transform.position;
        originScale = transform.localScale;
        originRot = transform.localRotation;
        moving = false;
        fade = GetComponent<Fadeable>();
    }

    public void ResetState() {
       StopAllCoroutines();
       transform.position = originPos; 
       transform.localScale = originScale;
       transform.localRotation = originRot;
       moving = false;
    }

    void OnMouseDown() {
        StartCoroutine(man.OnItemClick(id));
    }

    public IEnumerator FlyTo(Vector3 endPos) {
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * settings.endScalePercentage;
        float rotPerSec = settings.flyNumRotations / settings.flyDurationSEC;
        float degPerSec = rotPerSec * 360.0f;
        Quaternion startRot = transform.localRotation;

        float timePassed = 0.0f;

        moving = true;

        while(timePassed <= settings.flyDurationSEC) {

            float curveX = timePassed / settings.flyDurationSEC;
            float newX = Mathf.Lerp(startPos.x, endPos.x, settings.xMoveCurve.Evaluate(curveX));
            float newY = Mathf.LerpUnclamped(startPos.y, endPos.y, settings.yMoveCurve.Evaluate(curveX));
            float newZ = Mathf.Lerp(startPos.z, endPos.z, settings.zMoveCurve.Evaluate(curveX));
            var newPos = new Vector3(newX, newY, newZ);
            var newScale = Vector3.Lerp(startScale, endScale, settings.scaleCurve.Evaluate(curveX));
            Quaternion rotDist = Quaternion.Euler(0.0f, 0.0f, degPerSec * timePassed);
            Quaternion newRot = startRot * rotDist;

            transform.position = newPos;
            transform.localScale = newScale;
            transform.localRotation = newRot;

            yield return null;

            timePassed += Time.deltaTime;
        }

        moving = false;

    }

}
