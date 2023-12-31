using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChankoItem : MonoBehaviour {

    int typeID;
    ChankoManager man;
    bool clicked;
    bool clickable;
    public bool moving;
    public Fadeable fade;

    [SerializeField] ChankoItemSettings settings;

    public void Init(int id, ChankoManager man, bool clickable) {
        this.typeID = id;
        this.man = man; 
        this.clickable = clickable;
        moving = false;
        clicked = false;
        fade = GetComponent<Fadeable>();
    }

    public int Type() {
        return typeID;
    }

    void OnMouseDown() {
        if(clickable && !clicked) {
            clicked = true;
            StartCoroutine(man.OnItemClick(this));
        }
    }

    public void DestroyThis() {
        StopAllCoroutines();
        Destroy(gameObject);
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
