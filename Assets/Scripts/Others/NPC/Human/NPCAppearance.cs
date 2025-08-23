using UnityEngine;

[CreateAssetMenu(fileName = "NPCAppearance", menuName = "Scriptable Objects/NPCAppearance")]
public class NPCAppearance : ScriptableObject
{
    public enum Era { Founding, Golden, Modern }

    public Era era;
    public Sprite sprite;
    public AnimatorOverrideController animatorOverride;
}
