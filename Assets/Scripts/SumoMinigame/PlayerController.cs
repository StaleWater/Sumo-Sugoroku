using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    SumoGuy sumo;

    void Start() {
        sumo = GetComponent<SumoGuy>();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        float mov = Input.GetAxis("Horizontal");
        if(Mathf.Abs(mov) > 0.1f) sumo.Move(mov);
        else sumo.Idle();
        if(Input.GetButtonDown("Fire1")) sumo.PushAttack();
        if(Input.GetButtonDown("Fire2")) sumo.Block();
        if(Input.GetButtonUp("Fire2")) sumo.EndBlock();
    }


}

