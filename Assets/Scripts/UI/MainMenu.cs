using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {

	[SerializeField] UIFadeable screenCurtain;
	[SerializeField] private string startGameSceneName;

	private void Start() {
		screenCurtain.gameObject.SetActive(true);
		screenCurtain.Init();
		screenCurtain.Show();

		StartCoroutine(screenCurtain.FadeOut());
    }

	public void StartGame() {
		StartCoroutine(StartGameHelper());
	}

	private IEnumerator StartGameHelper() {
		yield return StartCoroutine(screenCurtain.FadeIn());
		SceneManager.LoadScene(startGameSceneName);
	}

	public void Options() {
		// TODO
	}
}
