using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] float popupDelay;
    [SerializeField] float cameraZoomPadding;
    [SerializeField] float cameraZoomDurationSEC;
    [SerializeField] AnimationCurve cameraZoomCurve;
    [SerializeField] Dice dice;
    [SerializeField] int tilesBetweenFights;

    Camera cam;
    int curTile;
    int endTile;
    CameraData defaultCamState;
    TermDictionary dictionary;
    int tilesTillNextFight;
    int curFightLevel;

    public GameState gameState;

    void Start() {
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
        hideRollText();

        dice.Init();

        endTile = tiles.Length - 1;

        StartCoroutine(InitSettleState());
    }

    // checks for Init() to make sure that, no matter what state we load from,
    // our starting state for the next roll is the same.
    IEnumerator InitSettleState() {
        if(stateData.usingState) {
            LoadState();
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(CamZoomReset());
        }
        else {
            tilesTillNextFight = tilesBetweenFights;
            curFightLevel = 1;
            PlayerTeleport(0);
            defaultCamState.Apply(cam);
        }

        // event checks come after the camera is in its full-screen default state

        gameState = GameState.RollPhase;

        if(curTile == endTile) GameEnd();
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

    public void OnRollButton() {
        if(gameState != GameState.RollPhase) return;

        gameState = GameState.Transitioning;

        dice.Roll((int x) => {
            StartCoroutine(Move(x));
        });
    }

    IEnumerator Move(int numMoves) {
        showRollText($"You rolled a {numMoves}");

        int nextTileIndex = Mathf.Min(curTile + numMoves, tiles.Length - 1);
        yield return StartCoroutine(PlayerToTile(nextTileIndex));

        hideRollText();

        dice.Reset();
        tilesTillNextFight -= numMoves;
        // curTile should be updated after the above coroutine
        Tile tile = tiles[curTile];

        yield return StartCoroutine(CamZoomTile(tile));
        yield return new WaitForSeconds(popupDelay);

        gameState = GameState.EventOccuring;
        tile.Event(this);
    }

    void showRollText(string text) {
        rollText.text = text;
        rollText.gameObject.SetActive(true);
    }

    void hideRollText() {
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

    void CheckForFight() {
        if(tilesTillNextFight <= 0) {
            tilesTillNextFight += tilesBetweenFights;
            SaveState();
            curFightLevel++;
            SceneManager.LoadScene("SumoFight");
        }
    }

    void GameEnd() {
        showRollText("You finished the game great job fool");
    }

    void SaveState() {
        stateData.curTile = curTile;
        stateData.curFightLevel = curFightLevel;
        stateData.playerPos = player.transform.position;
        stateData.tilesTillNextFight = tilesTillNextFight;
        stateData.camData = new CameraData(cam);
        stateData.usingState = true;
    }

    void LoadState() {
        if(!stateData.usingState) Debug.Log("WARNING: Loading from a state that is not intended to be used");
        curTile = stateData.curTile;
        curFightLevel = stateData.curFightLevel;
        player.transform.position = stateData.playerPos;
        player.transform.localRotation = stateData.camData.rotation;
        tilesTillNextFight = stateData.tilesTillNextFight;
        stateData.camData.Apply(cam);


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

    IEnumerator OnEventEnd() {
        CheckForFight();

        // if we make it here, a fight did not occur

        gameState = GameState.Transitioning;
        yield return StartCoroutine(CamZoomReset());

        if(curTile == endTile) GameEnd();

        gameState = GameState.RollPhase;
    }

}
