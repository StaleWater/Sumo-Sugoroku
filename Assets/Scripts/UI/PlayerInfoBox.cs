using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInfoBox : MonoBehaviour {

	[SerializeField] private List<Button> buttons;
	[SerializeField] private Button selectColorButton;
	[SerializeField] private List<Button> colorButtons;
	[SerializeField] private Button playerTypeButton;

	[SerializeField] private TMP_InputField nameField;

	[SerializeField] private Sprite playerImage;
	[SerializeField] private Sprite botImage;
	private bool isBot;
	private Image playerTypeImage;

	private List<bool> isColorAvailable;

	private UnityAction<int> removeEvent;
	private UnityAction<bool> selectColorEvent;
	private int id; // Index in player list, located in PlayerSelectMenu.cs
	private bool isSelectingColor = false;
	private int currColorIndex;
	private Image colorButtonImage;

	public void Init(int id, UnityAction<int> removeEvent, UnityAction<bool> selectColorEvent) {
		colorButtonImage = selectColorButton.GetComponent<Image>();
		playerTypeImage = playerTypeButton.GetComponent<Image>();
		this.removeEvent = removeEvent;
		this.selectColorEvent = selectColorEvent;
		isBot = false;
		UpdateId(id);

		// If it doesn't already exist, initialize a shared list of available colors
		if (isColorAvailable == null) {
			isColorAvailable = new List<bool>(colorButtons.Count);
			for (int i = 0; i < colorButtons.Count; ++i) {
				isColorAvailable.Add(true);
			}
		}

		// Set the default color
		int j = 0;
		while (!isColorAvailable[j]) {
			++j;
		}
		SetColor(j);
	}

	public string GetName() {
		return nameField.text;	
	}

	public int GetColorIndex() {
		return currColorIndex;
	}

	public bool IsBot() {
		return isBot;
	}

	public void UpdateId(int id) {
		nameField.placeholder.GetComponent<TMP_Text>().text = "Player " + (id + 1);
		this.id = id;
	}

	public void RemoveSelf() {
		// Make the current color available before removing
		isColorAvailable[currColorIndex] = true;

		removeEvent?.Invoke(id);
	}

	public void TogglePlayerType() {
		isBot = !isBot;
		if (isBot) {
			playerTypeImage.sprite = botImage;
		}
		else {
			playerTypeImage.sprite = playerImage;
		}
	}

	public void SelectColor() {
		if (!isSelectingColor) {
			isSelectingColor = true;

			// Make the current color available
			isColorAvailable[currColorIndex] = true;

			// Keep up to date with available colors relative to other info. boxes; change interactivity
			for (int i = 0; i < isColorAvailable.Count; ++i) {
				colorButtons[i].interactable = isColorAvailable[i];
			}

			gameObject.transform.SetAsLastSibling();
			selectColorEvent?.Invoke(false);
		} 
		else {
			isSelectingColor = false;
			selectColorEvent?.Invoke(true);
		}
	}

	public void SetColor(int colorIndex) {
		currColorIndex = colorIndex;
		isColorAvailable[colorIndex] = false;
		colorButtons[colorIndex].interactable = false;
		colorButtonImage.color = colorButtons[colorIndex].image.color;
		playerTypeImage.color = colorButtonImage.color;
	}

	public void ChangeAllButtonInteraction(bool interactable) {
		foreach (Button b in buttons) {
			b.interactable = interactable;
		}
	}
}
