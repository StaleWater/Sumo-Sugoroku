using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	[SerializeField] UIFadeable screenCurtain;
	[SerializeField] private string startGameSceneName;

	private void Start() {
		screenCurtain.Init();
		screenCurtain.Show();

		StartCoroutine(screenCurtain.FadeOut());
    }

	public void StartGame() {
		StartCoroutine(StartGameHelper());
	}

	private IEnumerator StartGameHelper() {
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(screenCurtain.FadeIn());
		SceneManager.LoadScene(startGameSceneName);
	}

	public void Options() {
		// TODO
	}
}
