using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

static class PlayerSelectMenuConstants {
	public const int MAX_PLAYER_COUNT = 4;
}

public struct PlayerSelectData {
	public string name;
	public int colorIndex;
	public bool isBot;

	public PlayerSelectData(string name, int colorIndex, bool isBot) {
		this.name = name;
		this.colorIndex = colorIndex;
		this.isBot = isBot;
	}
}

public class PlayerSelectMenu : MonoBehaviour {

	public static List<PlayerSelectData> playerData;

	[SerializeField] UIFadeable screenCurtain;

	[SerializeField] private GameObject playerInfoBoxPrefab;
	private PlayerInfoBox playerInfoBox;
	[SerializeField] private GameObject location;
	[SerializeField] private string mainMenuSceneName;
	[SerializeField] private string startGameSceneName;

	[SerializeField] private Button addButton;
	[SerializeField] private Button startGameButton;

	[SerializeField] private List<Button> buttons;

	public int playerCount;

	public List<PlayerInfoBox> playerList = new List<PlayerInfoBox>();

	private Canvas canvas;
	private RectTransform container; // Rect transform of gameobject where players are stored

	private void Start() {
		screenCurtain.Init();
		screenCurtain.Show();

		StartCoroutine(screenCurtain.FadeOut());

		playerInfoBox = playerInfoBoxPrefab.GetComponent<PlayerInfoBox>();
		canvas = transform.parent.GetComponent<Canvas>();
		container = location.GetComponent<RectTransform>();

		PlayerInfoBox.ResetAvailableColors();

		// Start with 1 player
		AddPlayer();
	}

	void SetupStaticData() {
		playerData = new List<PlayerSelectData>();
		foreach(var ibox in playerList) {
			var data = new PlayerSelectData(ibox.GetName(), ibox.GetColorIndex(), ibox.IsBot());
			playerData.Add(data);
		}
	}

	public void GoToMainMenu() {
		SceneManager.LoadScene(mainMenuSceneName);
	}

	public void StartGame() {
		StartCoroutine(StartGameHelper());
	}

	private IEnumerator StartGameHelper() {
		SetupStaticData();
		yield return StartCoroutine(screenCurtain.FadeIn());
			
		BgmMultiScene bgm = GameObject.FindGameObjectWithTag("bgm-multiscene").GetComponent<BgmMultiScene>();
		bgm.Stop();
		Destroy(bgm.gameObject);
		Debug.Log("bgm destroyed");

		SceneManager.LoadScene(startGameSceneName);
	}

	public void AddPlayer() {

		// Create a new player info box GameObject and update states
		playerList.Add(Instantiate(
			playerInfoBox,
			CalculatePosition(playerCount),
			Quaternion.identity,
			location.transform
		));

		PlayerInfoBox infoBox = playerList[playerCount];
		infoBox.Init(playerCount, RemovePlayer, ChangeAllButtonInteraction);

		++playerCount;

		// Update the position of the add button
		if (playerCount >= PlayerSelectMenuConstants.MAX_PLAYER_COUNT) {
			addButton.gameObject.SetActive(false);
		}
		else {
			addButton.transform.position = CalculatePosition(playerCount);
		}

		// Make sure the start button is active
		startGameButton.interactable = true;
	}

	public void RemovePlayer(int index) {

		--playerCount;

		// Make sure the start button is inactive if there are no more players
		if (playerCount <= 0) {
			startGameButton.interactable = false;
		}

		// Remove the given info box
		Destroy(playerList[index].gameObject);
		playerList.RemoveAt(index);

		// Reorganize the info boxes as necessary
		for (int i = 0; i < playerList.Count; ++i) {
			playerList[i].transform.position = CalculatePosition(i);
			PlayerInfoBox infoBox = playerList[i];
			infoBox.UpdateId(i);
		}

		// Update the position of the add button
		addButton.transform.position = CalculatePosition(playerCount);

		// Make sure the add button is active
		if (!addButton.gameObject.activeSelf) {
			addButton.gameObject.SetActive(true);
		}
	}

	private void ChangeAllButtonInteraction(bool interactable) {
		foreach (Button b in buttons) {
			b.interactable = interactable;
		}

		foreach (PlayerInfoBox infoBox in playerList) {
			infoBox.ChangeAllButtonInteraction(interactable);
		}
	}

	private Vector3 CalculatePosition(int index) {
		Vector3 newPosition = location.transform.position;

		float playerInfoBoxHeight = playerInfoBoxPrefab.GetComponent<RectTransform>().rect.height * canvas.scaleFactor;
		float containerHeight = container.rect.height * canvas.scaleFactor;
		float step = containerHeight / PlayerSelectMenuConstants.MAX_PLAYER_COUNT;
		float offset = (index * step) + (playerInfoBoxHeight / 2);
		newPosition.y -= offset - (containerHeight / 2);

		return newPosition;
	}
}
