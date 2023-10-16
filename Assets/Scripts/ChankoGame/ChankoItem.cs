using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChankoItem : MonoBehaviour {

    int id;
    ChankoManager man;

    public void Init(int id, ChankoManager man) {
        this.id = id;
        this.man = man; 
    }


    void OnMouseDown() {
        man.OnItemClick(id);
    }

}
