using UnityEngine;

public class GridTileClickHandler : MonoBehaviour
{
    private int gridX;
    private int gridY;
    private string gridID;

    public void SetGridPosition(int x, int y, string id)
    {
        gridX = x;
        gridY = y;
        gridID = id;
    }

    public void HandleClick()
    {
        if (BuildingManager.Instance != null && BuildingManager.Instance.isInPlacementMode)
        {
            Vector3 buildingPosition = new(transform.position.x - 0.5f, transform.position.y - 0.5f, 0f);
            BuildingManager.Instance.PlaceBuildingAtGrid(buildingPosition, gridX, gridY, gridID);
        }
    }
}