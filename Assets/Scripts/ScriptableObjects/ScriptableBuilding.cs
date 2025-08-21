using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableBuilding", menuName = "Scriptable Objects/ScriptableBuilding")]
public class ScriptableBuilding : ScriptableObject
{
    public string buildingName;
    public Sprite inProgressSprite;
    public List<FinishedSpriteVariant> finishedSprites;
    public int turnsToBuild = 3;
}
[System.Serializable]
public class FinishedSpriteVariant
{
    public string plotName;
    public Sprite sprite;
}
