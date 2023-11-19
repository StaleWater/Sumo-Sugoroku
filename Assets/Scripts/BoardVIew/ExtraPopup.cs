using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System;

public class ExtraPopup : MonoBehaviour {

	[SerializeField] TMP_Text eventText;
	[SerializeField] TMP_Text termText;

	[SerializeField] private GameObject transitionUI; // Buttons to exit and show the text
    [SerializeField] private GameObject informationUI; // Information

	[SerializeField] TMP_Text descriptionText;
	[SerializeField] GameObject definitionPanel;
	[SerializeField] TermDictionary dictionary;
	[SerializeField] UIEventChecker uiChecker;
	[SerializeField] Vector2 definitionOffset;

	// Initialization
	public void Init() {
        transitionUI.SetActive(false);
        informationUI.SetActive(false);
        gameObject.SetActive(false);
    }

    // Begin and show the pop-up interface
    public void Begin() {
        Debug.Log("Extra: begin");
		// TODO: add some animation here before showing UI
		gameObject.SetActive(true);
		transitionUI.SetActive(true);
	}

	// Close the extra pop-up
	public void Exit() {
		Debug.Log("Extra: exiting");
		StartCoroutine(ExitRoutine());
	}

	IEnumerator ExitRoutine() {
		yield return null;
		transitionUI.SetActive(false);
		informationUI.SetActive(false);
		gameObject.SetActive(false);
	}

	private void Update() {
		if (informationUI.activeSelf && Input.GetMouseButtonDown(0)) {
			if (definitionPanel.activeSelf && !uiChecker.ClickedOn(definitionPanel)) HideDefinition();
			else if (!definitionPanel.activeSelf) CheckLinkClick();
		}
	}

	private void CheckLinkClick() {
		Vector2 screenPos = Input.mousePosition;
		int index = TMP_TextUtilities.FindIntersectingLink(eventText, screenPos, null);
		if (index == -1) return;

		TMP_LinkInfo link = eventText.textInfo.linkInfo[index];

		string text = link.GetLinkText();
		string desc = dictionary.Lookup(text);

		ShowDefinition(text, desc);
	}

	private void ShowDefinition(string text, string desc) {
		termText.text = text;
		descriptionText.text = desc;
		definitionPanel.SetActive(true);
	}

	private void HideDefinition() {
		definitionPanel.SetActive(false);
	}

	// Set text
	public void SetText(in string text) {
		eventText.text = text;
	}

	// Show the extra information
	public void ShowInformation() {
        Debug.Log("Extra: showing information");
		transitionUI.SetActive(false);
        informationUI.SetActive(true);
	}

    // Hide the extra information
    public void HideInformation() {
        Debug.Log("Extra: hiding information");
		transitionUI.SetActive(true);
		informationUI.SetActive(false);
	}
}
