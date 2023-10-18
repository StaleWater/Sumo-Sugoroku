using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChankoItemSettings", menuName = "ScriptableObjects/ChankoItemSettings", order = 1)]
public class ChankoItemSettings : ScriptableObject
{
    public AnimationCurve scaleCurve;
    public AnimationCurve xMoveCurve;
    public AnimationCurve yMoveCurve;
    public float flyNumRotations;
    public float flyDurationSEC;
    public float endScalePercentage;

}

