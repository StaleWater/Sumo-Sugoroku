using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class EventPopup : MonoBehaviour {
    
    [SerializeField] TMP_Text eventText;
    [SerializeField] TMP_Text termText;
    [SerializeField] GameObject eventPanel;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] GameObject definitionPanel;
    [SerializeField] TermDictionary dictionary;
    [SerializeField] UIEventChecker uiChecker;
    [SerializeField] Vector2 definitionOffset;

	UnityAction onExit;
	UIFadeable fader;

	[SerializeField] ExtraPopup extraPopup;
    private Tile tile;

    private Vector3 originalEventPanelPosition;
    private Vector2 originalEventPanelSizeDelta;

    public void Init() {
        fader = GetComponent<UIFadeable>();
        fader.Init();
        definitionPanel.SetActive(false);
        gameObject.SetActive(false);
		originalEventPanelPosition = eventPanel.GetComponent<RectTransform>().position;
		originalEventPanelSizeDelta = eventPanel.GetComponent<RectTransform>().sizeDelta;
        extraPopup.Init();
	}

    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            if(definitionPanel.activeSelf && !uiChecker.ClickedOn(definitionPanel)) HideDefinition();
            else if(!definitionPanel.activeSelf) CheckLinkClick();
        }
    }

    void CheckLinkClick() {
        Vector2 screenPos = Input.mousePosition;
        int index = TMP_TextUtilities.FindIntersectingLink(eventText, screenPos, null);
        if(index == -1) return;

        TMP_LinkInfo link = eventText.textInfo.linkInfo[index];

        string text = link.GetLinkText();
        string desc = dictionary.Lookup(text);

        ShowDefinition(text, desc);
    }

    void ShowDefinition(string text, string desc) {
        termText.text = text;
        descriptionText.text = desc;
        definitionPanel.SetActive(true);
    }

    void HideDefinition() {
        definitionPanel.SetActive(false);
    }

    public void ApplyOffsetScale(in Vector2 offsetScale) {
        eventPanel.GetComponent<RectTransform>().position *= offsetScale;
	}

	public void SetScale(in Vector2 scale)
	{
		eventPanel.GetComponent<RectTransform>().sizeDelta *= scale;
	}

	public void SetText(in string text) {
        eventText.text = text;
	}

    public void Show(Tile tile) {
        this.tile = tile;
        gameObject.SetActive(true);
        StartCoroutine(fader.FadeIn());
		tile.GetComponent<BoxCollider2D>().enabled = true; // Tile now clickable since event is occuring
	}

    public void Hide() {
		// StartCoroutine(OnExit());
		StartCoroutine(OnHide());
	}

    public void RegisterOnExit(UnityAction e) {
        onExit += e;
    }

    public void OnExitButton() {
        StartCoroutine(OnExit());
    }

    IEnumerator OnHide() {
		tile.GetComponent<BoxCollider2D>().enabled = false; // Disable the tile
		yield return StartCoroutine(fader.FadeOut());
		eventPanel.GetComponent<RectTransform>().position = originalEventPanelPosition;
		eventPanel.GetComponent<RectTransform>().sizeDelta = originalEventPanelSizeDelta;
		Debug.Log("Hiding popup");
		gameObject.SetActive(false);
	}

    IEnumerator OnExit() {
		tile.GetComponent<BoxCollider2D>().enabled = false; // Disable the tile
		yield return StartCoroutine(fader.FadeOut());
        onExit?.Invoke();
        gameObject.SetActive(false);
		eventPanel.GetComponent<RectTransform>().position = originalEventPanelPosition;
		eventPanel.GetComponent<RectTransform>().sizeDelta = originalEventPanelSizeDelta;
        Debug.Log("Pop up exited");
	}

    public void ExtraEvent(Tile tile)
    {
        Debug.Log("Hello World!");

		// Hide the event popup
		tile.GetComponent<BoxCollider2D>().enabled = false; // Disable the tile
		Hide();

        // Run the extra event
        StartCoroutine(extraPopup.Begin());
        
        // Show the event popup
        Show(tile);
		tile.GetComponent<BoxCollider2D>().enabled = true; // Enable the tile
	}
}
