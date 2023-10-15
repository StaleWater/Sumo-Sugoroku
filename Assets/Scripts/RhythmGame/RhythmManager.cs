using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class RhythmManager : MonoBehaviour
{
    [SerializeField] float reactionTimeSEC;
    [SerializeField] float tempoQBPM; // for user input, a beat is considered a quater note. 
    [SerializeField] float startDelaySEC;
    [SerializeField] float hitToleranceBEATS;
    [SerializeField] float missToleranceBEATS;
    [SerializeField] Staff staff1;
    [SerializeField] Staff staff2;
    [SerializeField] Staff staff3;
    [SerializeField] Staff staff4;
    [SerializeField] Animator sumoAni;
    [SerializeField] float pushAniDurationSEC;

    // note values in units of beats.
    // internally, a beat is considered a 16th note to avoid floating point error issues.
    public static float SIXTEENTH_NOTE = 1.0f;
    public static float EIGHTH_NOTE = 2.0f;
    public static float QUARTER_NOTE = 4.0f;
    public static float HALF_NOTE = 8.0f;
    public static float WHOLE_NOTE = 16.0f;

    float reactionTimeBEATS;
    float startDelayBEATS;
    float tempoBPS; // 16th note based beats per second

    WaitForSeconds pushWaiter;
    Coroutine pushAniRoutine;
    StaffMultiplexer smp;

    void Start() {
        Init();
    }

    public void Init() {
        tempoBPS = (tempoQBPM * QUARTER_NOTE) / 60.0f;
        reactionTimeBEATS = reactionTimeSEC * tempoBPS;
        startDelayBEATS = startDelaySEC * tempoBPS;

        staff1.Init(KeyCode.UpArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff2.Init(KeyCode.RightArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff3.Init(KeyCode.LeftArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff4.Init(KeyCode.DownArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        UnityAction<bool, bool> onHitAttempt = OnHitAttempt;

        staff1.RegisterOnHitAttempt(onHitAttempt);
        staff2.RegisterOnHitAttempt(onHitAttempt);
        staff3.RegisterOnHitAttempt(onHitAttempt);
        staff4.RegisterOnHitAttempt(onHitAttempt);

        pushWaiter = new WaitForSeconds(pushAniDurationSEC);

        smp = new StaffMultiplexer(new List<Staff>() {staff1, staff2, staff3, staff4});

        float q = QUARTER_NOTE;
        float e = EIGHTH_NOTE;
        float s = SIXTEENTH_NOTE;

        // use bit masks to assign a note to multiple staves 
        int s1 = 1;
        int s2 = 2;
        int s3 = 4;
        int s4 = 8;

        List<(float, int)> notes = new List<(float, int)>() {
            (q, s1),
            (q, s1|s2),
            (e, s3),
            (e, s3),
            (q, s1),
            (q, s4),
            (q+e, s1|s2),
            (e, s2|s4),
            (e, s2|s4),
        };

        smp.SetSheetMusic(notes, startDelayBEATS);
    }

    public void StartGame() {
        smp.StartReading();
    }

    // an attempt can both not hit and not miss if there are no notes nearby
    void OnHitAttempt(bool hit, bool miss) {
        if(hit || !miss) {
            if(pushAniRoutine != null) StopCoroutine(pushAniRoutine);
            pushAniRoutine = StartCoroutine(PushAnimation());
        }
    }

    IEnumerator PushAnimation() {
        sumoAni.SetBool("Pushing", true);
        yield return pushWaiter;
        sumoAni.SetBool("Pushing", false);
    }

}
