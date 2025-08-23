using UnityEngine;

[CreateAssetMenu(fileName = "AnimalAppearance", menuName = "Scriptable Objects/AnimalAppearance")]
public class AnimalAppearance : ScriptableObject
{
    public Sprite sprite;
    public AnimatorOverrideController animatorOverride;
}