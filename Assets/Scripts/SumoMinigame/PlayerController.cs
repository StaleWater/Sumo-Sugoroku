using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Range(1.0f, 20.0f)]
    [SerializeField] float moveSpeed;
    SumoGuy sumo;

    void Start() {
        sumo = GetComponent<SumoGuy>();
    }

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        float mov = Input.GetAxis("Horizontal");
        if(Mathf.Abs(mov) > 0.1f) sumo.Move(mov * moveSpeed * Time.deltaTime);
        if(Input.GetButtonDown("Fire1")) sumo.PushAttack();
    }


}

