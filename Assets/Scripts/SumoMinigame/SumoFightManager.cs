using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SumoFightManager : MonoBehaviour {
    [SerializeField] SumoGuy player;
    [SerializeField] SumoEnemy[] enemyList;
    [SerializeField] Vector3 playerStartPos;
    [SerializeField] Vector3 enemyStartPos;
    [SerializeField] float fieldSize;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Color bgEndColor;
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] TMP_Text topText;
    
    SumoGuy enemyGuy;
    SumoEnemy enemy;

    void Start() {
        Init();
    }

    public void Init() {
        player.Init();

        enemy = ChooseEnemy();
        enemy.Init(this);
        enemyGuy = enemy.GetComponent<SumoGuy>();

        enemy.transform.position = enemyStartPos;
        player.transform.position = playerStartPos;

        screenCurtain.Init();
        screenCurtain.gameObject.SetActive(true);

        player.SetOnDeath(() => {
            GameEnd(enemyGuy, player);
        });

        enemyGuy.SetOnDeath(() => {
            GameEnd(player, enemyGuy);
        });

        StartCoroutine(CheckGameEnd());

        screenCurtain.Show();
        
        StartCoroutine(FightPrep());
    }

    SumoEnemy ChooseEnemy() {
        int i = Mathf.Max(0, SugorokuManager.stateData.curFightLevel - 1);

        var prefab = enemyList[i];
        SumoEnemy e = Instantiate(prefab, transform);
        return e;
    }

    IEnumerator FightPrep() {
        yield return StartCoroutine(screenCurtain.FadeOut());
        yield return new WaitForSeconds(1.0f);
        enemy.StartActionLoop();
    }

    IEnumerator CheckGameEnd() {
        var waiter = new WaitForSeconds(0.25f);
        while(true) {
            if(!InRing(player)) {
                GameEnd(enemyGuy, player);
                break;
            } 
            else if(!InRing(enemyGuy)) {
                GameEnd(player, enemyGuy);
                break;
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

    void GameEnd(SumoGuy winner, SumoGuy loser) {
        background.color = bgEndColor;
        enemy.active = false;
        enemy.StopActionLoop();
        string victor = winner == player ? "PLAYER" : "ENEMY";
        topText.text = $"{victor} WINS";

        StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

    bool InRing(SumoGuy guy) {
        var xpos = guy.transform.position.x;
        return Mathf.Abs(xpos) < (fieldSize - guy.box.bounds.extents.x);
    }

}
