using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.Events;

public enum GameState {
    Transitioning,
    RollPhase,
    EventOccuring,
}

public enum MinigameType {
    Chanko,
    Fight,
    Teppo,
}

public struct BoardStateData {
    public bool usingState;
    public PlayerSavedData[] players;
    public int curPlayer;
    public CameraData camData;
    public bool wonMinigame;
    public MinigameType minigame;
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

public struct PlayerData {
    public int id;
    public string name;
    public bool ai;
    public int colorIndex;
    public int curTile;
    public int fightLevel;
    public int chankoLevel;
    public int teppoLevel;
    public int turnSkips;
    public SumoGuy spritePrefab;
    public Player player;

    public PlayerData(int id, string name, bool ai, int colorIndex, SugorokuManager man) {
        this.id = id; this.name = name; this.ai = ai; this.colorIndex = colorIndex;
        curTile = 0; fightLevel = 0; chankoLevel = 0; teppoLevel = 0;
        turnSkips = 0;
        spritePrefab = man.GetSumoSize(0, 0);

        var spr = spritePrefab.GetComponent<SpriteRenderer>();
        player = man.SpawnPlayer(id, colorIndex, spr.sprite);
    }
}

public struct PlayerSavedData {
    public PlayerData data;
    public Vector3 pos;

    public PlayerSavedData(PlayerData pd) {
        data = pd;
        pos = pd.player.transform.position;
    }

    public PlayerData Load(SugorokuManager man) {
        PlayerData pd = new PlayerData(data.id, data.name, data.ai, data.colorIndex, man);
        pd.curTile = data.curTile;
        pd.fightLevel = data.fightLevel;
        pd.chankoLevel = data.chankoLevel;
        pd.teppoLevel = data.teppoLevel;
        pd.turnSkips = data.turnSkips;
        pd.player.transform.position = pos;
        pd.spritePrefab = man.GetSumoSize(pd.curTile, pd.colorIndex);

        return pd;
    }
}

public class SugorokuManager : MonoBehaviour {

    public static BoardStateData stateData;

    [SerializeField] Tile[] tiles;
    [SerializeField] EventPopup popup;
    [SerializeField] TMP_Text infoText;
    [SerializeField] UIFadeable infoTextContainer; 
    [SerializeField] float popupDelay;
    [SerializeField] float cameraZoomPadding;
    [SerializeField] float cameraZoomDurationSEC;
    [SerializeField] AnimationCurve cameraZoomCurve;
	[SerializeField] float cameraPanDurationSEC;
	[SerializeField] AnimationCurve cameraPanCurve;
    [SerializeField] Dice minigameDice;
	[SerializeField] Dice dice;
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] private TileHighlight tileHighlight;
    [SerializeField] ClickCollider clickOverlay;
    [SerializeField] Player playerTokenPrefab;
    [SerializeField] float AIMinigameWinRate;
    [SerializeField] SumoGuy[] sumoSizePrefabs;
    [SerializeField] SumoGuy[] sumoColorPrefabs;
    [SerializeField] float playerTileMovementTimeGap;
    [SerializeField] TermDictionary dictionary;
    [SerializeField] int startTile;
    [SerializeField] WinLoseText winLoseText;
    [SerializeField] UIFadeable rollTextContainer;
    [SerializeField] BoardInstructions instructions;

    AudioManager audioman;
	Camera cam;
    CameraData defaultCamState;
    int endTile;
    int chosenMinigame;
    bool freeRoamMode;
    bool paused;
    bool instructionsShown;
    int curPlayer;
    int numPlayers;
    PlayerData[] players;
    UnityAction<bool> endHighlight;


    public GameState gameState;

    void Start() {
        Application.targetFrameRate = 60;
        cam = Camera.main;
		popup.Init();
        popup.RegisterOnExit(OnPopupExit);
        defaultCamState = new CameraData(cam);
        EventPopup.extraEventHasEnded = EndExtraEvent;
        audioman = GameObject.FindWithTag("audioman").GetComponent<AudioManager>();

		GameObject menuBgm = GameObject.FindGameObjectWithTag("bgm-multiscene");
        if(menuBgm != null) Destroy(menuBgm);

        // Check if game is restarting or player just finished minigame
        if (PlayerSelectMenu.loadedFromPlayerSelect) {
            stateData.usingState = false;
			PlayerSelectMenu.loadedFromPlayerSelect = false;
        }

		Init();
    }

    public void Init() {
        // For when the Restart button is hit; may remove later, but useful for debugging right now
        StopAllCoroutines();


        numPlayers = PlayerSelectMenu.playerData.Count;
        gameState = GameState.Transitioning;
        chosenMinigame = -1;
        curPlayer = -1;
        freeRoamMode = false;
        paused = false;
        instructionsShown = false;

        foreach (var tile in tiles) { 
            tile.Init(dictionary, OnTileClick);
            endHighlight += tile.showNumberMask;
        }

        dice.Init();
        minigameDice.Init();
        infoTextContainer.Init();
        screenCurtain.Init();
        rollTextContainer.Init();
        instructions.Init();

        rollTextContainer.gameObject.SetActive(true);
        clickOverlay.gameObject.SetActive(false);

        rollTextContainer.Hide();
        winLoseText.Hide();
        instructions.Hide();


        endTile = tiles.Length - 1;

        screenCurtain.gameObject.SetActive(true);
        screenCurtain.Show();

        StartCoroutine(InitSettleState());
    }

    // checks for Init() to make sure that, no matter what state we load from,
    // our starting state for the next roll is the same.
    IEnumerator InitSettleState() {
        if(stateData.usingState) {
            LoadState();
            yield return StartCoroutine(ReturnFromMinigame());
        }
        else {
            StartGameState();
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(screenCurtain.FadeOut());
            yield return new WaitForSeconds(1.0f);
            Tile tile = tiles[0];
            yield return StartCoroutine(TileZoomProcess(tile));
            StartEvent(tile);
		}
    }

    void StartGameState() {
        PlayersInit();

        defaultCamState.Apply(cam);
        gameState = GameState.RollPhase;
        infoTextContainer.Hide();
    }

    public Player SpawnPlayer(int playerIndex, int colorIndex, Sprite s) {
        Player p = Instantiate(playerTokenPrefab);
        p.SetColor(colorIndex);
        p.SetIcon(s);
        return p;
    }

    void PlayersInit() {
        players = new PlayerData[numPlayers];

        for(int i=0; i < numPlayers; i++) {
            var data = PlayerSelectMenu.playerData[i];
            players[i] = new PlayerData(i, data.name, data.isBot, data.colorIndex, this);
            PlayerTeleport(i, startTile);
        }
    }


    IEnumerator ReturnFromMinigame() {
        HidePlayers();
        yield return StartCoroutine(screenCurtain.FadeOut());
        yield return new WaitForSeconds(1.0f);

        PlayerData pd = players[curPlayer];

        if(!stateData.wonMinigame) {
            switch(stateData.minigame) {
                case MinigameType.Fight:
                    pd.fightLevel--;
                    break;
                case MinigameType.Chanko:
                    pd.chankoLevel--;
                    break;
                case MinigameType.Teppo:
                    pd.teppoLevel--;
                    break;
            }

            players[curPlayer] = pd;

            // so the backwards move doesn't trigger a minigame
            chosenMinigame = -1;

            FadeInPlayers();
            gameState = GameState.Transitioning;
            yield return StartCoroutine(CamZoomReset());
            StartCoroutine(Move(curPlayer, -2));
            yield break;
        }

        if(pd.curTile == endTile) {
            // minigame happens before event for the last tile
            Tile tile = tiles[pd.curTile];
            yield return StartCoroutine(TileZoomProcess(tile, true, false));
            StartEvent(tile);
        }
        else yield return StartCoroutine(ReturnFromEvent());
    }



    void HidePlayers() {
        foreach(PlayerData pd in players) {
            pd.player.Hide();
        }
    }

    void FadeInPlayers() {
        foreach(PlayerData pd in players) {
            pd.player.FadeIn();
        }
    }

    void FadeOutPlayers() {
        foreach(PlayerData pd in players) {
            pd.player.FadeOut();
        }
    }


    public SumoGuy GetSumoSize(int curTile, int colorIndex) {
        int i = curTile / 9;
        i = Mathf.Min(Mathf.Max(0, i), sumoSizePrefabs.Length - 1);

        // 3rd stage, each player gets a colored mawashi
        if(i == 2) {
            return sumoColorPrefabs[colorIndex];
        }
        else return sumoSizePrefabs[i];
    }

    void UpdateSumoSize(int pi) {
        players[pi].spritePrefab = GetSumoSize(players[pi].curTile, players[pi].colorIndex);
        players[pi].player.SetIcon(players[pi].spritePrefab.GetComponent<SpriteRenderer>().sprite);
    }

    Vector3 GetPlayerPosOnTile(int pi, Tile tile) {
        Player player = players[pi].player;
        SpriteRenderer spr = tile.GetComponent<SpriteRenderer>();

        var pos = tile.transform.position;
        if (numPlayers == 2) {
            pos.x -= spr.bounds.extents.x / 2.0f;
            pos.x += spr.bounds.extents.x * pi;
        }
        else if(numPlayers == 3) {
            float d = spr.bounds.size.x / 3.0f;
            pos.x -= d;
            pos.x += d * pi;
        }
        else if(numPlayers == 4) {
            float dx = spr.bounds.extents.x / 2.0f;
            float dy = spr.bounds.extents.y / 2.0f;

            pos.x -= dx;
            pos.x += dx * 2 * (pi % 2);
            pos.y -= dy;
            pos.y += dy * 2 * (pi / 2);
        }

        return pos;
    }

    IEnumerator PlayerToTile(int pi, int tileIndex, bool stayOnPath = true) {
        Tile tile = tiles[tileIndex];

        if(stayOnPath) {
            int delta = tileIndex > players[pi].curTile ? 1 : -1;
            while(players[pi].curTile != tileIndex) {
                players[pi].curTile += delta;
                var pos = GetPlayerPosOnTile(pi, tiles[players[pi].curTile]);
                yield return StartCoroutine(players[pi].player.MoveTo(pos));
                audioman.Play("tap");
                yield return new WaitForSeconds(playerTileMovementTimeGap);
            }
        }
        else {
            var pos = GetPlayerPosOnTile(pi, tile);
            yield return StartCoroutine(players[pi].player.MoveTo(pos));
        }
    }

    void PlayerTeleport(int pi, int tileIndex) {
        players[pi].curTile = tileIndex;
        players[pi].player.transform.position = GetPlayerPosOnTile(pi, tiles[tileIndex]);
    }

    public void OnOverlayClick() {
        if(gameState != GameState.RollPhase) return;

        clickOverlay.gameObject.SetActive(false);
        
        if(!instructionsShown) {
            // the overlay was clicked to hide instructions.
            StartCoroutine(HideInstructions());
        }
        else {
            // the overlay was clicked to roll the dice.
            StartCoroutine(rollTextContainer.FadeOut());
            StartCoroutine(RollDice());
        }
    }

    IEnumerator RollDice() {
        gameState = GameState.Transitioning;

        int moves = -1;
        chosenMinigame = -1;


        dice.Roll((int x) => {
            moves = x;
        });

        minigameDice.Roll((int x) => {
            chosenMinigame = x;
        });

        yield return new WaitUntil(() => {
            return moves != -1 && chosenMinigame != -1;
        });

        StartCoroutine(Move(curPlayer, moves));

    }

    IEnumerator Move(int pi, int numMoves) {
        if(numMoves == 0) {
            StartCoroutine(ReturnFromEvent());
            yield break;
        }
        //ShowRollText($"You rolled a {numMoves}");

        int nextTileIndex = Mathf.Min(players[pi].curTile + numMoves, tiles.Length - 1);
        nextTileIndex = Mathf.Max(0, nextTileIndex);
        yield return StartCoroutine(PlayerToTile(pi, nextTileIndex));

        HideRollText();

        dice.Hide();
        minigameDice.Hide();

        UpdateSumoSize(pi);

        // curTile should be updated after the above coroutine
        Tile tile = tiles[players[pi].curTile];

        if(players[pi].curTile == endTile) {
            // zoom in with no offset and start the final fight
            yield return StartCoroutine(TileZoomProcess(tile, false));
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(StartMinigame(MinigameType.Fight));
        }
        else {
            yield return StartCoroutine(TileZoomProcess(tile));
            StartEvent(tile);
        }

    }

    IEnumerator TileZoomProcess(Tile tile, bool offset = true, bool sound = true) {
        StartCoroutine(infoTextContainer.FadeOut());

        if(sound) audioman.Play("zoom");
        float tileZoom = tile.IsPortrait ? 0.8f : 0.5f;
        yield return StartCoroutine(CamZoomTile(tile, tileZoom));

        // save this cam position to return to after the minigame ends
        stateData.camData = new CameraData(cam);

		// Fade out the game pieces on the current tile
        if (gameState != GameState.EventOccuring) FadeOutPlayers();

        // Offset the camera to allow for the pop-up
        if(offset) yield return StartCoroutine(CamPanTilePercent(tile, 50));

		// Wait for some delay before introducing a pop-up done outside of this function
		yield return new WaitForSeconds(popupDelay);
    }

    void StartEvent(Tile tile) {
        gameState = GameState.EventOccuring;
		tile.Event(this, TileContentType.Narrative, tileHighlight);
    }

    bool HasBonusMove(int pi) {
        if(pi == -1) return false;

        int curTile = players[pi].curTile;
        return (curTile == 10 || curTile == 17 || curTile == 19 || curTile == 21);
    }

    void CheckBonusStall(int pi) {
        if(pi == -1) return;
        if(players[pi].curTile == 14) players[pi].turnSkips++;
    }

    // returns whether or not the bonus event will move the player to a different tile
    void BonusTileMove(int pi) {
        if(pi == -1) return;

        switch(players[pi].curTile) {
            case 10:
                int toStart = -1 * players[pi].curTile;
                StartCoroutine(Move(pi, toStart));
                break;
            case 17:
                StartCoroutine(Move(pi, -2));
                break;
            case 19:
                StartCoroutine(Move(pi, 3));
                break;
            case 21:
                StartCoroutine(Move(pi, -3));
                break;
        }
    }

    public void OnTileClick(Tile tile) {
        if(paused) return;

        Debug.Log("TILE CLICKED");
        if(freeRoamMode && gameState == GameState.RollPhase) {
            StartCoroutine(FreeRoamTileClick(tile));
        }
        else if(gameState == GameState.EventOccuring) StartExtraEvent(tile);
    }

    IEnumerator FreeRoamTileClick(Tile tile) {
        DisableAllTileClicks();
        yield return StartCoroutine(TileZoomProcess(tile));
        StartEvent(tile);
    }

    public void StartExtraEvent(Tile tile) {
		// Notes: gameState == GameState.EventOccuring
		StartCoroutine(ProcessExtraEvent(tile, false));
	}

	public void EndExtraEvent(Tile tile) {
		StartCoroutine(ProcessExtraEvent(tile, true));
	}

	IEnumerator ProcessExtraEvent(Tile tile, bool exiting) {
        if (exiting) {
            yield return StartCoroutine(TileZoomProcess(tile, true, false));
            popup.Show(tile);
        }
        else {
            popup.Hide();
            yield return StartCoroutine(CamPanTilePercent(tile, -50));
            yield return StartCoroutine(CamZoomTile(tile));
			tile.Event(this, TileContentType.Extra, tileHighlight);
		}
    }

    public void ShowExtraPopup(in string text) {
        popup.BeginExtraPopup(text);
    }

	void ShowRollText(string text) {
        infoText.text = text;
        infoText.gameObject.SetActive(true);
    }

    void HideRollText() {
        infoText.gameObject.SetActive(false);
    }

    public Vector3 GetScreenPosition(in Vector3 worldPosition) {
        return cam.WorldToScreenPoint(worldPosition);
    }
    
	IEnumerator CamZoomTile(Tile tile, float scale = 1.0f) {

        Vector3 tileExtentsSize = tile.GetComponent<SpriteRenderer>().bounds.extents;
		bool isSideways = tile.orientation == Orientation.Right || tile.orientation == Orientation.Left;
        float tileRatio = isSideways ? tileExtentsSize.y / tileExtentsSize.x : tileExtentsSize.x / tileExtentsSize.y;

        // Figure out the new view
        float newCamSize = isSideways ? tileExtentsSize.x : tileExtentsSize.y;
		if (cam.aspect < tileRatio) {
			newCamSize *= (tileRatio / cam.aspect);
		}
        newCamSize = (newCamSize / scale) + cameraZoomPadding; // If the scale increases, the zoom increases

        // Figure out the new position
		Vector3 newCamPos = tile.transform.position; // Set the camera position onto the center of the tile

        // Figure out the new rotation
		Quaternion newCamRotation;
        switch (tile.orientation) { // Rotate the camera so that the image on the tile is upright
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
        audioman.Play("zoom");
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
            // rotate the players to align with the camera
            foreach(var pd in players) {
                pd.player.transform.localRotation = rotQ;
            }

            yield return null;

            timePassed += Time.deltaTime;
        }

        cam.transform.position = endCamPos;
        cam.orthographicSize = endCamSize;
        cam.transform.localRotation = endCamRotation;

    }


    // Offsets the tile to some percentage of the camera's view
	IEnumerator CamPanTilePercent(Tile tile, int percent) {
		// Get the tile's aspect ratio to determine where to pan the camera
        // Orientation.* = edge : Up = bottom, Down = top, Right = left, Left = right
		bool isSideways = tile.orientation == Orientation.Right || tile.orientation == Orientation.Left;

		Vector3 offset;
        float oneFourthCamHeight = cam.orthographicSize * (percent / 100.0f);
        float oneFourthCamWidth = oneFourthCamHeight * cam.aspect;
		if (!tile.IsPortrait) {
            // Landscape
            offset = -cam.transform.up;
            offset *= isSideways ? oneFourthCamHeight : oneFourthCamHeight;
		} 
        else {
            // Portrait
            offset = -cam.transform.right;
			offset *= isSideways ? oneFourthCamWidth : oneFourthCamWidth;
		} 

		// Begin the camera pan after gathering information
		CameraData cd = new CameraData(cam.transform.position + offset, 1.0f, Quaternion.identity);
		yield return StartCoroutine(CamPan(cd));
	}

	IEnumerator CamPan(CameraData cd) {
		Vector3 startPos = cam.transform.position;
		Vector3 endCamPos = cd.position;
		endCamPos.z = cam.transform.position.z;

		float timePassed = 0.0f;
		while (timePassed <= cameraPanDurationSEC)
		{
			float curveX = timePassed / cameraPanDurationSEC;
			float curveY = cameraPanCurve.Evaluate(curveX);
			cam.transform.position = Vector3.Lerp(startPos, endCamPos, curveY);

			yield return null;

			timePassed += Time.deltaTime;
		}

		cam.transform.position = endCamPos;
    }

    IEnumerator GameEndText(bool won, float waitTime = 1.0f) {
        yield return winLoseText.FadeIn(won);

        if(won) audioman.Play("win");
        else audioman.Play("lose");
        yield return new WaitForSeconds(waitTime);

        yield return winLoseText.FadeOut(won);
    }

    IEnumerator StartMinigame(MinigameType mg) {
            if(players[curPlayer].ai) {
                bool won = Random.value < AIMinigameWinRate;
                stateData.wonMinigame = won;

                yield return StartCoroutine(GameEndText(won));

                StartCoroutine(ReturnFromMinigame());
                yield break;
            }

            // save the state, but don't save the cam position
            // tile zoom cam position was already saved
            // the camera will spawn over the tile on minigame exit instead of the dice
            SaveState(mg, true);
            
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(screenCurtain.FadeIn());
            SceneManager.LoadScene(MinigameSceneName(mg));
    }

    string MinigameSceneName(MinigameType mg) {
        switch(mg) {
            case MinigameType.Fight:
                return "SumoFight";
            case MinigameType.Chanko:
                return "ChankoGame";
            case MinigameType.Teppo:
            default:
                return "RhythmGame";
        }
    }

    void GameEnd(int winnerIndex) {
        StartCoroutine(GameEndHelper(winnerIndex));
    }

    IEnumerator GameEndHelper(int winnerIndex) {
        string name = players[winnerIndex].name;
        name = name.ToUpper();
        winLoseText.SetWinText($"{name}\nWINS");

        yield return StartCoroutine(GameEndText(true, 3.0f));

        yield return StartCoroutine(infoTextContainer.FadeIn());

        ShowRollText("Click on any tile!");

        StartFreeRoamMode();

    }

    // pass in which minigame you'll transition to
    void SaveState(MinigameType mg, bool ignoreCam = false) {
        stateData.wonMinigame = false;
        stateData.minigame = mg;

        stateData.players = new PlayerSavedData[numPlayers];
        for(int i=0; i < numPlayers; i++) {
            stateData.players[i] = new PlayerSavedData(players[i]);
        }

        stateData.curPlayer = curPlayer;

        if(!ignoreCam) stateData.camData = new CameraData(cam);

        stateData.usingState = true;
    }

    void LoadState() {
        if(!stateData.usingState) Debug.Log("WARNING: Loading from a state that is not intended to be used");

        instructionsShown = true;

        players = new PlayerData[numPlayers];
        for(int i=0; i < numPlayers; i++) {
            players[i] = stateData.players[i].Load(this);
            UpdateSumoSize(i);
        }

        curPlayer = stateData.curPlayer;
        stateData.camData.Apply(cam);

        foreach(PlayerData pd in players) {
            pd.player.transform.localRotation = stateData.camData.rotation;
        }

        // a state should only ever be loaded from once, so disable the flag
        stateData.usingState = false;
    }

    public void ShowPopup(in string text, in Vector2 scale, in Vector2 offsetScale, in Tile currTile) {
		popup.SetText(text);
		popup.SetScale(scale);
		popup.ApplyOffsetScale(offsetScale);
        popup.Show(currTile);
        tileHighlight.StartHightlight();
    }

    public void OnPopupExit() {
        StartCoroutine(OnEventEnd());
        tileHighlight.EndHighlight();
        endHighlight?.Invoke(false);
	}

    IEnumerator ReturnFromEvent() {
        gameState = GameState.Transitioning;
        FadeInPlayers();

        if(HasBonusMove(curPlayer)) {
            yield return StartCoroutine(CamZoomReset());

            BonusTileMove(curPlayer);
            yield break;
        }

        dice.DiceReset();
        minigameDice.DiceReset();

        yield return StartCoroutine(CamZoomReset());


        gameState = GameState.RollPhase;

        if(!instructionsShown) StartCoroutine(ShowInstructions());
        else if(freeRoamMode) {
            StartCoroutine(infoTextContainer.FadeIn());
            EnableAllTileClicks();
        }
        else if(curPlayer >= 0 && players[curPlayer].curTile == endTile) GameEnd(curPlayer);
        else StartCoroutine(StartNextTurn());
    }

    IEnumerator ShowInstructions() {
        yield return StartCoroutine(instructions.FadeIn());
        clickOverlay.gameObject.SetActive(true);
    }

    IEnumerator HideInstructions() {
        instructionsShown = true;
        yield return StartCoroutine(instructions.FadeOut());
        StartCoroutine(StartNextTurn());
    }

    IEnumerator StartNextTurn() {

        curPlayer = (curPlayer + 1) % numPlayers;

        while (players[curPlayer].turnSkips > 0) {
            ShowRollText($"{players[curPlayer].name} Turn Skipped!");
            players[curPlayer].turnSkips--;
            curPlayer = (curPlayer + 1) % numPlayers;

            yield return new WaitForSeconds(1.0f);
        }

        ShowRollText($"{players[curPlayer].name}'s Turn");
        audioman.Play("start-turn");
        yield return StartCoroutine(infoTextContainer.FadeIn());

        if(players[curPlayer].ai) {
            StartCoroutine(AITakeTurn());
        }
        else {
            clickOverlay.gameObject.SetActive(true);
            StartCoroutine(rollTextContainer.FadeIn());
        }
    }

    IEnumerator AITakeTurn() {
        yield return new WaitForSeconds(1.0f);
        OnOverlayClick();
    }

    IEnumerator OnEventEnd() {
        dice.Show();
        minigameDice.Show();

        CheckBonusStall(curPlayer);

        if(curPlayer == -1 || players[curPlayer].curTile == endTile || 
           HasBonusMove(curPlayer) || chosenMinigame == -1) {

            StartCoroutine(ReturnFromEvent());
            yield break;
        }

        // switching to a minigame based on dice roll

        yield return ZoomOnMinigameDice();
        yield return new WaitForSeconds(1.0f);

        MinigameType mg = DiceNumToMinigame(chosenMinigame);
        PlayerData pd = players[curPlayer];

        switch(mg) {
            case MinigameType.Fight:
                pd.fightLevel++;
                break;

            case MinigameType.Chanko:
                pd.chankoLevel++;
                break;

            case MinigameType.Teppo:
                pd.teppoLevel++;
                break;

            default:
                Debug.Log($"SOMETHING WENT BAD IN MINIGAME SELECT: {chosenMinigame}");
                break;
        }

        players[curPlayer] = pd;

        yield return StartCoroutine(StartMinigame(mg));
    }

    MinigameType DiceNumToMinigame(int n) {
        switch(n) {
            case 1:
            case 6:
                return MinigameType.Fight;
            case 3:
            case 4:
                return MinigameType.Chanko;
            case 2:
            case 5:
            default:
                return MinigameType.Teppo;
        }
    }

    IEnumerator ZoomOnMinigameDice() {
        var pos = minigameDice.transform.position;
        float camSize = minigameDice.GetComponent<BoxCollider>().bounds.size.x;
        var rot = minigameDice.GetUpDirRotation();
        CameraData cd = new CameraData(pos, camSize, rot);

        audioman.Play("zoom");
        yield return StartCoroutine(CamZoom(cd));
    }




    void StartFreeRoamMode() {
        freeRoamMode = true;
        EnableAllTileClicks();
    }

    void EnableAllTileClicks() {
        foreach(Tile tile in tiles) {
            tile.SetIsClickable(true);
        }
    }

    void DisableAllTileClicks() {
        foreach(Tile tile in tiles) {
            tile.SetIsClickable(false);
        }
    }
    
}
