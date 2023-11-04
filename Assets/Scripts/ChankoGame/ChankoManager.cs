using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChankoManager : MonoBehaviour
{
    [SerializeField] ChankoItem[] items;
    [SerializeField] float showTime;
    [SerializeField] float betweenItemsTime;
    [SerializeField] Transform pot;
    [SerializeField] int numRounds;
    [SerializeField] UIFadeable screenCurtain;

    Fadeable[] itemImages;
    bool selectTime;
    WaitForSeconds showWaiter;
    WaitForSeconds betweenWaiter;
    int[] order;
    int orderIndex;
    int curRound;

    void Start() {
        Init();    
    }

    public void Init() {
        selectTime = false;
        showWaiter = new WaitForSeconds(showTime);
        betweenWaiter = new WaitForSeconds(betweenItemsTime);

        screenCurtain.Init();
        screenCurtain.gameObject.SetActive(true);

        for(int i=0; i < items.Length; i++) {
            items[i].Init(i, this);
        }
        SpawnImages();

        StartCoroutine(Prep());
    }

    IEnumerator Prep() {
        screenCurtain.Show();
        yield return StartCoroutine(screenCurtain.FadeOut());
        yield return new WaitForSeconds(1.0f);
    }


    void SpawnImages() {
        itemImages = new Fadeable[items.Length];
        for(int i=0; i < items.Length; i++) {
            Fadeable img = Instantiate(items[i].fade);
            img.transform.position = transform.position;
            img.Hide();
            itemImages[i] = img;
        }
    }

    void HideItems() {
        foreach(var item in items) {
            item.fade.Hide();
        }
    }

    bool AllItemsGone() {
        foreach(var item in items) {
            if(item.fade.IsVisible()) return false;
        }
        return true;
    }

    public void StartGame() {
        curRound = 0;
        HideItems();
        StartCoroutine(NextRound());
    }

    IEnumerator NextRound() {
        curRound++;
        if(curRound > numRounds) {
            GameEnd();
            yield break;
        }

        Debug.Log($"ROUND {curRound}");

        order = RandomItemOrder();

        yield return StartCoroutine(Showtime());

        yield return StartCoroutine(ResetItems());

        selectTime = true;
        orderIndex = 0;
    }

    IEnumerator ResetItems() {
        foreach(var item in items) {
            item.ResetState();
            item.fade.FadeIn();
        }

        yield return new WaitUntil(() => {
            bool done = true;
            foreach(var item in items) {
                if(item.fade.IsFading()) {
                    done = false;
                    break;
                }
            }
            return done;
        });
    }

    IEnumerator Showtime() {
        for(int i=0; i < order.Length; i++) {
            yield return StartCoroutine(ShowItem(order[i]));
            yield return betweenWaiter;
        }

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

    public IEnumerator OnItemClick(int itemId) {
        if(!selectTime) yield break;

        if(itemId != order[orderIndex]) {
            HideItems();
            RoundEnd(false);
        }
        else {
            var item = items[itemId];

            orderIndex++;
            if(orderIndex >= order.Length) StartCoroutine(RoundEnd(true));

            var newItemPos = pot.position;
            newItemPos.z = 1.0f;
            yield return StartCoroutine(item.FlyTo(newItemPos));
            items[itemId].fade.FadeOut();
        }
    }

    IEnumerator RoundEnd(bool win) {
        selectTime = false;

        if(win) Debug.Log("GOOD JOB");
        else {
            Debug.Log("IDIOT YOU SUCK YOU LOSE");
            GameEnd();
            yield break;
        }

        yield return new WaitUntil(() => AllItemsGone());

        StartCoroutine(NextRound());
    }

    void GameEnd() {
        Debug.Log("GAME DONE");
        StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

}
