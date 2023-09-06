using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SugorokuManager : MonoBehaviour {

    [SerializeField] Tile[] tiles;
    [SerializeField] Player player;
    [SerializeField] Vector2Int diceMinMax;
    [SerializeField] EventPopup popup;
    [SerializeField] TMP_Text rollText;
    [SerializeField] float popupDelay;
    [SerializeField] float cameraZoomPadding;
    [SerializeField] float cameraZoomDurationSEC;
    [SerializeField] AnimationCurve cameraZoomCurve;

    Camera cam;
    int curTile;
    int endTile;
    float defaultCamSize;
    Vector3 defaultCamPos;

    void Start() {
        cam = Camera.main;
        popup.RegisterOnExit(OnPopupExit);
        defaultCamSize = cam.orthographicSize;
        defaultCamPos = cam.transform.position;
        Init();
    }

    public void Init() {
        popup.Init();
        hideRollText();
        endTile = tiles.Length - 1;
        PlayerTeleport(0);
        StartCoroutine(CamZoomReset());
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
        StartCoroutine(RollAndMove());
    }

    IEnumerator RollAndMove() {

        // dice animation here

        int numMoves = RandomNumMoves();
        showRollText($"You rolled a {numMoves}");

        int nextTileIndex = Mathf.Min(curTile + numMoves, tiles.Length - 1);
        yield return StartCoroutine(PlayerToTile(nextTileIndex));

        hideRollText();

        // curTile should be updated after the above coroutine
        Tile tile = tiles[curTile];
        yield return StartCoroutine(CamZoomTile(tile));
        yield return new WaitForSeconds(popupDelay);
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

        yield return StartCoroutine(CamZoom(newCamPos, newCamSize, newCamRotation));
    }

    IEnumerator CamZoomReset() {
        yield return StartCoroutine(CamZoom(defaultCamPos, defaultCamSize, Quaternion.identity));
    }

    IEnumerator CamZoom(Vector3 endCamPos, float endCamSize, Quaternion endCamRotation) {
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
            cam.transform.localRotation = Quaternion.Slerp(startRotation, endCamRotation, curveY);

            yield return null;

            timePassed += Time.deltaTime;
        }

        cam.transform.position = endCamPos;
        cam.orthographicSize = endCamSize;
        cam.transform.localRotation = endCamRotation;

    }

    void GameEnd() {
        showRollText("You finished the game great job fool");
    }

    public void ShowPopup(string text) {
        popup.SetText(text);
        popup.Show();
    }

    public void OnPopupExit() {
        StartCoroutine(OnEventEnd());
    }

    IEnumerator OnEventEnd() {
        yield return StartCoroutine(CamZoomReset());
        
        if(curTile == endTile) GameEnd();
    }

}
