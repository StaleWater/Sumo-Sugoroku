using UnityEngine;
using UnityEngine.SceneManagement;

static class PlayerSelectMenuConstants {
	public const int MAX_PLAYER_COUNT = 4;
}

public class PlayerSelectMenu : MonoBehaviour {

	[SerializeField] private GameObject playerInfoBoxPrefab;
	[SerializeField] private GameObject location;
	[SerializeField] private string mainMenuSceneName;
	[SerializeField] private string startGameSceneName;

	private static int playerCount;

	private void Start() {
		// Start with 1 player
		playerCount = 1;
		Instantiate(playerInfoBoxPrefab, Vector3.one, Quaternion.identity, location.transform);
	}

	public void GoToMainMenu() {
		SceneManager.LoadScene(mainMenuSceneName);
	}

	public void StartGame() {
		SceneManager.LoadScene(startGameSceneName);
	}
}
