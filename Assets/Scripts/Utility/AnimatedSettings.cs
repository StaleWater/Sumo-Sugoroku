using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatedSettings", menuName = "ScriptableObjects/AnimatedSettings", order = 1)]
public class AnimatedSettings : ScriptableObject
{
    public AnimationCurve scaleCurve;
    public float scaleDurationSEC;

}

