using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GlossaryEntry : MonoBehaviour {
    [SerializeField] TMP_Text termText;
    [SerializeField] TMP_Text descText;

    public void SetText(string term, string description) {
        termText.text = term;
        descText.text = description;
    }
}
