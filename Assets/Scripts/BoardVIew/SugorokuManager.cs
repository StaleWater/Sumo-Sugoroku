using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState {
    Transitioning,
    RollPhase,
    EventOccuring,
}

public struct BoardStateData {
    public bool usingState;
    public int curTile;
    public int tilesTillNextFight;
    public Vector3 playerPos;
    public int curFightLevel;
    public CameraData camData;
    public int riggedTileIndex;
}

public struct CameraData {
    public Vector3 position;
    public float size;
    public Quaternion rotation;

    public CameraData(Vector3 pos, float s, Quaternion rot) {
        position = pos;
        size = s;
        rotation = rot;
    }

    public CameraData(Camera cam) {
        position = cam.transform.position;
        size = cam.orthographicSize;
        rotation = cam.transform.localRotation;
    }

    public void Apply(Camera cam) {
        cam.transform.position = position;
        cam.orthographicSize = size;
        cam.transform.localRotation = rotation;
    }
}

public class SugorokuManager : MonoBehaviour {


    public static BoardStateData stateData;

    [SerializeField] Tile[] tiles;
    [SerializeField] Player player;
    [SerializeField] Vector2Int diceMinMax;
    [SerializeField] EventPopup popup;
    [SerializeField] TMP_Text rollText;
    [SerializeField] UIFadeable rollTextContainer; 
    [SerializeField] Button rollButton;
    [SerializeField] float popupDelay;
    [SerializeField] float cameraZoomPadding;
    [SerializeField] float cameraZoomDurationSEC;
    [SerializeField] AnimationCurve cameraZoomCurve;
    [SerializeField] Dice dice;
    [SerializeField] int tilesBetweenFights;
    [SerializeField] int[] tilesToVisit;
    [SerializeField] UIFadeable screenCurtain; 

    Camera cam;
    int curTile;
    int endTile;
    int riggedTileIndex;
    CameraData defaultCamState;
    TermDictionary dictionary;
    int tilesTillNextFight;
    public int curFightLevel;

    string rollPhaseText = "Roll the dice!";

    public GameState gameState;

    void Start() {
        Application.targetFrameRate = 60;
        cam = Camera.main;
        popup.RegisterOnExit(OnPopupExit);
        defaultCamState = new CameraData(cam);
        Init();
    }

    public void Init() {
        // For when the Restart button is hit; may remove later, but useful for debugging right now
        StopAllCoroutines();

        gameState = GameState.Transitioning;

        dictionary = GetComponent<TermDictionary>();
        dictionary.Init();
        foreach(var tile in tiles) tile.Init(dictionary);
        
        popup.Init();
        dice.Init();
        rollTextContainer.Init();
        screenCurtain.Init();

        endTile = tiles.Length - 1;
        ShowRollText(rollPhaseText);

        screenCurtain.gameObject.SetActive(true);
        screenCurtain.Show();

        StartCoroutine(InitSettleState());
    }

    // checks for Init() to make sure that, no matter what state we load from,
    // our starting state for the next roll is the same.
    IEnumerator InitSettleState() {
        if(stateData.usingState) {
            LoadState();
            player.GetComponent<Fadeable>().Hide();
            yield return StartCoroutine(screenCurtain.FadeOut());
            yield return new WaitForSeconds(1.0f);

            if(curTile == endTile) StartEvent(tiles[curTile]);
            else yield return StartCoroutine(ReturnFromEvent());
        }
        else {
            StartGameState();
            yield return StartCoroutine(screenCurtain.FadeOut());
            yield return new WaitForSeconds(1.0f);
            Tile tile = tiles[curTile];
            yield return StartCoroutine(TileZoomProcess(tile));
            StartEvent(tile);
        }


    }

    void StartGameState() {
            tilesTillNextFight = tilesBetweenFights;
            curFightLevel = 0;
            PlayerTeleport(0);
            defaultCamState.Apply(cam);
            riggedTileIndex = 0;
            rollTextContainer.Show();
            gameState = GameState.RollPhase;
            curTile = 0;
            rollTextContainer.Hide();
    }

    IEnumerator PlayerToTile(int tileIndex, bool stayOnPath = true) {
        Tile tile = tiles[tileIndex];

        if(stayOnPath) {
            int delta = tileIndex > curTile ? 1 : -1;
            while(curTile != tileIndex) {
                curTile += delta;
                yield return StartCoroutine(player.MoveTo(tiles[curTile].transform.position));
            }
        }
        else yield return StartCoroutine(player.MoveTo(tile.transform.position));

    }

    void PlayerTeleport(int tileIndex) {
        curTile = tileIndex;
        player.transform.position = tiles[tileIndex].transform.position;
    }

    int RandomNumMoves() {
        return Random.Range(diceMinMax.x, diceMinMax.y + 1);
    }

    int RiggedNumMoves() {
        int nextTile = tilesToVisit[++riggedTileIndex] - 1;
        int dist = nextTile - curTile;
        if(dist > 6) {
            Debug.Log("TOO BIG STOP");
            return 0;
        }

        return dist;
    }

    public void OnRollButton() {
        if(gameState != GameState.RollPhase) return;

        gameState = GameState.Transitioning;

        int moves = RiggedNumMoves();


        dice.RiggedRoll((int x) => {
            StartCoroutine(Move(x));
        }, moves);
    }

    IEnumerator Move(int numMoves) {
        ShowRollText($"You rolled a {numMoves}");

        int nextTileIndex = Mathf.Min(curTile + numMoves, tiles.Length - 1);
        yield return StartCoroutine(PlayerToTile(nextTileIndex));

        HideRollText();

        dice.Reset();
        tilesTillNextFight -= numMoves;
        // curTile should be updated after the above coroutine
        Tile tile = tiles[curTile];

        yield return StartCoroutine(TileZoomProcess(tile));

        if(curTile == endTile) StartCoroutine(StartFight());
        else StartEvent(tile);
    }

    IEnumerator TileZoomProcess(Tile tile) {
        StartCoroutine(rollTextContainer.FadeOut());
        yield return StartCoroutine(CamZoomTile(tile));
        player.GetComponent<Fadeable>().FadeOut();
        yield return new WaitForSeconds(popupDelay);
    }

   void StartEvent(Tile tile) {
        gameState = GameState.EventOccuring;
        tile.Event(this);
   } 

    void ShowRollText(string text) {
        rollText.text = text;
        rollText.gameObject.SetActive(true);
    }

    void HideRollText() {
        rollText.gameObject.SetActive(false);
    }

    IEnumerator CamZoomTile(Tile tile) {
        float newCamSize = tile.GetComponent<SpriteRenderer>().bounds.extents.y + cameraZoomPadding;
        Vector3 newCamPos = tile.transform.position;

        Quaternion newCamRotation;
        switch (tile.orientation) {
            case Orientation.Up:
                newCamRotation = Quaternion.identity;
                break;
            case Orientation.Right:
                newCamRotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
                break;
            case Orientation.Down:
                newCamRotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                break;
            case Orientation.Left:
                newCamRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
                break;

            default:
                newCamRotation = Quaternion.identity;
                break;
        }

        CameraData cd = new CameraData(newCamPos, newCamSize, newCamRotation);

        yield return StartCoroutine(CamZoom(cd));
    }

    IEnumerator CamZoomReset() {
        yield return StartCoroutine(CamZoom(defaultCamState));
    }

    IEnumerator CamZoom(CameraData cd) {
        Vector3 endCamPos = cd.position;
        float endCamSize = cd.size;
        Quaternion endCamRotation = cd.rotation; 
        endCamPos.z = cam.transform.position.z;

        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;
        Quaternion startRotation = cam.transform.localRotation;
        float timePassed = 0.0f;

        while(timePassed <= cameraZoomDurationSEC) {
            float curveX = timePassed / cameraZoomDurationSEC;
            float curveY = cameraZoomCurve.Evaluate(curveX);
            cam.transform.position = Vector3.Lerp(startPos, endCamPos, curveY);
            cam.orthographicSize = Mathf.Lerp(startSize, endCamSize, curveY);

            var rotQ = Quaternion.Slerp(startRotation, endCamRotation, curveY);
            cam.transform.localRotation = rotQ;
            // rotate the player to align with the camera
            player.transform.localRotation = rotQ;

            yield return null;

            timePassed += Time.deltaTime;
        }

        cam.transform.position = endCamPos;
        cam.orthographicSize = endCamSize;
        cam.transform.localRotation = endCamRotation;

    }

    bool CheckForFight() {
        if(tilesTillNextFight <= 0) {
            tilesTillNextFight += tilesBetweenFights;
            StartCoroutine(StartFight());
            return true;
        }
        return false;
    }

    IEnumerator StartFight() {
            curFightLevel++;
            SaveState();
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(screenCurtain.FadeIn());
            SceneManager.LoadScene("SumoFight");
    }

    void GameEnd() {
        ShowRollText("Game Finished!");
        rollButton.interactable = false;
    }

    void SaveState() {
        stateData.curTile = curTile;
        stateData.curFightLevel = curFightLevel;
        stateData.playerPos = player.transform.position;
        stateData.tilesTillNextFight = tilesTillNextFight;
        stateData.camData = new CameraData(cam);
        stateData.usingState = true;
        stateData.riggedTileIndex = riggedTileIndex;
    }

    void LoadState() {
        if(!stateData.usingState) Debug.Log("WARNING: Loading from a state that is not intended to be used");
        curTile = stateData.curTile;
        curFightLevel = stateData.curFightLevel;
        player.transform.position = stateData.playerPos;
        player.transform.localRotation = stateData.camData.rotation;
        tilesTillNextFight = stateData.tilesTillNextFight;
        stateData.camData.Apply(cam);
        riggedTileIndex = stateData.riggedTileIndex;


        // a state should only ever be loaded from once, so disable the flag
        stateData.usingState = false;
    }

    public void ShowPopup(string text) {
        popup.SetText(text);
        popup.Show();
    }

    public void OnPopupExit() {
        StartCoroutine(OnEventEnd());
    }

    IEnumerator ReturnFromEvent() {
        gameState = GameState.Transitioning;
        player.GetComponent<Fadeable>().FadeIn();
        yield return StartCoroutine(CamZoomReset());

        gameState = GameState.RollPhase;
        ShowRollText(rollPhaseText);
        StartCoroutine(rollTextContainer.FadeIn());

        if(curTile == endTile) GameEnd();
    }

    IEnumerator OnEventEnd() {
        if(!CheckForFight()) {
            yield return StartCoroutine(ReturnFromEvent());
        }
    }

    public void BackToMenu() {
        StartCoroutine(BackToMenuHelper());
    }

    IEnumerator BackToMenuHelper() {
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("MainMenu");
    }

}
