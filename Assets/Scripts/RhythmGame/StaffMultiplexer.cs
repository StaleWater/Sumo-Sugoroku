using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaffMultiplexer {

    List<Staff> staves;

    public StaffMultiplexer(List<Staff> staves) {
        this.staves = staves;
    }

    public void StartReading() {
        foreach(Staff staff in staves) staff.StartReading();
    }

    public void SetSheetMusic(List<(float, int)> notes, float startDelayBEATS) {

        List<float>[] sheets = new List<float>[staves.Count];
        for(int i=0; i < sheets.Length; i++) sheets[i] = new List<float>();

        var waitSums = new float[staves.Count];
        var startDelays = new float[staves.Count];

        for(int i=0; i < notes.Count; i++) {
            (float note, int staffBits) = notes[i];

            for(int j=0; j < staves.Count; j++) {
                if((staffBits & (1 << j)) != 0) {
                    List<float> sheet = sheets[j];

                    if(sheet.Count > 0) sheet[sheet.Count - 1] += waitSums[j];
                    else startDelays[j] = startDelayBEATS + waitSums[j];
                    waitSums[j] = 0.0f;

                    sheet.Add(note);
                }
                else waitSums[j] += note;
            }

        }

        for(int i=0; i < staves.Count; i++) {
            staves[i].SetSheetMusic(sheets[i], startDelays[i]);
        }
    }
}
