using UnityEngine;

public class BuildingClick : MonoBehaviour
{
    private BuildingPosition buildingData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buildingData = GetComponent<BuildingPosition>();
    }

    public void HandleClick()
    {
        if (BuildingUIManager.Instance != null)
        {
            BuildingUIManager.Instance.ShowBuildingInfo(this.gameObject, buildingData);
        }
    }
}