using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Glossary : MonoBehaviour
{
    [SerializeField] GlossaryEntry entryPrefab;
    [SerializeField] TermDictionary termDict;
    [SerializeField] UIFadeable container;

    void Start() {
        StartCoroutine(StartupProcess());
    }

    IEnumerator StartupProcess() {
        Init();
        yield return StartCoroutine(ActualFuckingMagic());
        container.Show();
    }

    // I promise you the glossary stops working if you don't run this.
    // I cannot explain why. I'm sorry.
    IEnumerator ActualFuckingMagic() {
        var layout = GetComponent<VerticalLayoutGroup>();

        for(int i=0; i < 3; i++) {
            layout.enabled = false;
            yield return null;
            layout.enabled = true;
            yield return null;
        }
    }

    void Init() {
        container.Init();
        container.Hide();

        Dictionary<string, string> dict = termDict.GetDict();

        foreach(KeyValuePair<string, string> entry in dict) {
            SpawnEntry(entry.Key, entry.Value);
        }

    }

    GlossaryEntry SpawnEntry(string term, string description) {
        GlossaryEntry ge = Instantiate(entryPrefab, transform);
        string tagged = termDict.TagTermsInString(description, false);
        ge.SetText(term, tagged);

        return ge;
    }
}
