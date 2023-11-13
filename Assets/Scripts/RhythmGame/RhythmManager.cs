using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


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
    [SerializeField] float pushAniDurationSEC;
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] float hitPercentToWin;
    [SerializeField] UIFadeable instructionsPanel;
    [SerializeField] UIFadeable infoTextContainer;
    [SerializeField] Vector3 playerSpritePos;
    [SerializeField] Vector3 playerSpriteScale;
    [SerializeField] SumoGuy defaultPlayerPrefab;

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
    Animator playerSprite;

    void Start() {
        Init();
    }

    public void Init() {
        playerSprite = SpawnPlayerSprite();

        tempoBPS = (tempoQBPM * QUARTER_NOTE) / 60.0f;
        reactionTimeBEATS = reactionTimeSEC * tempoBPS;
        startDelayBEATS = startDelaySEC * tempoBPS;

        screenCurtain.Init();
        screenCurtain.gameObject.SetActive(true);

        infoTextContainer.Init();
        infoTextContainer.gameObject.SetActive(true);
        infoTextContainer.Show();

        instructionsPanel.Init();
        instructionsPanel.gameObject.SetActive(true);
        instructionsPanel.Hide();


        staff1.Init(KeyCode.LeftArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff2.Init(KeyCode.DownArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff3.Init(KeyCode.UpArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff4.Init(KeyCode.RightArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        UnityAction<bool, bool> onHitAttempt = OnHitAttempt;

        staff1.RegisterOnHitAttempt(onHitAttempt);
        staff2.RegisterOnHitAttempt(onHitAttempt);
        staff3.RegisterOnHitAttempt(onHitAttempt);
        staff4.RegisterOnHitAttempt(onHitAttempt);

        pushWaiter = new WaitForSeconds(pushAniDurationSEC);

        smp = new StaffMultiplexer(new List<Staff>() {staff1, staff2, staff3, staff4});

        var notes = GetSheetMusic();

        smp.SetSheetMusic(notes, startDelayBEATS);

        if(Level() == 1) {
            instructionsPanel.Show();
        }
        else StartCoroutine(WaitToStart());

        StartCoroutine(Prep());
    }

    IEnumerator WaitToStart() {
        yield return StartCoroutine(instructionsPanel.FadeOut());
        instructionsPanel.gameObject.SetActive(false);

        while(!Input.GetKeyDown(KeyCode.Space)) yield return null;
        yield return StartCoroutine(infoTextContainer.FadeOut());
        StartGame();
    }

    Animator SpawnPlayerSprite() {
        SumoGuy prefab = defaultPlayerPrefab;
        if(SugorokuManager.stateData.usingState) {
            int pi = SugorokuManager.stateData.curPlayer;
            prefab = SugorokuManager.stateData.players[pi].data.spritePrefab;
        }

        var guy = Instantiate(prefab, transform);
        guy.transform.localPosition = playerSpritePos;
        guy.transform.localScale = playerSpriteScale;
        guy.GetComponent<PlayerController>().enabled = false;

        foreach(Transform child in guy.transform) {
            Destroy(child.gameObject);
        }

        return guy.GetComponent<Animator>();
    }

    public void OnInstructionsClose() {
        StartCoroutine(WaitToStart());
    }

    IEnumerator Prep() {
        screenCurtain.Show();
        yield return StartCoroutine(screenCurtain.FadeOut());
        yield return new WaitForSeconds(1.0f);
    }

    int Level() {
        if(SugorokuManager.stateData.usingState) {
            return SugorokuManager.stateData.players[SugorokuManager.stateData.curPlayer].data.teppoLevel;
        }
        else return 1;
    } 

    List<(float, int)> GetSheetMusic() {
        int difficulty = Level();
        difficulty = Mathf.Min(Mathf.Max(1, difficulty), 5);

        switch(difficulty) {
            case 1:
                return SheetMusic1();
            case 2:
                return SheetMusic2();
            case 3:
                return SheetMusic3();
            case 4:
                return SheetMusic4();
            case 5:
            default:
                return SheetMusic5();
        }

    }


    void StartGame() {
        StartCoroutine(Game());
    }

    IEnumerator Game() {
        yield return StartCoroutine(smp.Read());

        (int notesHit, int totalNotes) = smp.GetHitRate();

        bool win = (notesHit / (float)totalNotes) >= hitPercentToWin;

        if(win) Debug.Log("YOU WIN");
        else Debug.Log("YOU LOSE");

        SugorokuManager.stateData.wonMinigame = win;

        yield return StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(3.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

    // an attempt can both not hit and not miss if there are no notes nearby
    void OnHitAttempt(bool hit, bool miss) {
        if(hit || !miss) {
            if(pushAniRoutine != null) StopCoroutine(pushAniRoutine);
            pushAniRoutine = StartCoroutine(PushAnimation());
        }
    }

    IEnumerator PushAnimation() {
        playerSprite.SetBool("Pushing", true);
        yield return pushWaiter;
        playerSprite.SetBool("Pushing", false);
    }

    List<(float, int)> SheetMusic1() {
        float h = HALF_NOTE;
        float q = QUARTER_NOTE;
        float e = EIGHTH_NOTE;
        float s = SIXTEENTH_NOTE;

        // use bit masks to assign a note to multiple staves 
        int l = 1;
        int d = 2;
        int u = 4;
        int r = 8;

        List<(float, int)> notes = new List<(float, int)>() {
            (h, l),
            (h, r),
            (q, l),
            (q, l),
            (h, r),

            (h, u),
            (h, d),
            (q, u),
            (q, u),
            (h, d),

            (q, l),
            (q, d),
            (q, u),
            (q, r),

            (q, r),
            (q, u),
            (q, d),
            (q, l),

            (h, u),
            (h, d),
            (q, u),
            (q, u),
            (h, d),
        };

        return notes;
    }

    List<(float, int)> SheetMusic2() {
        float h = HALF_NOTE;
        float q = QUARTER_NOTE;
        float e = EIGHTH_NOTE;
        float s = SIXTEENTH_NOTE;

        // use bit masks to assign a note to multiple staves 
        int l = 1;
        int d = 2;
        int u = 4;
        int r = 8;

        List<(float, int)> notes = new List<(float, int)>() {
            (q, l),
            (q, l),
            (q, l),
            (q, u),
            (q, l),
            (q, l),
            (q, l),
            (q, d),


            (q, r),
            (q, r),
            (q, r),
            (q, u),
            (q, r),
            (q, r),
            (q, r),
            (q, d),


            (q, l),
            (q, u),
            (q, d),
            (q, r),
            (q, d),
            (q, u),
            (q, l),
            (q, r),

            (h, r|l),
            (h, r|l),
            (h, r|l),
        };

        return notes;
    }

    List<(float, int)> SheetMusic3() {
        float h = HALF_NOTE;
        float q = QUARTER_NOTE;
        float e = EIGHTH_NOTE;
        float s = SIXTEENTH_NOTE;

        // use bit masks to assign a note to multiple staves 
        int l = 1;
        int d = 2;
        int u = 4;
        int r = 8;

        List<(float, int)> notes = new List<(float, int)>() {
            (q, u),
            (e, u),
            (e, u),
            (q, u),
            (q, u),

            (q, d),
            (e, d),
            (e, d),
            (q, d),


            (q, l),
            (e, r),
            (e, r),
            (q, l),
            (q, r),
            (h, l|r),
            (h, l|r),


            (q, l|u),
            (q, l),
            (q, d),
            (q, u),
            (q, u|r),
            (q, r),
            (q, d),
            (q, u),

            (h, l|r),
            (q, l|d),
            (q, l|d),
            (h, l|r),
        };

        return notes;
    }

    List<(float, int)> SheetMusic4() {
        return SheetMusic1();
    }

    List<(float, int)> SheetMusic5() {
        float h = HALF_NOTE;
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
            (h, s1|s4),
            (e, s1),
            (e, s2),
            (e, s3),
            (e, s4),
            (e, s3),
            (e, s2),
            (e, s1),
            (e, s2),
            (e, s1),
            (e, s3),
            (e, s1),
            (e, s4),
            (e, s1),
            (e, s4),
            (e, s1),
            (e, s4),
            (q, s2|s3),
            (q, s2|s3),
            (s, s3),
            (s, s2),
            (s, s3),
            (s, s2),
            (s, s3),
            (s, s2),
            (s, s3),
            (s, s2),
            (q, s3|s2),
            (q, s3|s2),
            (h, s3|s2),
        };

        return notes;

    }
}
