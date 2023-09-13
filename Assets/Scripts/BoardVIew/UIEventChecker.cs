using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIEventChecker : MonoBehaviour
{
    [SerializeField] EventSystem ev;
    GraphicRaycaster ray;
    PointerEventData md;

    void Start() {
        ray = GetComponent<GraphicRaycaster>();        
        md = new PointerEventData(ev);
    }

    public bool ClickedOn(GameObject thing) {
        md.position = Input.mousePosition;
        var results = new List<RaycastResult>();
        ray.Raycast(md, results);
        foreach(RaycastResult res in results) {
            if(res.gameObject == thing) return true;
        }
        return false;
    }

}
