using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumoFightManager : MonoBehaviour
{
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
        float distToPlayer = Mathf.Abs(enemy.transform.position.x - player.transform.position.x - player.box.bounds.size.x);
        float distToEdge = Mathf.Abs(enemy.transform.position.x - fieldSize - player.box.bounds.extents.x);
        return (distToPlayer, distToEdge);
    }

    void GameEnd(SumoGuy winner, SumoGuy loser) {
        background.color = bgEndColor;
        enemy.active = false;
        enemy.StopActionLoop();
        Debug.Log($"GAME OVER {winner} WINS");
    }

    bool InRing(SumoGuy guy) {
        var xpos = guy.transform.position.x;
        return Mathf.Abs(xpos) < (fieldSize - guy.box.bounds.extents.x);
    }

}
