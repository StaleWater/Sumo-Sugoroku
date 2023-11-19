using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInfoBox : MonoBehaviour {

	[SerializeField] private List<Button> buttons;
	[SerializeField] private Button selectColorButton;
	[SerializeField] private List<Button> colorButtons;

	static private List<bool> isColorAvailable;

	private UnityAction<int> removeEvent;
	private UnityAction<bool> selectColorEvent;
	private int id; // Index in player list, located in PlayerSelectMenu.cs
	private bool isSelectingColor = false;
	private int currColorIndex;
	private Image buttonImage;

	public void Init(int id, UnityAction<int> removeEvent, UnityAction<bool> selectColorEvent) {
		buttonImage = selectColorButton.GetComponent<Image>();
		this.id = id;
		this.removeEvent = removeEvent;
		this.selectColorEvent = selectColorEvent;

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

	public void UpdateId(int id) {
		this.id = id;
	}

	public void RemoveSelf() {
		removeEvent?.Invoke(id);
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
		buttonImage.color = colorButtons[colorIndex].image.color;
	}

	public void ChangeAllButtonInteraction(bool interactable) {
		foreach (Button b in buttons) {
			b.interactable = interactable;
		}
	}
}
