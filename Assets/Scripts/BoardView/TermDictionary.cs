using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TermDictionary : MonoBehaviour {

    [SerializeField] DictUnit[] dictEditor;
    Dictionary<string, string> dict;

    public void Init() {
        BuildDictionary();
    }

    void BuildDictionary() {
        dict = new Dictionary<string, string>();
        foreach(var u in dictEditor) {
            dict[u.term] = u.description;
        }
    }

    public string Lookup(string term) {
        return dict[term];
    }

    public string TagTermsInString(string text) {
        foreach(var u in dictEditor) {
            string term = u.term;
            string newText = "";
            int si = 0;
            int i = text.IndexOf(term, si);
            while(i >= 0) {
                int head = i - 1;
                int tail = i + term.Length;

                newText += text.Substring(si, i - si);
                if((head < 0 || !Char.IsLetter(text[head])) && (tail >= text.Length || !Char.IsLetter(text[tail]))) {
                    newText += $"<link><color=#ce2c2f>{term}</color></link>";
                }

                si = tail;
                i = text.IndexOf(term, si);
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
