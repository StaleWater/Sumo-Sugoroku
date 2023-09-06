using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FadeableSettings", menuName = "ScriptableObjects/FadeableSettings", order = 1)]
public class FadeableSettings : ScriptableObject
{

    [Tooltip("percentage/second")] // Shows when you hover over the name
    [Range(0.5f, 10.0f)] // Make it show up as a slider!!!!
    public float fadeInSpeed;

    [Tooltip("percentage/second")] 
    [Range(0.5f, 10.0f)] 
    public float fadeOutSpeed;

}

