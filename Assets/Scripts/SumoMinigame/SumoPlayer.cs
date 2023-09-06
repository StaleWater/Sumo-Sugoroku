using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumoPlayer : MonoBehaviour {
    [Range(1.0f, 20.0f)]
    [SerializeField] float moveSpeed;

    void Update() {
        HandleInput();
    }

    void HandleInput() {
        float mov = Input.GetAxis("Horizontal");
        if(Mathf.Abs(mov) > 0.1f) Move(mov * moveSpeed * Time.deltaTime);
    }

    void Move(float delta) {
        var pos = transform.position;
        pos.x += delta;
        transform.position = pos;
    }

}

