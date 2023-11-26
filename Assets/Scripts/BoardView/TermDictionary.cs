using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TermDictionary : MonoBehaviour {

    [SerializeField] DictUnit[] dictEditor;
    Dictionary<string, string> dict;

    void Awake() {
        BuildDictionary();
    }

    void BuildDictionary() {
        dict = new Dictionary<string, string>();
        foreach(var u in dictEditor) {
            dict[u.term.ToLower()] = u.description;
        }
    }

    public Dictionary<string, string> GetDict() {
        return dict;
    }

    public string Lookup(string term) {
        return dict[term.ToLower()];
    }

    public string TagTermsInString(string text, bool color = true) {
        if(text.Length < 1) return text;

        foreach(var u in dictEditor) {
			string ltext = text.ToLower();
			string term = u.term.ToLower();
            string newText = "";
            int si = 0;
            int i = ltext.IndexOf(term, si);
            while(i >= 0) {
				int head = i - 1;
                int tail = i + term.Length;

                newText += text.Substring(si, i - si);
                string matched = text.Substring(i, term.Length);
				if ((head < 0 || !Char.IsLetter(text[head])) && (tail >= text.Length || !Char.IsLetter(text[tail]))) {
                    if(color) newText += $"<link><i><color=#ce2c2f>{matched}</color></i></link>";
                    else newText += $"<link><i>{matched}</i></link>";
                }
                else newText += matched;

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
