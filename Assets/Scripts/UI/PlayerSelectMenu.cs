using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectMenu : MonoBehaviour {

	[SerializeField] private GameObject playerInfoBoxPrefab;
	[SerializeField] private GameObject location;

	private void Start() {
		// Instantiate(playerInfoBoxPrefab, Vector3.one, Quaternion.identity, location.transform);
	}
}
