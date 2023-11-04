using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SumoEnemy : MonoBehaviour {

    [SerializeField] float actionCooldownSEC;
    [SerializeField] ActionWeights[] weights;
    public SumoAction state;
    public bool active;

    Dictionary<SumoAction, int> weightsDict;
    SumoGuy guy;
    SumoFightManager man;
    Coroutine actionLoop;

    public enum SumoAction {
        MoveForward,
        MoveBack,
        Push,
        Block,
        Idle
    }

    [Serializable]
    public struct ActionWeights {
        public SumoAction action;
        public int weight;
    }

    public void Init(SumoFightManager m) {
        man = m;
        guy = GetComponent<SumoGuy>();
        guy.Init();
        state = SumoAction.Idle;
        weightsDict = new Dictionary<SumoAction, int>();
        foreach(var aw in weights) {
            weightsDict[aw.action] = aw.weight;
        }
        active = true;
    }

    public void StartActionLoop() {
        actionLoop = StartCoroutine(ActionLoop());
    }

    public void StopActionLoop() {
        if(actionLoop != null) StopCoroutine(actionLoop);
        state = SumoAction.Idle;
        guy.Idle();
    }

    IEnumerator ActionLoop() {
        var waiter = new WaitForSeconds(actionCooldownSEC);
        while(guy.alive) {
            (float pDist, float edgeDist) = man.GetEnemyInputData();
            NextAction(pDist, edgeDist);
            yield return waiter;
        }
    }

    void Update() {
        if(!active) return;
        int dir = guy.facingRight ? 0 : -1;
        switch(state) {
            case SumoAction.MoveForward:
                guy.Move(dir);
                break;
            case SumoAction.MoveBack:
                guy.Move(dir);
                break;
            default:
                guy.Idle();
                break;
        }
    }

    void TakeAction(SumoAction action) {
        switch(action) {
            case SumoAction.Push:
                guy.PushAttack();
                state = SumoAction.Idle;
                break;
            case SumoAction.Block:
                guy.Block();
                state = SumoAction.Block;
                break;
            default:
                state = action;
                break;
        }
    }

    void NextAction(float distToPlayer, float distToRingEdge) {
        if(state == SumoAction.Block) guy.EndBlock();

        var possible = new List<SumoAction>();
        int weightTotal = 0;

        possible.Add(SumoAction.Idle);
        if(distToPlayer <= guy.pushRange) possible.Add(SumoAction.Push);
        if(distToPlayer > guy.pushRange / 2.0f) possible.Add(SumoAction.MoveForward);
        if(distToRingEdge > guy.moveSpeed * actionCooldownSEC) possible.Add(SumoAction.MoveBack);
        if(distToPlayer <= guy.pushRange * 2.0f) possible.Add(SumoAction.Block);

        int numActions = possible.Count;
        var percents = new float[numActions];

        for(int i=0; i < numActions; i++) {
            int w = weightsDict[possible[i]];
            weightTotal += w;
            percents[i] = w;
        }

        for(int i=0; i < numActions; i++) {
            percents[i] /= weightTotal;
        }

        float p = UnityEngine.Random.Range(0.0f, 1.0f);

        for(int i=0; i < numActions; i++) {
            if(p <= percents[i]) {
                TakeAction(possible[i]);
                return;
            }
            p -= percents[i];
        }

        Debug.Log("Something went wrong in NextAction()");
        state = SumoAction.Idle;
    }
}
