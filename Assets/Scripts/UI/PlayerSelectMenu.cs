using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

static class PlayerSelectMenuConstants {
	public const int MAX_PLAYER_COUNT = 4;
}

public class PlayerSelectMenu : MonoBehaviour {

	[SerializeField] UIFadeable screenCurtain;

	[SerializeField] private GameObject playerInfoBoxPrefab;
	[SerializeField] private GameObject location;
	[SerializeField] private string mainMenuSceneName;
	[SerializeField] private string startGameSceneName;

	[SerializeField] private Button addButton;
	[SerializeField] private Button startGameButton;

	[SerializeField] private List<Button> buttons;

	private static int playerCount;
	private static Vector3 locationPosition;
	private static List<GameObject> playerList = new List<GameObject>();

	private static readonly List<Vector3> predeterminedBoxPositions = new List<Vector3> {
		new Vector3(0.0f, 275.0f, 0.0f),
		new Vector3(0.0f, 100.0f, 0.0f),
		new Vector3(0.0f, -75.0f, 0.0f),
		new Vector3(0.0f, -250.0f, 0.0f)
	};

	private void Start() {
		screenCurtain.Init();
		screenCurtain.Show();

		StartCoroutine(screenCurtain.FadeOut());

		locationPosition = location.transform.position;

		// Start with 1 player
		AddPlayer();
	}

	public void GoToMainMenu() {
		SceneManager.LoadScene(mainMenuSceneName);
	}

	public void StartGame() {
		StartCoroutine(StartGameHelper());
	}

	private IEnumerator StartGameHelper() {
		yield return StartCoroutine(screenCurtain.FadeIn());
		SceneManager.LoadScene(startGameSceneName);
	}

	public void AddPlayer() {

		// Create a new player info box GameObject and update states
		playerList.Add(Instantiate(
			playerInfoBoxPrefab,
			locationPosition + predeterminedBoxPositions[playerCount],
			Quaternion.identity,
			location.transform
		));
		PlayerInfoBox infoBox = playerList[playerCount].GetComponent<PlayerInfoBox>();
		infoBox.Init(playerCount, RemovePlayer, ChangeAllButtonInteraction);
		++playerCount;

		// Update the position of the add button
		if (playerCount >= PlayerSelectMenuConstants.MAX_PLAYER_COUNT) {
			addButton.gameObject.SetActive(false);
		}
		else {
			addButton.transform.position = locationPosition + predeterminedBoxPositions[playerCount];
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
		Destroy(playerList[index]);
		playerList.RemoveAt(index);

		// Reorganize the info boxes as necessary
		for (int i = 0; i < playerList.Count; ++i) {
			playerList[i].transform.position = locationPosition + predeterminedBoxPositions[i];
			PlayerInfoBox infoBox = playerList[i].GetComponent<PlayerInfoBox>();
			infoBox.UpdateId(i);
		}

		// Update the position of the add button
		addButton.transform.position = locationPosition + predeterminedBoxPositions[playerCount];

		// Make sure the add button is active
		if (!addButton.gameObject.activeSelf) {
			addButton.gameObject.SetActive(true);
		}
	}

	private void ChangeAllButtonInteraction(bool interactable) {
		foreach (Button b in buttons) {
			b.interactable = interactable;
		}

		foreach (GameObject player in playerList) {
			PlayerInfoBox infoBox = player.GetComponent<PlayerInfoBox>();
			infoBox.ChangeAllButtonInteraction(interactable);
		}
	}
}
