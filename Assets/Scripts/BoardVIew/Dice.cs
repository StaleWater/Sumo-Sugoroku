using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum DiceDir {
    XPos,
    XNeg,
    YPos,
    YNeg,
    ZPos,
    ZNeg
}

public class Dice : MonoBehaviour {
    [SerializeField] Vector3 startPos;
    [SerializeField] Vector3 rollForce;
    [SerializeField] Vector3 appliedPosOnCube;
    [SerializeField] float soundVelocityThreshold;
    [SerializeField] float rollSoundDelay;

    AudioManager audioman;
    Rigidbody rb;
    BoxCollider box;
    Quaternion[] rollNumbers;

    public void Init() {
        audioman = GameObject.FindWithTag("audioman").GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
        InitRiggedDice();
        DiceReset(); 
    }

    void InitRiggedDice() {
        rollNumbers = new Quaternion[6];
        rollNumbers[0] = new Quaternion(0.00000f, 0.70711f, 0.00000f, 0.70711f);
        rollNumbers[1] = new Quaternion(0f, 0f, 0f, 1f);
        rollNumbers[2] = new Quaternion(0.70711f, 0f, 0f, 0.70711f);
        rollNumbers[3] = new Quaternion(-0.70711f, 0f, 0f, 0.70711f);
        rollNumbers[4] = new Quaternion(1f, 0f, 0f, 0f);
        rollNumbers[5] = new Quaternion(0.00000f, -0.70711f, 0.00000f, 0.70711f);
    }

    public void Roll(UnityAction<int> onRoll) {
        rb.isKinematic = false;
        Vector3 appliedPos = startPos + Vector3.Scale(box.bounds.extents, appliedPosOnCube);
        rb.rotation = Random.rotation;
        rb.AddForceAtPosition(rollForce, appliedPos);

        StartCoroutine(RollSound());
        StartCoroutine(WaitToFinishRoll(onRoll));
    }

    public void RiggedRoll(UnityAction<int> onRoll, int num) {
        rb.isKinematic = false;
        Vector3 appliedPos = startPos + Vector3.Scale(box.bounds.extents, appliedPosOnCube);

        rb.rotation = rollNumbers[num - 1];

        rb.AddForceAtPosition(rollForce, appliedPos);
        StartCoroutine(WaitToFinishRoll(onRoll));
    }

    public void DiceReset() {
        StopAllCoroutines();
        rb.isKinematic = true;
        transform.position = startPos;
    }

    IEnumerator WaitToFinishRoll(UnityAction<int> onRoll) {
        var waiter = new WaitForSeconds(0.25f);
        while(!rb.IsSleeping()) yield return waiter;

        yield return null;

        onRoll?.Invoke(GetDiceNum());
    }

    int GetUpDirIndex() {
        var up = transform.up;
        var right = transform.right;
        var zp = Vector3.Cross(up, right);
        var realUp = new Vector3(0.0f, 0.0f, -1.0f);

        Vector3[] dirs = {-right, -zp, up, -up, zp, right};
        float[] diffs = new float[6];

        int minI = 0;
        for(int i=0; i < 6; i++) {
            diffs[i] = (dirs[i] - realUp).magnitude;
            if(diffs[i] < diffs[minI]) minI = i;
        }

        return minI;
    }

    public DiceDir GetUpAxis() {
        DiceDir[] dds = {DiceDir.XNeg, DiceDir.ZNeg, DiceDir.YPos, DiceDir.YNeg, DiceDir.ZPos, DiceDir.XPos};
        int i = GetUpDirIndex(); 
        return dds[i];
    }

    public Quaternion GetUpDirRotation() {
        DiceDir dd = GetUpAxis();
        var diceRot = transform.rotation.eulerAngles;

        Quaternion output;
        switch(dd) {
            case DiceDir.XPos:
            case DiceDir.XNeg:
                output = Quaternion.Euler(0.0f, 0.0f, diceRot.x);
                break;
            case DiceDir.YPos:
            case DiceDir.YNeg:
                output = Quaternion.Euler(0.0f, 0.0f, diceRot.y);
                break;
            case DiceDir.ZPos:
            case DiceDir.ZNeg:
                output = Quaternion.Euler(0.0f, 0.0f, diceRot.z);
                break;
            default:
                output = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                break;
        }

        return output;
    }

    int GetDiceNum() {
        return GetUpDirIndex() + 1;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    IEnumerator RollSound() {
        yield return new WaitForSeconds(rollSoundDelay);
        audioman.Play("diceroll");
    }

    void OnCollisionEnter(Collision collision) {
        float v = collision.relativeVelocity.magnitude;
        if(v > soundVelocityThreshold) {
            float volume = Mathf.Min(v / 10.0f, 1.0f);
            audioman.Play("dicehit", volume);
        }
    }

}
