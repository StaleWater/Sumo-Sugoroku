using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChankoManager : MonoBehaviour
{
    [SerializeField] ChankoItem[] items;
    [SerializeField] float showTime;
    [SerializeField] float betweenItemsTime;

    Fadeable[] itemImages;
    bool selectTime;
    WaitForSeconds showWaiter;
    WaitForSeconds betweenWaiter;
    int[] order;
    int orderIndex;

    void Start() {
        Init();    
    }

    public void Init() {
        selectTime = false;
        showWaiter = new WaitForSeconds(showTime);
        betweenWaiter = new WaitForSeconds(betweenItemsTime);
        for(int i=0; i < items.Length; i++) {
            items[i].Init(i, this);
        }
        SpawnImages();
    }

    void SpawnImages() {
        itemImages = new Fadeable[items.Length];
        for(int i=0; i < items.Length; i++) {
            Fadeable img = Instantiate(items[i].GetComponent<Fadeable>());
            img.transform.position = transform.position;
            img.Hide();
            itemImages[i] = img;
        }
    }

    public void StartGame() {
        order = RandomItemOrder();
        StartCoroutine(Gametime());
    }

    IEnumerator Gametime() {
        foreach(var item in items) {
            item.GetComponent<Fadeable>().FadeIn();
        }
        yield return new WaitUntil(() => {
            bool done = true;
            foreach(var item in items) {
                if(item.GetComponent<Fadeable>().IsFading()) {
                    done = false;
                    break;
                }
            }
            return done;
        });
        Debug.Log("MADE IT");

        yield return StartCoroutine(Showtime());

    }

    IEnumerator Showtime() {
        for(int i=0; i < order.Length; i++) {
            yield return StartCoroutine(ShowItem(order[i]));
            yield return betweenWaiter;
        }

        selectTime = true;
        orderIndex = 0;
    }

    int[] RandomItemOrder() {
        int size = items.Length;
        int[] arr = new int[size];
        for(int i=0; i < size; i++) {
            arr[i] = i;
        }
        for(int i=0; i < size - 1; i++) {
            int r = Random.Range(i, size);
            int temp = arr[i];
            arr[i] = arr[r];
            arr[r] = temp;
        }

        return arr;
    }

    public IEnumerator ShowItem(int id) {
        Fadeable img = itemImages[id];

        img.FadeIn();
        yield return showWaiter;
        img.FadeOut();
    }

    public void OnItemClick(int itemId) {
        if(!selectTime) return;
        if(itemId != order[orderIndex]) {
            Debug.Log("WRONG ITEM");
            GameEnd(false);
        }
        else {
            Debug.Log("GOOD");
            items[itemId].GetComponent<Fadeable>().FadeOut();
            orderIndex++;
            if(orderIndex >= order.Length) GameEnd(true);
        }
    }

    void GameEnd(bool win) {
        selectTime = false;
        if(win) Debug.Log("GOOD JOB YOU WIN");
        else Debug.Log("IDIOT YOU SUCK YOU LOSE");
    }

}
