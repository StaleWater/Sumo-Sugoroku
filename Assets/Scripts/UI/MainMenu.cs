using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    [SerializeField] private StyleSheet styleSheet;

	[SerializeField] private string startGameSceneName;

    private void Start()
    {
		StartCoroutine(Generate());
    }

	// Updates the visuals while editing it in code
	private void OnValidate()
	{
		if (Application.isPlaying)
			return;

		StartCoroutine(Generate());
	}

	private IEnumerator Generate()
    {
		yield return null;

		VisualElement root = document.rootVisualElement;
		root.Clear();

		root.styleSheets.Add(styleSheet);



		Label title = new("SUMO");
		title.AddToClassList("title");

		Label subtitle = new("SUGOROKU");
		subtitle.AddToClassList("sub-title");

		VisualElement titleContainer = new();
		titleContainer.AddToClassList("title-container");
		titleContainer.Add(title);
		titleContainer.Add(subtitle);



		Button startGameButton = new();
		startGameButton.text = "Start Game";
		startGameButton.clicked += StartGame;

		Button optionsButton = new();
		optionsButton.text = "Options";

		VisualElement menuButtons = new();
		menuButtons.AddToClassList("menu-buttons");
		menuButtons.Add(startGameButton);
		menuButtons.Add(optionsButton);



		root.Add(titleContainer);
		root.Add(menuButtons);
	}

	private void StartGame()
	{
		SceneManager.LoadScene(startGameSceneName);
	}

	private void Options()
	{
		// TODO
	}
}
