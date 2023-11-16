using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerInfoBox : MonoBehaviour {

	[SerializeField] private List<Button> buttons;
	[SerializeField] private Button colorButton;
    [SerializeField] private List<Button> colorButtons;

	private UnityAction<int> removeEvent;
    private UnityAction<bool> selectColorEvent;
	private int id; // Index in player list, located in PlayerSelectMenu.cs

    private bool isSelectingColor = false;
    private Image buttonImage;

    public void Init(int id, UnityAction<int> removeEvent, UnityAction<bool> selectColorEvent) {
		buttonImage = colorButton.GetComponent<Image>();
		this.id = id;
        this.removeEvent = removeEvent;
        this.selectColorEvent = selectColorEvent;
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
            gameObject.transform.SetAsLastSibling();
            selectColorEvent?.Invoke(false);
        } else {
			isSelectingColor = false;
			selectColorEvent?.Invoke(true);
		}
    }

    public void SetColor(int index) {
		buttonImage.color = colorButtons[index].image.color;
    }

    public void ChangeAllButtonInteraction(bool interactable) {
		foreach (Button b in buttons) {
			b.interactable = interactable;
		}
	}
}
