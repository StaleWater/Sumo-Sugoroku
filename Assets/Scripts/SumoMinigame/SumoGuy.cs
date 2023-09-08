using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumoGuy : MonoBehaviour {

    public int endurance;
    public bool facingRight;
    [SerializeField] int maxEndurance;
    [SerializeField] int enduranceRecoveryPerSec;
    [SerializeField] int pushDamage;
    [SerializeField] float pushBackDistance;
    [SerializeField] float pushRange;

    public BoxCollider2D box;
    Movable mov;
    RaycastHit2D[] raycastHits; 


    public void Init() {
        box = GetComponent<BoxCollider2D>();
        mov = GetComponent<Movable>();
        raycastHits = new RaycastHit2D[5];
        endurance = maxEndurance;
    }

    public void Move(float delta) {
        var pos = transform.position;
        pos.x += delta;
        if((facingRight && delta > 0.0f) || (!facingRight && delta < 0.0f)) {
            SumoGuy other = FindSumoInFront(Mathf.Abs(delta));
            if(other != null) {
                if(facingRight) pos.x = other.box.bounds.min.x - box.bounds.extents.x;
                else pos.x = other.box.bounds.max.x + box.bounds.extents.x;
            }
        }

        transform.position = pos;
    }

    public void ShiftBack(float delta) {
        var pos = transform.position;
        pos.x += facingRight ? -delta : delta;
        mov.MoveTo(pos);
    }

    public void PushAttack() {
        SumoGuy other = FindSumoInFront(pushRange);
        if(other == null) return;

        other.TakeDamage(pushDamage);
        other.ShiftBack(pushBackDistance);
    }

    SumoGuy FindSumoInFront(float distance) {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;

        int numHits = box.Cast(dir, raycastHits, distance);
        SumoGuy guy = null;
        for(int i=0; i < numHits; i++) {
            guy = raycastHits[i].collider.GetComponent<SumoGuy>();
            if(guy != null) break;
        }

        return guy;
    }

    IEnumerator PassiveRecovery() {
        var waiter = new WaitForSeconds(1);
        while(endurance < maxEndurance) {
            yield return waiter;
            Recover(enduranceRecoveryPerSec);
        }
    }

    public void Recover(int amount) {
        if(endurance == maxEndurance || amount == 0) return;

        endurance += amount;
        if(endurance > maxEndurance) {
            endurance = maxEndurance;
            HideEnduranceBar();
        }
    }

    public void TakeDamage(int amount) {
        if(endurance == 0 || amount == 0) return;

        bool wasAtMax = endurance == maxEndurance;

        endurance -= amount;
        if(endurance < 0) {
            endurance = 0;
            Die();
        }
        else if(wasAtMax) {
            ShowEnduranceBar();
            StartCoroutine(PassiveRecovery());
        }

    }

    void Die() {
        Debug.Log("You died");
    }

    void ShowEnduranceBar() {

    }

    void HideEnduranceBar() {

    }
}

