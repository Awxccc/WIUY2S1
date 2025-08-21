using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Manager Settings")]
    public static BuildingManager Instance;
    public bool isInPlacementMode = false;

    private GameObject selectedBuildingPrefab;
    private PlotManager.PlotBuildable selectedBuildingType;
    private int selectedBuildingWidth, selectedBuildingHeight, selectedTurnsToBuild;
    private string selectedBuildingName;

    // Awake is called before the application starts
    void Awake()
    {
        // Here to make sure only one instance of BuildingManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // This method is called when the player clicks a building button in UI-PlotGrid
    public void StartPlacement(GameObject buildingPrefab, int width, int height, string buildingName, PlotManager.PlotBuildable buildingType, int turnsToBuild)
    {
        selectedBuildingPrefab = buildingPrefab;
        selectedBuildingWidth = width;
        selectedBuildingHeight = height;
        selectedBuildingName = buildingName;
        selectedBuildingType = buildingType;
        selectedTurnsToBuild = turnsToBuild;
        isInPlacementMode = true;
    }

    // This method is called when the player clicks the same button or has placed a building
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedBuildingPrefab = null;
    }

    // Attempt to place a building at the clicked tile if its not already occupied
    public void PlaceBuildingAtGrid(Vector3 gridPosition, int clickedX, int clickedY, string gridID)
    {
        if (!isInPlacementMode || selectedBuildingPrefab == null)
            return;

        if (!TryPlaceAt(clickedX, 0, gridID))
        {
            Debug.Log($"{selectedBuildingName} cannot be placed as the area is occupied or other reasons!");
            return;
        }

        CancelPlacement();
    }

    // Method to place building prefabs at a set grid location
    bool TryPlaceAt(int gridX, int gridY, string gridID)
    {
        // Check if the building is allowed to be placed on the tile
        if (!CanPlaceOnTileType(gridX, gridY, gridID))
        {
            Debug.Log($"{selectedBuildingName} cannot be placed on this tile type!");
            return false;
        }

        // Check if area is available to be placed at
        BuildingPosition[] allBuildings = FindObjectsByType<BuildingPosition>(FindObjectsSortMode.None);

        for (int x = gridX; x < gridX + selectedBuildingWidth; x++)
        {
            for (int y = gridY; y < gridY + selectedBuildingHeight; y++)
            {
                foreach (BuildingPosition building in allBuildings)
                {
                    bool sameGrid = building.gridID == gridID;
                    bool withinX = (x >= building.gridX && x < building.gridX + building.width);
                    bool withinY = (y >= building.gridY && y < building.gridY + building.height);

                    // If area is already occupied, then return false
                    if (sameGrid && withinX && withinY)
                        return false;
                }
            }
        }

        // If the area is not occupied by an existing building, place at that location
        GameObject floorTile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        Vector3 spawnPosition;
        if (floorTile != null)
        {
            spawnPosition = new Vector3(floorTile.transform.position.x - 0.5f, floorTile.transform.position.y - 0.5f, 0f);
        }
        else
        {
            spawnPosition = new Vector3(gridX, gridY, 0f);
        }

        GameObject newBuilding = Instantiate(selectedBuildingPrefab, spawnPosition, Quaternion.identity);
        newBuilding.name = selectedBuildingName + "_Building";

        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate != null && gridTemplate.transform.parent != null)
        {
            newBuilding.transform.SetParent(gridTemplate.transform.parent);
        }
        else if (gridTemplate != null)
        {
            newBuilding.transform.SetParent(gridTemplate.transform);
            Debug.LogWarning($"Grid {gridID} has no parent, using grid itself as parent");
        }
        else
        {
            Debug.LogWarning($"Could not find grid: {gridID}");
        }


        BuildingPosition position = newBuilding.AddComponent<BuildingPosition>();
        position.gridX = gridX;
        position.gridY = gridY;
        position.width = selectedBuildingWidth;
        position.height = selectedBuildingHeight;
        position.gridID = gridID;

        return true;
    }

    bool CanPlaceOnTileType(int gridX, int gridY, string gridID)
    {
        GameObject tile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        if (tile == null)
            return false;

        GridTemplateScript gridTemplate = tile.GetComponentInParent<GridTemplateScript>();
        if (gridTemplate == null)
            return false;

        GridTemplateScript.PlotBuildable tileType = gridTemplate.defaultBuildType;
        PlotManager.PlotBuildable requiredType = selectedBuildingType;

        return (int)requiredType == (int)tileType;
    }
}

// Component that tracks which tile a building is occupying
public class BuildingPosition : MonoBehaviour
{
    public int gridX, gridY, width, height;
    public string gridID;
}