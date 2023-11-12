using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TermDictionary : MonoBehaviour {

    [SerializeField] DictUnit[] dictEditor;
    SortedDictionary<string, string> dict;

    public void Init() {
        BuildDictionary();
    }

    void BuildDictionary() {
        dict = new SortedDictionary<string, string>();
        foreach(var u in dictEditor) {
            dict[u.term.ToLower()] = u.description;
        }
    }

    public SortedDictionary<string, string> GetDict() {
        return dict;
    }

    public string Lookup(string term) {
        return dict[term.ToLower()];
    }

    public string TagTermsInString(string text) {
        string ltext = text.ToLower();
        foreach(var u in dictEditor) {
            string term = u.term.ToLower();
            string newText = "";
            int si = 0;
            int i = ltext.IndexOf(term, si);
            while(i >= 0) {
                int head = i - 1;
                int tail = i + term.Length;

                newText += text.Substring(si, i - si);
                if((head < 0 || !Char.IsLetter(text[head])) && (tail >= text.Length || !Char.IsLetter(text[tail]))) {
                    string matched = text.Substring(i, term.Length);
                    newText += $"<link><color=#ce2c2f>{matched}</color></link>";
                }

                si = tail;
                i = ltext.IndexOf(term, si);
            }

            newText += text.Substring(si);

            text = newText;
        }


        return text;
    }


}

[Serializable]
public struct DictUnit {
    public string term;
    [TextArea(15,20)]
    public string description;
}
