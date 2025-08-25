using UnityEngine;

public class BuildingClick : MonoBehaviour
{
    private BuildingPosition buildingData;
    private BuildingProgress buildingProgress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingData = GetComponent<BuildingPosition>();
        buildingProgress = GetComponent<BuildingProgress>();
    }

    public void HandleClick()
    {
        if (buildingProgress != null && !buildingProgress.isComplete)
        {
            return;
        }
        if (BuildingUIManager.Instance != null)
        {
            BuildingUIManager.Instance.ShowBuildingInfo(this.gameObject, buildingData);
        }
    }
}