using UnityEngine;

[CreateAssetMenu(fileName = "MovableSettings", menuName = "ScriptableObjects/MovableSettings", order = 1)]
public class MovableSettings : ScriptableObject
{
    [Range(0.0f, 50.0f)]
    public float movementSpeed;

    public AnimationCurve curve;

}

