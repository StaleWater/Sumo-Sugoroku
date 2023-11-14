using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public enum TileContentType {
    Narrative,
    Extra
}

public class Tile : MonoBehaviour {

    [SerializeField] private UnityEvent clicked;
	private bool activeClick = false;

	public Orientation orientation;
    [TextArea(15,20)]
    [SerializeField] string narrative;
    [SerializeField] string extraContent;
    private string parsedNarrative;
    private string parsedExtraContent;
    public bool IsPortrait { get; private set; }

	private Material clickableMaterial;

    public void Init(TermDictionary dict) {
		GetComponent<BoxCollider2D>().enabled = false;

		clickableMaterial = GetComponent<Renderer>().material;

		parsedNarrative = dict.TagTermsInString(narrative);
		parsedExtraContent = dict.TagTermsInString(extraContent);

		Vector2 scale = GetComponent<SpriteRenderer>().bounds.size;
		bool isSideways = orientation == Orientation.Right || orientation == Orientation.Left;
		float ratio = isSideways ? scale.y / scale.x : scale.x / scale.y;
        IsPortrait = ratio <= 1; // Square is considered a portrait
	}

    public void Event(SugorokuManager man, TileContentType type, TileHighlight tileHighlight) {
		// Set up the tile highlight and run it
		tileHighlight.SetHighlight(this);

		// Start the event pop-up and send information about this tile
		switch (type) {
			case TileContentType.Narrative:
                Vector2 values = IsPortrait ? new Vector2(0.5f, 1.0f) : new Vector2(1.0f, 0.5f);
				man.ShowPopup(parsedNarrative, values, values, this);
				break;
			case TileContentType.Extra:
				man.ShowExtraPopup(parsedExtraContent);
				break;

			default:
                // TODO: do something with invalid type
                Debug.Log("Shouldn't print");
				break;
		}
    }

	public void OnMouseDown() {
        activeClick = true;
	}

	// Signal to Sugoroku Manager that this tile was clicked
	public void OnMouseUp() {
		if (activeClick) {
			clicked.Invoke();
            activeClick = false;
		}
	}

	public void OnMouseEnter() {
		clickableMaterial.SetFloat("_Is_Selected", 1.0f);
	}

	public void OnMouseExit() {
		clickableMaterial.SetFloat("_Is_Selected", 0.0f);
	}

	private bool continueRunningPulse = false;
	public void SetIsClickable(bool clickable) {

		if (clickable) {
			continueRunningPulse = true;
			StartCoroutine(RunPulse());
			GetComponent<BoxCollider2D>().enabled = true; // Enable the tile
		} else {
			GetComponent<BoxCollider2D>().enabled = false; // Disable the tile
			continueRunningPulse = false;
		}
	}

	private IEnumerator RunPulse() {

		float elapsedTime = 0.0f;
		float cycle = 2 * Mathf.PI / clickableMaterial.GetFloat("_Pulse_Speed");

		// Begin the animation
		while (continueRunningPulse) {
			elapsedTime += Time.deltaTime;
			elapsedTime %= cycle;
			clickableMaterial.SetFloat("_Elapsed_Time", elapsedTime);
			yield return null;
		}

		// Make sure the animation finishes its current cycle before stopping
		while (cycle - elapsedTime > 0.0f) {
			elapsedTime += Time.deltaTime;
			clickableMaterial.SetFloat("_Elapsed_Time", elapsedTime);
			yield return null;
		}
		clickableMaterial.SetFloat("_Elapsed_Time", 0.0f);
	}
}

public enum Orientation {
    Up,
    Right,
    Down,
    Left
}
