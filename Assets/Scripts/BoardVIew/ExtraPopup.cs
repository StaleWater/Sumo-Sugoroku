using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using System;

public class ExtraPopup : MonoBehaviour {

    [SerializeField] private GameObject transitionUI; // Buttons to exit and show the text
    [SerializeField] private GameObject informationUI; // Information

    // Initialization
    public void Init() {
        transitionUI.SetActive(false);
        informationUI.SetActive(false);
        gameObject.SetActive(false);
    }

    // Begin and show the pop-up interface
    public void Begin() {
        Debug.Log("Extra: begin");
		// TODO: add some animation here before showing UI
		gameObject.SetActive(true);
		transitionUI.SetActive(true);
	}

    // Show the extra information
    public void ShowInformation() {
        Debug.Log("Extra: showing information");
		transitionUI.SetActive(false);
        informationUI.SetActive(true);
	}

    // Hide the extra information
    public void HideInformation() {
        Debug.Log("Extra: hiding information");
		transitionUI.SetActive(true);
		informationUI.SetActive(false);
	}

    // Close the extra pop-up
    public void Exit() {
        Debug.Log("Extra: exiting");
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine() {
        yield return null;
		transitionUI.SetActive(false);
		informationUI.SetActive(false);
		gameObject.SetActive(false);
	}
}