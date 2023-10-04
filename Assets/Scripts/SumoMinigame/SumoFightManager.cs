using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SumoFightManager : MonoBehaviour {
    [SerializeField] SumoGuy player;
    [SerializeField] SumoEnemy enemy;
    SumoGuy enemyGuy;

    [Range(1,6)]
    [SerializeField] int difficulty;
    [SerializeField] float fieldSize;
    [SerializeField] SpriteRenderer background;
    [SerializeField] Color bgEndColor;
    
    void Start() {
        Init();
    }

    public void Init() {
        player.Init();
        enemy.Init(this);
        enemyGuy = enemy.GetComponent<SumoGuy>();

        player.SetOnDeath(() => {
            GameEnd(enemyGuy, player);
        });

        enemyGuy.SetOnDeath(() => {
            GameEnd(player, enemyGuy);
        });

        if(SugorokuManager.stateData.usingState) difficulty = SugorokuManager.stateData.curFightLevel;

        StartCoroutine(CheckGameEnd());
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
        Debug.Log($"GAME OVER {winner} WINS");
        StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(5.0f);
        SceneManager.LoadScene("TheBoard");
    }

    bool InRing(SumoGuy guy) {
        var xpos = guy.transform.position.x;
        return Mathf.Abs(xpos) < (fieldSize - guy.box.bounds.extents.x);
    }

}
