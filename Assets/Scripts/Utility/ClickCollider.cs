using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClickCollider : MonoBehaviour {
    public UnityEvent onClick;

    void OnMouseDown() {
        onClick?.Invoke();
    }
}