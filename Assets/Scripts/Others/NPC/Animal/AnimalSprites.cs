using UnityEngine;

[CreateAssetMenu(fileName = "AnimalAppearance", menuName = "Scriptable Objects/AnimalAppearance")]
public class AnimalSprites : ScriptableObject
{
    public Sprite sprite;
    public AnimatorOverrideController animatorOverride;
}