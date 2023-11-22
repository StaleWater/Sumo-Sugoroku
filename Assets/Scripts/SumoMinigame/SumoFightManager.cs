using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SumoFightManager : MonoBehaviour {
    [SerializeField] SumoGuy defaultPlayerPrefab;
    [SerializeField] SumoEnemy[] enemyList;
    [SerializeField] Vector3 playerStartPos;
    [SerializeField] Vector3 enemyStartPos;
    [SerializeField] float fieldSize;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Color bgEndColor;
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] UIFadeable instructionsPanel;
    [SerializeField] UIFadeable infoTextContainer;
    [SerializeField] TMP_Text infoText;
    [SerializeField] int defaultLevel;
    [SerializeField] WinLoseText winLoseText;
    
    AudioManager audioman;
    SumoGuy enemyGuy;
    SumoEnemy enemy;
    SumoGuy player;
    bool ended;

    string helpText = "Move:  A-D or Arrow Keys      Push: Space	     Block: Shift";
    string startText = "- Press Space to Start -";

    void Start() {
        audioman = GameObject.FindWithTag("audioman").GetComponent<AudioManager>();
        Init();
    }

    public void Init() {
        ended = false;

        player = SpawnPlayer();
        player.Init();

        enemy = ChooseEnemy();
        enemy.Init(this);
        enemyGuy = enemy.GetComponent<SumoGuy>();

        enemy.transform.position = enemyStartPos;
        player.transform.position = playerStartPos;

        screenCurtain.Init();
        screenCurtain.gameObject.SetActive(true);

        instructionsPanel.Init();
        instructionsPanel.gameObject.SetActive(true);
        instructionsPanel.Hide();

        winLoseText.Hide();

        infoText.text = startText;

        player.SetOnDeath(() => {
            GameEnd(enemyGuy, player);
        });

        enemyGuy.SetOnDeath(() => {
            GameEnd(player, enemyGuy);
        });

        player.GetComponent<PlayerController>().enabled = false;

        if(Level() == 1) {
            instructionsPanel.Show();
        }
        else StartCoroutine(WaitToStart());

        screenCurtain.Show();
        StartCoroutine(screenCurtain.FadeOut());
    }

    int Level() {
        if(SugorokuManager.stateData.usingState) {
            return SugorokuManager.stateData.players[SugorokuManager.stateData.curPlayer].data.fightLevel;
        }
        else return defaultLevel;
    } 

    SumoGuy SpawnPlayer() {
        SumoGuy prefab = defaultPlayerPrefab;
        if(SugorokuManager.stateData.usingState) {
            int pi = SugorokuManager.stateData.curPlayer;
            prefab = SugorokuManager.stateData.players[pi].data.spritePrefab;
        }

        return Instantiate(prefab, transform);
    }

    SumoEnemy ChooseEnemy() {

        int difficulty = Level();
        int i = Mathf.Min(Mathf.Max(0, difficulty-1), 4);

        var prefab = enemyList[i];
        SumoEnemy e = Instantiate(prefab, transform);
        return e;
    }

    IEnumerator CheckGameEnd() {
        var waiter = new WaitForSeconds(0.25f);
        while(!ended) {
            if(!InRing(player)) {
                GameEnd(enemyGuy, player);
                yield break;
            } 
            else if(!InRing(enemyGuy)) {
                GameEnd(player, enemyGuy);
                yield break;
            }
            yield return waiter;
        }
    }

    public (float, float) GetEnemyInputData() {
        float pfr = Mathf.Abs(enemyGuy.box.bounds.min.x - player.box.bounds.max.x);
        float efr = Mathf.Abs(player.box.bounds.min.x - enemyGuy.box.bounds.max.x);
        float distToPlayer = Mathf.Min(pfr, efr);
        float distToEdge = Mathf.Abs(enemy.transform.position.x - fieldSize - player.box.bounds.extents.x);
        return (distToPlayer, distToEdge);
    }

    IEnumerator GameEndText(bool won) {
        yield return winLoseText.FadeIn(won);

        if(won) audioman.Play("win");
        else audioman.Play("lose");
        yield return new WaitForSeconds(2.0f);

        yield return winLoseText.FadeOut(won);
    }

    void GameEnd(SumoGuy winner, SumoGuy loser) {
        if(ended) return;

        ended = true;
        background.color = bgEndColor;
        enemy.active = false;
        enemy.StopActionLoop();
        winner.alive = false;
        loser.alive = false;

        audioman.Play("match");

        bool won = winner == player;
        SugorokuManager.stateData.wonMinigame = won;

        StartCoroutine(GameEndHelper(won));
    }

    IEnumerator GameEndHelper(bool won) {
        infoTextContainer.FadeOut();

        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(GameEndText(won));

        StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

    bool InRing(SumoGuy guy) {
        var xpos = guy.transform.position.x;
        return Mathf.Abs(xpos) < (fieldSize - guy.box.bounds.extents.x);
    }

    void StartFight() {
        audioman.Play("match");
        StartCoroutine(CheckGameEnd());
        enemy.StartActionLoop();
        player.GetComponent<PlayerController>().enabled = true;

        infoText.text = helpText;
    }

    IEnumerator WaitToStart() {
        yield return StartCoroutine(instructionsPanel.FadeOut());
        instructionsPanel.gameObject.SetActive(false);

        while(!Input.GetKeyDown(KeyCode.Space)) yield return null;
        StartFight();
    }

    public void OnInstructionsClose() {
        StartCoroutine(WaitToStart());
    }

}
