using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ChankoManager : MonoBehaviour
{
    [SerializeField] ChankoItem[] itemTypes;
    [SerializeField] float showTime;
    [SerializeField] float betweenItemsTime;
    [SerializeField] Transform pot;
    [SerializeField] int numRounds;
    [SerializeField] UIFadeable screenCurtain;
    [SerializeField] UIFadeable infoTextContainer;
    [SerializeField] TMP_Text infoText;
    [SerializeField] UIFadeable instructionsPanel;
    [SerializeField] float itemsPosYOffset;
    [SerializeField] float itemsSpreadWidth;
    [SerializeField] ClickCollider clickOverlay;

    Fadeable[] itemImages;
    bool selectTime;
    bool lastChance;
    WaitForSeconds showWaiter;
    WaitForSeconds betweenWaiter;
    WaitUntil noneFading;
    List<int> order;
    List<ChankoItem> items;
    int orderIndex;
    int[] counts;
    int curRound;
    int numItemTypes;
    int numTypesUsed;


    void Start() {
        Init();    
    }

    public void Init() {
        selectTime = false;
        lastChance = false;
        showWaiter = new WaitForSeconds(showTime);
        betweenWaiter = new WaitForSeconds(betweenItemsTime);
        noneFading = new WaitUntil(() => NoItemsFading());

        screenCurtain.Init();
        screenCurtain.gameObject.SetActive(true);

        infoTextContainer.Init();
        infoTextContainer.gameObject.SetActive(true);
        infoTextContainer.Show();

        instructionsPanel.Init();
        instructionsPanel.gameObject.SetActive(true);
        instructionsPanel.Show();

        clickOverlay.gameObject.SetActive(false);

        numItemTypes = itemTypes.Length;

        items = new List<ChankoItem>();
        order = new List<int>();
        counts = new int[numItemTypes];

        SpawnImages();

        StartCoroutine(Prep());
    }

    public void OnClickToStart() {
        clickOverlay.gameObject.SetActive(false);
        StartGame();
    }
    
    public void OnInstructionsClose() {
        StartCoroutine(InstructionsCloseHelper());
    }

    IEnumerator InstructionsCloseHelper() {
        yield return StartCoroutine(instructionsPanel.FadeOut());
        instructionsPanel.gameObject.SetActive(false);

        clickOverlay.gameObject.SetActive(true);
    }

    IEnumerator Prep() {
        screenCurtain.Show();
        yield return StartCoroutine(screenCurtain.FadeOut());
        yield return new WaitForSeconds(1.0f);
    }

    void SpawnImages() {
        itemImages = new Fadeable[numItemTypes];

        for(int i=0; i < numItemTypes; i++) {
            ChankoItem item = Instantiate(itemTypes[i]);

            // init as non-clickable item
            item.Init(-1, this, false);

            Fadeable img = item.GetComponent<Fadeable>();
            img.transform.position = transform.position;
            img.Hide();
            itemImages[i] = img;
        }
    }

    IEnumerator SpawnItems() {
        float gapWidth = itemsSpreadWidth / numTypesUsed;
        float xOffset = transform.position.x - gapWidth * (numTypesUsed / 2);
        if(numTypesUsed % 2 == 0) xOffset += gapWidth / 2.0f;

        for(int i=0; i < numItemTypes; i++) {
            for(int j=0; j < counts[i]; j++) {
                var item = Instantiate(itemTypes[i]);

                var pos = transform.position;
                pos.y += itemsPosYOffset;
                pos.x += xOffset;
                item.transform.position = pos;

                item.Init(i, this, true);
                items.Add(item);
                item.fade.FadeIn();
            }
            if(counts[i] > 0) xOffset += gapWidth;
        }

        yield return noneFading;

    }

    void DestroyAllItems() {
        foreach(ChankoItem item in items) {
            item.DestroyThis();
        }

        items.Clear();
    }

    bool AllItemsGone() {
        foreach(var item in items) {
            if(item.fade.IsVisible()) return false;
        }
        return true;
    }

    bool NoItemsFading() {
        foreach(var item in items) {
            if(item.fade.IsFading()) return false;
        }
        return true;
    }

    void HideItems() {
        foreach(var item in items) {
            item.fade.Hide();
        }
    }

    IEnumerator FadeOutAllItems() {
        foreach(var item in items) {
            item.fade.FadeOut();
        }

        yield return noneFading;
    }


    public void StartGame() {
        curRound = 0;
        HideItems();
        StartCoroutine(NextRound());
    }

    IEnumerator NextRound() {
        curRound++;
        if(curRound > numRounds) {
            StartCoroutine(GameEnd(true));
            yield break;
        }

        infoText.text = $"Round {curRound}";

        int len = SugorokuManager.stateData.curChankoLevel + numItemTypes + curRound - 3;
        len = Mathf.Min(len, 8);

        RandomItemOrder(len);

        yield return StartCoroutine(Showtime());

        yield return StartCoroutine(SpawnItems());

        selectTime = true;
        orderIndex = 0;
    }

    IEnumerator Showtime() {
        for(int i=0; i < order.Count; i++) {
            yield return StartCoroutine(ShowItem(order[i]));
            yield return betweenWaiter;
        }

    }


    void RandomItemOrder(int length) {
        order.Clear();
        numTypesUsed = 0;

        for(int i=0; i < numItemTypes; i++) {
            counts[i] = 0;
        }

        for(int i=0; i < length; i++) {
            int n = Random.Range(0, numItemTypes);
            order.Add(n);
            if(counts[n] == 0) numTypesUsed++;
            counts[n]++;
        }
        
    }

    public IEnumerator ShowItem(int id) {
        Fadeable img = itemImages[id];

        img.FadeIn();
        yield return showWaiter;
        img.FadeOut();
    }

    public IEnumerator OnItemClick(ChankoItem clicked) {
        if(!selectTime) yield break;

        int itemId = clicked.Type();

        if(itemId != order[orderIndex]) {
            StartCoroutine(RoundEnd(false));
        }
        else {
            // correct item was selected

            orderIndex++;
            if(orderIndex >= order.Count) StartCoroutine(RoundEnd(true));

            var newItemPos = pot.position;
            newItemPos.z = 1.0f;
            yield return StartCoroutine(clicked.FlyTo(newItemPos));
            clicked.fade.Hide();
        }
    }

    IEnumerator RoundEnd(bool win) {
        selectTime = false;

        if(win) {
            lastChance = false;
        }
        else if(!lastChance) {
            infoText.text = "Wrong! One more chance!";
            lastChance = true;
            curRound--;

            yield return StartCoroutine(FadeOutAllItems());
            yield return new WaitForSeconds(2.0f);
        }
        else {
            StartCoroutine(GameEnd(false));
            yield break;
        }

        yield return new WaitUntil(() => AllItemsGone());
        DestroyAllItems();

        StartCoroutine(NextRound());
    }

    IEnumerator GameEnd(bool win) {
        if(win) infoText.text = "You Win!";
        else infoText.text = "You Lose!";
        SugorokuManager.stateData.wonMinigame = win;

        yield return StartCoroutine(BackToBoard());
    }

    IEnumerator BackToBoard() {
        yield return new WaitForSeconds(5.0f);
        yield return StartCoroutine(screenCurtain.FadeIn());
        SceneManager.LoadScene("TheBoard");
    }

}
