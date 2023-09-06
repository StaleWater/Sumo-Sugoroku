using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public Orientation orientation;

    public void Event(SugorokuManager man) {
        man.ShowPopup($"This is the event for {name}");
    }

}

public enum Orientation {
    Up,
    Right,
    Down,
    Left
}
