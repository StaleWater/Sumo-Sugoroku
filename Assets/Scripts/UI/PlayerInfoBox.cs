using UnityEngine;
using UnityEngine.Events;

public class PlayerInfoBox : MonoBehaviour {

    private UnityAction<int> removeEvent;
    private int id; // Index in player list located in PlayerSelectMenu.cs

    public void Init(int id, UnityAction<int> removeEvent) {
        this.id = id;
        this.removeEvent = removeEvent;
    }

    public void UpdateId(int id) {
        this.id = id;
    }

    public void RemoveSelf() {
		removeEvent?.Invoke(id);
    }
}
