using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Manager Settings")]
    public static BuildingManager Instance;
    public bool isInPlacementMode = false;

    private GameObject selectedBuildingPrefab;
    private PlotManager.PlotBuildable selectedBuildingType;
    private int selectedBuildingWidth, selectedBuildingHeight;
    private int selectedTurnsToBuild;
    private string selectedBuildingName;
    private Sprite selectedFinishedSprite;

    private bool lastPlacementMode = false;
    private int lastViewingQuadrant = -1;

    // Awake is called before the application starts
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        // Toggle grid template visibility when placement state or quadrant changes
        if ((isInPlacementMode != lastPlacementMode) || (GameManager.Instance.ViewingQuadrant != lastViewingQuadrant))
        {
            if (GameManager.Instance == null || GameManager.Instance.ViewingQuadrant == 0)
                return;

            GameObject currentQuadrant = GetCurrentQuadrant();
            if (currentQuadrant == null) return;

            GridTemplateScript[] allGrids = currentQuadrant.GetComponentsInChildren<GridTemplateScript>(true);

            foreach (GridTemplateScript grid in allGrids)
            {
                grid.gameObject.SetActive(isInPlacementMode);
            }

            lastPlacementMode = isInPlacementMode;
            lastViewingQuadrant = GameManager.Instance != null ? GameManager.Instance.ViewingQuadrant : -1;
        }
    }

    // Called when player clicks a building button
    public void StartPlacement(GameObject buildingPrefab, int width, int height, string buildingName,
                               PlotManager.PlotBuildable buildingType, int turnsToBuild, Sprite finishedSprite = null)
    {
        selectedBuildingPrefab = buildingPrefab;
        selectedBuildingWidth = width;
        selectedBuildingHeight = height;
        selectedBuildingName = buildingName;
        selectedBuildingType = buildingType;
        selectedTurnsToBuild = turnsToBuild;
        selectedFinishedSprite = finishedSprite;

        isInPlacementMode = true;
    }

    // Called when placement is cancelled
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedBuildingPrefab = null;
        selectedFinishedSprite = null;
    }

    // Attempt to place a building at the clicked tile
    public void PlaceBuildingAtGrid(Vector3 gridPosition, int clickedX, int clickedY, string gridID)
    {
        if (!isInPlacementMode || selectedBuildingPrefab == null)
            return;

        // Ensure the building can fit in the grid
        if (!CanBuildingFitInGrid(gridID))
        {
            Debug.Log($"{selectedBuildingName} is too large for this grid!");
            return;
        }

        // Find best X position starting from clicked tile
        int finalX = FindBestPlacementX(clickedX, gridID);

        if (finalX == -1)
        {
            Debug.Log($"{selectedBuildingName} cannot be placed as there is no available space!");
            return;
        }

        // Try placing building
        if (TryPlaceAt(finalX, 0, gridID))
            CancelPlacement();
    }

    // Actually spawn the building prefab
    bool TryPlaceAt(int gridX, int gridY, string gridID)
    {
        if (!CanPlaceOnTileType(gridX, gridY, gridID))
        {
            Debug.Log($"{selectedBuildingName} cannot be placed on this tile type!");
            return false;
        }

        // Check overlap with other buildings
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

                    if (sameGrid && withinX && withinY)
                        return false;
                }
            }
        }

        // Find spawn tile position
        GameObject floorTile = GameObject.Find($"{gridID}_GridSquare_Pos{gridX}{gridY}");
        Vector3 spawnPosition = floorTile != null ?
            new Vector3(floorTile.transform.position.x - 0.5f, floorTile.transform.position.y - 0.5f, 0f) :
            new Vector3(gridX, gridY, 0f);

        GameObject newBuilding = Instantiate(selectedBuildingPrefab, spawnPosition, Quaternion.identity);
        newBuilding.name = selectedBuildingName + "_Building";
        newBuilding.transform.localScale = new Vector3(selectedBuildingWidth, selectedBuildingHeight, 1f);

        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate != null && gridTemplate.transform.parent != null)
        {
            newBuilding.transform.SetParent(gridTemplate.transform.parent);
        }
        else if (gridTemplate != null)
        {
            newBuilding.transform.SetParent(gridTemplate.transform);
        }
        else
        {
            Debug.LogWarning($"Could not find grid: {gridID}");
        }

        // Register building position
        BuildingPosition position = newBuilding.AddComponent<BuildingPosition>();
        position.gridX = gridX;
        position.gridY = gridY;
        position.width = selectedBuildingWidth;
        position.height = selectedBuildingHeight;
        position.gridID = gridID;

        // Restore construction progress system
        BuildingProgress bp = newBuilding.GetComponent<BuildingProgress>();
        if (bp != null)
        {
            bp.Initialize(selectedFinishedSprite, selectedTurnsToBuild);
        }

        return true;
    }

    // Check tile type
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

    // Ensure building fits in template bounds
    bool CanBuildingFitInGrid(string gridID)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null) return false;

        GridTemplateScript grid = gridTemplate.GetComponent<GridTemplateScript>();
        if (grid == null) return false;

        return selectedBuildingWidth <= grid.templateWidth && selectedBuildingHeight <= grid.templateHeight;
    }

    // Try finding a legal position starting from clicked tile
    int FindBestPlacementX(int clickedX, string gridID)
    {
        GameObject gridTemplate = GameObject.Find(gridID);
        if (gridTemplate == null) return -1;

        GridTemplateScript grid = gridTemplate.GetComponent<GridTemplateScript>();
        if (grid == null) return -1;

        int maxValidX = grid.templateWidth - selectedBuildingWidth;
        int startX = Mathf.Min(clickedX, maxValidX);

        for (int testX = startX; testX >= 0; testX--)
        {
            if (CanPlaceAtPosition(testX, 0, gridID))
            {
                return testX;
            }
        }

        return -1;
    }

    // Test if a building can go at a given spot
    bool CanPlaceAtPosition(int gridX, int gridY, string gridID)
    {
        if (!CanPlaceOnTileType(gridX, gridY, gridID))
            return false;

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

                    if (sameGrid && withinX && withinY)
                        return false;
                }
            }
        }

        return true;
    }

    GameObject GetCurrentQuadrant()
    {
        string quadrantName = "Grid_Quadrant" + GameManager.Instance.ViewingQuadrant;
        return GameObject.Find(quadrantName);
    }
}

// Tracks which tile a building occupies
public class BuildingPosition : MonoBehaviour
{
    public int gridX, gridY, width, height;
    public string gridID;
}
