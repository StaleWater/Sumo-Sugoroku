using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dice : MonoBehaviour {
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 rollForce;
    [SerializeField] Vector3 appliedPosOnCube;

    Rigidbody rb;
    BoxCollider box;

    public void Init() {
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        Reset(); 
    }

    public void Roll(UnityAction<int> onRoll) {
        rb.isKinematic = false;
        Vector3 appliedPos = startPos + Vector3.Scale(box.bounds.extents, appliedPosOnCube);
        rb.rotation = Random.rotation;
        rb.AddForceAtPosition(rollForce, appliedPos);
        StartCoroutine(WaitToFinishRoll(onRoll));
    }

    public void Reset() {
        rb.isKinematic = true;
        transform.position = startPos;
    }

    IEnumerator WaitToFinishRoll(UnityAction<int> onRoll) {
        var waiter = new WaitForSeconds(0.25f);
        while(!rb.IsSleeping()) yield return waiter;

        yield return null;

        onRoll?.Invoke(GetDiceNum());
    }

    int GetDiceNum() {
        var up = transform.up;
        var right = transform.right;
        var zp = Vector3.Cross(up, right);
        var realUp = new Vector3(0.0f, 0.0f, -1.0f);
        Vector3[] dirs = {-right, -zp, up, -up, zp, right};

        for(int i=0; i < 6; i++) {
            float diff = (dirs[i] - realUp).magnitude;
            if(diff < 0.01f) return i + 1;
        }

        return 0;
    }
}
