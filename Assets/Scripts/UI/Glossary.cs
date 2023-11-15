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
        Init();
        StartCoroutine(ActualFuckingMagic());
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

        container.Show();
    }

    void Init() {
        container.Init();
        container.Hide();

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
