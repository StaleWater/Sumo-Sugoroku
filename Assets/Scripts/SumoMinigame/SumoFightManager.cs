using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumoFightManager : MonoBehaviour
{
    [SerializeField] SumoGuy player;
    [SerializeField] SumoGuy enemy;
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
        enemy.Init();
        StartCoroutine(CheckGameEnd());
    }

    IEnumerator CheckGameEnd() {
        var waiter = new WaitForSeconds(0.25f);
        while(true) {
            if(!InRing(player)) {
                GameEnd(enemy, player);
                break;
            } 
            else if(!InRing(enemy)) {
                GameEnd(player, enemy);
                break;
            }
            yield return waiter;
        }
    }

    void GameEnd(SumoGuy winner, SumoGuy loser) {
        background.color = bgEndColor;
        Debug.Log($"GAME OVER {winner} WINS");
    }

    bool InRing(SumoGuy guy) {
        var xpos = guy.transform.position.x;
        return Mathf.Abs(xpos) < (fieldSize - guy.box.bounds.extents.x);
    }

}
