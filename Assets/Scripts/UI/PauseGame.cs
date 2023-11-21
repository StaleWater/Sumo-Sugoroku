using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
	private float currTimeScale = 0.0f;
	private bool isGamePaused = false;

	public void Pause() {
		if (Time.timeScale > 0.0f) {
			isGamePaused = true;
			currTimeScale = Time.timeScale;
			Time.timeScale = 0.0f;
		}
		else if (Time.timeScale == 0.0f) {
			isGamePaused = false;
			Time.timeScale = currTimeScale;
		}
	}

	public bool IsGamePaused() {
		return isGamePaused;
	}

	public void BackToMenu() {
		if (Time.timeScale == 0.0f) {
			Time.timeScale = currTimeScale;
		}
		SceneManager.LoadScene("MainMenu");
		// StartCoroutine(BackToMenuHelper());
	}

// TODO
/*	
 	IEnumerator BackToMenuHelper() {
		yield return StartCoroutine(screenCurtain.FadeIn());
		SceneManager.LoadScene("MainMenu");
	}
*/
}
