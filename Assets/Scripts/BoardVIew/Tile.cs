using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

// TODO: make it clickable if player lands on it, which will reveal extra information
// Idea is to highlight it or add some animation as feedback
public enum TileContentType {
    Narrative,
    Extra
}

public class Tile : MonoBehaviour {

    [SerializeField] private UnityEvent clicked;

    public Orientation orientation;
    [TextArea(15,20)]
    [SerializeField] string narrative;
    [SerializeField] string extraContent;
    private string parsedNarrative;
    private string parsedExtraContent;
    public bool isPortrait { get; private set; }

    private bool activeClick = false;

    public void Init(TermDictionary dict) {
		GetComponent<BoxCollider2D>().enabled = false;

		parsedNarrative = dict.TagTermsInString(narrative);
		parsedExtraContent = dict.TagTermsInString(extraContent);

		Vector2 scale = GetComponent<SpriteRenderer>().bounds.size;
		bool isSideways = orientation == Orientation.Right || orientation == Orientation.Left;
		float ratio = isSideways ? scale.y / scale.x : scale.x / scale.y;
        isPortrait = ratio <= 1; // Square is considered a portrait
	}

    public void Event(SugorokuManager man, TileContentType type) {
		switch (type) {
			case TileContentType.Narrative:
                Vector2 values = isPortrait ? new Vector2(0.5f, 1.0f) : new Vector2(1.0f, 0.5f);
				man.ShowPopup(parsedNarrative, values, values, this);
				break;
			case TileContentType.Extra:
                // TODO
				man.ShowPopup(parsedExtraContent, Vector2.one, Vector2.one, this);
				break;

			default:
                // TODO: do something with invalid type
                Debug.Log("Shouldn't print");
				break;
		}
    }

	public void OnMouseDown() {
        Debug.Log("A tile was clicked");
        activeClick = true;
	}

	// TODO
	// Signal to EventPopup that a tile was clicked via Sugoroku Manager
	public void OnMouseUp() {
		if (activeClick) {
			clicked.Invoke();
            activeClick = false;
		}
	}
}

public enum Orientation {
    Up,
    Right,
    Down,
    Left
}
