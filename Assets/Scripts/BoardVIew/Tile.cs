using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public Orientation orientation;
    [TextArea(15,20)]
    [SerializeField] string content;
    string parsedContent; 

    public void Init(TermDictionary dict) {
        parsedContent = dict.TagTermsInString(content);
    }

    public void Event(SugorokuManager man) {
        man.ShowPopup(parsedContent);
    }

}

public enum Orientation {
    Up,
    Right,
    Down,
    Left
}
