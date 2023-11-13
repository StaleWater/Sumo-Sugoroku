using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Glossary : MonoBehaviour
{
    [SerializeField] GlossaryEntry entryPrefab;
    [SerializeField] TermDictionary termDict;

    void Start() {
        Init();
    }

    void Init() {
        SortedDictionary<string, string> dict = termDict.GetDict();

        foreach(KeyValuePair<string, string> entry in dict) {
            SpawnEntry(entry.Key, entry.Value);
        }

        RectTransform ert = entryPrefab.GetComponent<RectTransform>();
        float entryHeight = ert.sizeDelta.y;
        int numEntries = dict.Count;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, entryHeight * numEntries);
    }

    GlossaryEntry SpawnEntry(string term, string description) {
        GlossaryEntry ge = Instantiate(entryPrefab, transform);
        ge.SetText(term, description);

        return ge;
    }
}
