using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInstructions : MonoBehaviour
{
    [SerializeField] FadeRestore spritesContainer;
    [SerializeField] UIFadeable textContainer;

    public void Init() {
       textContainer.Init();
       spritesContainer.Init();
    }

    public void Hide() {
        spritesContainer.Hide();
        textContainer.Hide();

        textContainer.gameObject.SetActive(false);
        spritesContainer.gameObject.SetActive(false);
    }

    public void Show() {
        textContainer.gameObject.SetActive(true);
        spritesContainer.gameObject.SetActive(true);

        spritesContainer.Show();
        textContainer.Show();
    }

    public IEnumerator FadeIn() {
        textContainer.gameObject.SetActive(true);
        spritesContainer.gameObject.SetActive(true);

        StartCoroutine(textContainer.FadeIn());
        yield return StartCoroutine(spritesContainer.FadeIn());
    }

    public IEnumerator FadeOut() {
        StartCoroutine(textContainer.FadeOut());
        yield return StartCoroutine(spritesContainer.FadeOut());
        
        textContainer.gameObject.SetActive(false);
        spritesContainer.gameObject.SetActive(false);
    }

}
