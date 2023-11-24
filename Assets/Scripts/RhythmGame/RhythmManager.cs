using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;


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
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] float hitPercentToWin;
    [SerializeField] UIFadeable instructionsPanel;
    [SerializeField] UIFadeable infoTextContainer;
    [SerializeField] TMP_Text infoText;
    [SerializeField] Vector3 playerSpritePos;
    [SerializeField] Vector3 playerSpriteScale;
    [SerializeField] SumoGuy defaultPlayerPrefab;
    [SerializeField] int defaultLevel;
    [SerializeField] WinLoseText winLoseText;
    [SerializeField] SpriteRenderer backgroundImg;
    [SerializeField] Pole pole;
    [SerializeField] float playerPushDist;
    [SerializeField] float playerPushDurationSEC;
    [SerializeField] AnimationCurve playerPushCurve;

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

    AudioManager audioman;
    Coroutine pushRoutine;
    StaffMultiplexer smp;
    Animator playerSprite;

    void Start() {
        audioman = GameObject.FindWithTag("audioman").GetComponent<AudioManager>();
        Init();
    }

    public void Init() {
        bool fatPlayerSprite = SpawnPlayerSprite();

        pole.Init(playerSprite.GetComponent<SpriteRenderer>(), fatPlayerSprite);

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

        winLoseText.Hide();

        // when the screen size changes, scale the camera so that it always 
        // has the whole background in view
        Camera cam = Camera.main;
        float camSize = backgroundImg.bounds.extents.x / cam.aspect;
        cam.orthographicSize = camSize;

        staff1.Init(KeyCode.LeftArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff2.Init(KeyCode.DownArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff3.Init(KeyCode.UpArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        staff4.Init(KeyCode.RightArrow, reactionTimeBEATS, tempoBPS, 
                hitToleranceBEATS, missToleranceBEATS);

        UnityAction<bool, bool, KeyCode> onHitAttempt = OnHitAttempt;

        staff1.RegisterOnHitAttempt(onHitAttempt);
        staff2.RegisterOnHitAttempt(onHitAttempt);
        staff3.RegisterOnHitAttempt(onHitAttempt);
        staff4.RegisterOnHitAttempt(onHitAttempt);

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

    bool SpawnPlayerSprite() {
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

        playerSprite = guy.GetComponent<Animator>();
        return guy.largestSize;
    }

    void PlayerPush() {
        if(pushRoutine != null) StopCoroutine(pushRoutine);
        StartCoroutine(PlayerPushHelper());
    }

    IEnumerator PlayerPushHelper() {
        var player = playerSprite.transform;
        player.localPosition = playerSpritePos;

        var startPos = playerSpritePos;
        var endPos = startPos;
        endPos.x += playerPushDist;


        playerSprite.SetBool("Pushing", true);

        float timePassed = 0.0f;
        while(timePassed < playerPushDurationSEC) {
            timePassed += Time.deltaTime;

            float curveX = timePassed / playerPushDurationSEC;
            float curveY = playerPushCurve.Evaluate(curveX);
            var pos = Vector3.Lerp(startPos, endPos, curveY);
            player.localPosition = pos;

            yield return null;
        }

        player.localPosition = playerSpritePos;
        playerSprite.SetBool("Pushing", false);
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
        else return defaultLevel;
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
        audioman.Play("game-start");
        StartCoroutine(Game());
    }

    IEnumerator GameEndText(bool won) {
        yield return winLoseText.FadeIn(won);

        if(won) audioman.Play("win");
        else audioman.Play("lose");
        yield return new WaitForSeconds(2.0f);

        yield return winLoseText.FadeOut(won);
    }

    IEnumerator Game() {
        yield return StartCoroutine(smp.Read());

        (int notesHit, int totalNotes) = smp.GetHitRate();

        bool win = (notesHit / (float)totalNotes) >= hitPercentToWin;

        yield return new WaitForSeconds(3.0f);

        yield return StartCoroutine(GameEndText(win));

        SugorokuManager.stateData.wonMinigame = win;

        yield return StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

    // an attempt can both not hit and not miss if there are no notes nearby
    void OnHitAttempt(bool hit, bool miss, KeyCode key) {
        if(hit || !miss) {
            PlayerPush();

            switch(key) {
                case KeyCode.UpArrow:
                    audioman.Play("up");
                    break;
                case KeyCode.DownArrow:
                    audioman.Play("down");
                    break;
                case KeyCode.LeftArrow:
                    audioman.Play("left");
                    break;
                case KeyCode.RightArrow:
                    audioman.Play("right");
                    break;
            }

            if(hit) pole.Shake();
        }
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
            (h, d),
            (h, r),
            (q, d),
            (q, d),
            (h, r),

            (h, u),
            (h, l),
            (q, u),
            (q, u),
            (h, l),

            (q, u),
            (q, d),
            (q, l),
            (q, r),

            (q, d),
            (q, u),
            (q, r),
            (q, l),

            (h, u),
            (h, r),
            (q, d),
            (q, d),
            (h, l),
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
            (q, u),
            (q, u),
            (q, u),
            (q, l),
            (q, u),
            (q, u),
            (q, u),
            (q, l),


            (q, d),
            (q, d),
            (q, d),
            (q, r),
            (q, d),
            (q, d),
            (q, d),
            (q, r),


            (q, u),
            (q, r),
            (q, d),
            (q, l),
            (q, u),
            (q, r),
            (q, d),
            (q, l),

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
            (h, d),


            (q, u),
            (e, r),
            (e, r),
            (q, u),
            (q, r),
            (h, u|d),
            (h, u|d),


            (q, u|d),
            (q, l),
            (q, d),
            (q, u),
            (q, u|d),
            (q, r),
            (q, d),
            (q, u),

            (h, u|d),
            (q, l|r),
            (q, l|r),
            (h, u|d),
        };

        return notes;
    }

    List<(float, int)> SheetMusic5() {
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
            (e, u),
            (e, d),
            (e, l),
            (e, r),

            (e, u),
            (e, d),
            (e, l),
            (e, r),

            (q, u|d),
            (q, u|d),
            (h, u|d),

            (e, r),
            (s, r),
            (s, r),
            (e, d),
            (e, r),

            (e, l),
            (s, l),
            (s, l),
            (e, u),
            (e, l),

            (e, l|r),
            (e, d),
            (e, l|r),
            (e, d),
            (h, l|r),

            (e, u),
            (q, l),
            (q, l),
            (e, d),
            (q, r),
            (q, r),

            (e, d),
            (e, l),
            (e, d),
            (e, r),
            (h, l|r),

            (q, l|r),
            (q, u|d),
            (q, l|r),

            (s, u),
            (s, d),
            (s, l),
            (s, r),

            (e, l|r),
            (e, d),
            (e, l|r),
            (e, d),
            (h, l|r),
        };

        return notes;
    }

    List<(float, int)> SheetMusic4() {
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
            (q, d),
            (q, l|r),
            (e, d),
            (e, d),
            (q, d),
            (e, l|r),
            (q, d),
            (e, d),
            (h, l|r),

            (e, u),
            (e, r),
            (e, l),
            (e, d),
            (e, d),
            (e, l),
            (e, r),
            (e, u),
            (e, l),
            (e, d),
            (e, l),
            (e, d),
            (e, r),
            (e, d),
            (e, r),
            (e, u),
            (q, u|d),
            (q, u|d),
            (e, u|d),
            (q, u|d),
            (e, l|r),
            (e, l|r),
            (e, u|d),
            (e, l|r),
            (e, u|d),
            (h, l|r),
        };

        return notes;

    }
}
