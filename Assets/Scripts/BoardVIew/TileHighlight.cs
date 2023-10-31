using System.Collections;
using UnityEngine;

public class TileHighlight : MonoBehaviour {

	[Header("Settings")]
	[SerializeField] private float animationTimeSEC;
	[SerializeField] private AnimationCurve animationCurve;
	[SerializeField, Range(0.0f, 1.0f)] private float maxDimLevel;

	[Header("References")]
	[SerializeField] private GameObject highlight;
	[SerializeField] private GameObject shade;

	private void Awake() {
		highlight.SetActive(false);
		shade.SetActive(false);
	}

	// TODO: scale is reversed in x and y for some reason
	public void SetHighlight(in Tile tile) {
		highlight.transform.position = tile.transform.position;
		highlight.transform.localScale = new Vector3(tile.transform.localScale.y, tile.transform.localScale.x, tile.transform.localScale.z); ;
		// highlight.transform.rotation = tile.transform.rotation;
	}

	public void StartHightlight() {
		Debug.Log("Starting highlight");
		StartCoroutine(StartHighlightRoutine());
	}

	public void EndHighlight() {
		Debug.Log("Ending highlight");
		StartCoroutine(EndHighlightRoutine());
	}

	private IEnumerator StartHighlightRoutine() {
		highlight.SetActive(true);
		shade.SetActive(true);
		yield return StartCoroutine(Highlight(0.0f, maxDimLevel));
	}

	private IEnumerator EndHighlightRoutine() {
		yield return StartCoroutine(Highlight(maxDimLevel, 0.0f));
		highlight.SetActive(false);
		shade.SetActive(false);
	}

	private IEnumerator Highlight(float initialLevel, float finalLevel) {
		float elapsedTime = 0.0f;
		while (elapsedTime <= animationTimeSEC) {
			float currDim = Mathf.Lerp(initialLevel, finalLevel, animationCurve.Evaluate(elapsedTime / animationTimeSEC));
			Color c = shade.GetComponent<SpriteRenderer>().color;
			c.a = currDim;
			shade.GetComponent<SpriteRenderer>().color = c;

			elapsedTime += Time.deltaTime;

			yield return null;
		}
	}
}
