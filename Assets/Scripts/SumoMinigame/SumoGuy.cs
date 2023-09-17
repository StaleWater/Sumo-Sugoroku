using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SumoGuy : MonoBehaviour {

    [SerializeField] int maxEndurance;
    [SerializeField] float enduranceRecoveryRate;
    [SerializeField] int pushDamage;
    [SerializeField] float pushBackDistance;
    [SerializeField] SpriteRenderer enduranceBarSpr;
    [SerializeField] float actionStunTimeSEC;

    public int endurance;
    public bool facingRight;
    public float pushRange;
    public bool alive;
    public bool blocking;
    bool duringAction;
    float groundLevel;

    Material endBarMaterial;
    public BoxCollider2D box;
    Movable mov;
    RaycastHit2D[] raycastHits; 
    UnityAction onDeath;
    WaitForSeconds actionStunWaiter;


    public void Init() {
        box = GetComponent<BoxCollider2D>();
        mov = GetComponent<Movable>();
        raycastHits = new RaycastHit2D[5];
        endurance = maxEndurance;
        endBarMaterial = new Material(enduranceBarSpr.material);
        UpdateEndBarMaterial();
        HideEnduranceBar();
        actionStunWaiter = new WaitForSeconds(actionStunTimeSEC);
        alive = true;
        duringAction = false;
        groundLevel = transform.position.y;
    }

    public void SetOnDeath(UnityAction ond) {
        onDeath = ond;
    }

    public void Move(float delta) {
        if(duringAction) return;

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
        if(duringAction) return;

        SumoGuy other = FindSumoInFront(pushRange);
        if(other == null) return;

        int dmg = pushDamage;
        float pushDist = pushBackDistance;

        if(other.blocking) {
            dmg /= 4;
            pushDist *= 0.1f;
        }

        other.TakeDamage(dmg);
        other.ShiftBack(pushDist);

        StartCoroutine(ActionStun());
    }

    public void Block() {
        if(duringAction) return;

        blocking = true;
        duringAction = true;
        var pos = transform.position;
        pos.y -= 0.1f;
        transform.position = pos;
    }

    public void EndBlock() {
        blocking = false;
        duringAction = false;
        var pos = transform.position;
        pos.y = groundLevel;
        transform.position = pos;
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

    IEnumerator ActionStun() {
        duringAction = true;
        yield return actionStunWaiter;
        duringAction = false;
    }

    IEnumerator PassiveRecovery() {
        var waiter = new WaitForSeconds(enduranceRecoveryRate);
        while(endurance < maxEndurance) {
            yield return waiter;
            Recover(1);
        }
    }

    public void Recover(int amount) {
        if(endurance == maxEndurance || amount == 0) return;

        endurance += amount;
        UpdateEndBarMaterial();
        if(endurance >= maxEndurance) {
            endurance = maxEndurance;
            HideEnduranceBar();
        }
    }

    void UpdateEndBarMaterial() {
        float per = endurance / (float)maxEndurance;
        endBarMaterial.SetFloat("_Percent", per);
        enduranceBarSpr.material = endBarMaterial;
    }

    public void TakeDamage(int amount) {
        if(endurance == 0 || amount == 0) return;

        bool wasAtMax = endurance == maxEndurance;

        endurance -= amount;
        UpdateEndBarMaterial();
        if(endurance < 0) {
            endurance = 0;
            Die();
        }
        else if(wasAtMax) {
            ShowEnduranceBar();
            StartCoroutine(PassiveRecovery());
        }

        if(!blocking) StartCoroutine(ActionStun());

    }

    public void Die() {
        alive = false;
        duringAction = true;
        Debug.Log("You died");
        onDeath?.Invoke();
    }

    void ShowEnduranceBar() {
        enduranceBarSpr.enabled = true;
    }

    void HideEnduranceBar() {
        enduranceBarSpr.enabled = false;
    }
}

