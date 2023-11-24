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
    public float moveSpeed;
    public int endurance;
    public bool facingRight;
    public float pushRange;
    public bool alive;
    public bool blocking;

    // for use in RhythmManager.cs, set in inspector
    public bool largestSize;

    bool duringAction;
    float groundLevel;

    Material endBarMaterial;
    public BoxCollider2D box;
    Movable mov;
    RaycastHit2D[] raycastHits; 
    UnityAction onDeath;
    WaitForSeconds actionStunWaiter;
    Animator ani;
    AudioManager audioman;


    public void Init() {
        audioman = GameObject.FindWithTag("audioman").GetComponent<AudioManager>();
        box = GetComponent<BoxCollider2D>();
        mov = GetComponent<Movable>();
        ani = GetComponent<Animator>();
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

    public void Idle() {
        ani.SetFloat("Movement", 0.0f);
    }

    public void Move(float inputDelta) {
        if(duringAction) {
            Idle();
            return;
        }

        float delta = inputDelta * moveSpeed * Time.deltaTime;

        var pos = transform.position;
        pos.x += delta;
        if((facingRight && delta > 0.0f) || (!facingRight && delta < 0.0f)) {
            SumoGuy other = FindSumoInFront(Mathf.Abs(delta));
            if(other != null) {
                if(facingRight) {
                    float fatness = box.bounds.max.x - transform.position.x;
                    pos.x = other.box.bounds.min.x - fatness;
                }
                else {
                    float fatness = transform.position.x - box.bounds.min.x;
                    pos.x = other.box.bounds.max.x + fatness;
                }
            }
        }

        float trueDelta = Mathf.Abs(transform.position.x - pos.x);
        if(trueDelta < 0.001f) inputDelta = 0.0f;
        ani.SetFloat("Movement", inputDelta);

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
        if(other != null) {
            int dmg = pushDamage;
            float pushDist = pushBackDistance;

            if(other.blocking) {
                dmg /= 4;
                pushDist *= 0.1f;
                audioman.Play("push-blocked");
            }
            else audioman.Play("push");

            other.TakeDamage(dmg);
            other.ShiftBack(pushDist);
        }


        StartCoroutine(PushAnimation());
    }

    IEnumerator PushAnimation() {
        ani.SetBool("Pushing", true);
        yield return StartCoroutine(ActionStun());
        ani.SetBool("Pushing", false);
    }

    public void Block() {
        if(duringAction) return;

        ani.SetBool("Blocking", true);
        blocking = true;
        duringAction = true;
    }

    public void EndBlock() {
        ani.SetBool("Blocking", false);
        blocking = false;
        duringAction = false;
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
            if(!alive) yield break;
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
        onDeath?.Invoke();
    }

    void ShowEnduranceBar() {
        enduranceBarSpr.enabled = true;
    }

    void HideEnduranceBar() {
        enduranceBarSpr.enabled = false;
    }
}

