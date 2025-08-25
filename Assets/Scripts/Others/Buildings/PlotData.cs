using UnityEngine;
using static PlotManager;

[CreateAssetMenu(fileName = "PlotData", menuName = "Scriptable Objects/PlotData")]
public class PlotData : ScriptableObject
{
    public string PlotName;
    public int Level = 1;
    public Sprite PlotImage;
    public GameObject BuildingPrefab;
    public PlotCategory PlotCategory;
    public PlotBuildable PlotBuildable;
    public int AvailableBuildByTurn;
    public int TurnsToBuild;
    public int TileSizeWidth;
    public int TileSizeHeight;
    public int CostFunds;
    public int CostWood;
    public int CostStone;
    public int CostMetal;
    public int GainFunds;
    public int GainWood;
    public int GainStone;
    public int GainPopulation;

    public string Description = "";

    public PlotData[] Upgrades;
}
