using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class EventPopup : MonoBehaviour {
    
    [SerializeField] TMP_Text eventText;
    [SerializeField] TMP_Text termText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] GameObject definitionPanel;
    [SerializeField] TermDictionary dictionary;
    [SerializeField] UIEventChecker uiChecker;
    [SerializeField] Vector2 definitionOffset;

    UnityAction onExit;
    UIFadeable fader;

    public void Init() {
        fader = GetComponent<UIFadeable>();
        fader.Init();
        definitionPanel.SetActive(false);
        gameObject.SetActive(false);
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


    public void SetText(string text) {
        eventText.text = text;
    }

    public void Show() {
        gameObject.SetActive(true);
        StartCoroutine(fader.FadeIn());
    }

    public void Hide() {
        StartCoroutine(OnExit());
    }


    public void RegisterOnExit(UnityAction e) {
        onExit += e;
    }

    public void OnExitButton() {
        StartCoroutine(OnExit());
    }

    IEnumerator OnExit() {
        yield return StartCoroutine(fader.FadeOut());
        onExit?.Invoke();
        gameObject.SetActive(false);
    }


}
