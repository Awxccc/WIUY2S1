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
    private PlotManager.PlotData selectedPlotData;

    private int finalCostFunds, finalCostWood, finalCostStone, finalCostMetal;

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
    public void StartPlacement(PlotManager.PlotData plotData, int turnsToBuild, int funds, int wood, int stone, int metal)
    {
        // Store the data for the building
        selectedPlotData = plotData;
        selectedBuildingPrefab = plotData.BuildingPrefab;
        selectedBuildingWidth = plotData.TileSizeWidth;
        selectedBuildingHeight = plotData.TileSizeHeight;
        selectedBuildingName = plotData.PlotName;
        selectedBuildingType = plotData.PlotBuildable;
        selectedFinishedSprite = plotData.PlotImage;

        // Store the final calculated turns and cost
        selectedTurnsToBuild = turnsToBuild;
        finalCostFunds = funds;
        finalCostWood = wood;
        finalCostStone = stone;
        finalCostMetal = metal;

        isInPlacementMode = true;
    }

    // Called when placement is cancelled
    public void CancelPlacement()
    {
        isInPlacementMode = false;
        selectedBuildingPrefab = null;
        selectedFinishedSprite = null;
        selectedPlotData = null;
        selectedBuildingName = null;
        selectedBuildingWidth = 0;
        selectedBuildingHeight = 0;
        selectedTurnsToBuild = 0;
        finalCostFunds = 0;
        finalCostWood = 0;
        finalCostStone = 0;
        finalCostMetal = 0;
    }

    // Attempt to place a building at the clicked tile
    public void PlaceBuildingAtGrid(Vector3 gridPosition, int clickedX, int clickedY, string gridID)
    {
        if (!isInPlacementMode || selectedBuildingPrefab == null)
            return;

        // Check if player has enough resources before trying to place
        if (!GameManager.Instance.HasEnoughFunds(finalCostFunds) ||
            !GameManager.Instance.HasEnoughWood(finalCostWood) ||
            !GameManager.Instance.HasEnoughStone(finalCostStone) ||
            !GameManager.Instance.HasEnoughMetal(finalCostMetal))
        {
            Debug.Log("Not enough resources to place this building!");
            return;
        }

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
        {
            // Deduct resources only after the building is successfully placed
            GameManager.Instance.RemoveFunds(finalCostFunds);
            GameManager.Instance.RemoveWood(finalCostWood);
            GameManager.Instance.RemoveStone(finalCostStone);
            GameManager.Instance.RemoveMetal(finalCostMetal);

            CancelPlacement();
        }
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

        if (selectedPlotData.PlotCategory == PlotManager.PlotCategory.Housing)
        {
            GameManager.Instance.AddPopulation(selectedPlotData.GainPopulation);
        }

        // Restore construction progress system
        BuildingProgress bp = newBuilding.GetComponent<BuildingProgress>();
        if (bp != null)
        {
            bp.Initialize(selectedPlotData, selectedTurnsToBuild);
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
    public void UpgradeBuilding(GameObject buildingToUpgrade)
    {
        BuildingProgress bp = buildingToUpgrade.GetComponent<BuildingProgress>();
        if (bp == null || bp.plotData == null || bp.plotData.Upgrades.Length == 0)
        {
            Debug.Log("This building cannot be upgraded.");
            return;
        }

        PlotManager.PlotData upgradeData = bp.plotData.Upgrades[0];
        BuildingPosition pos = buildingToUpgrade.GetComponent<BuildingPosition>();

        GameManager.Instance.RemoveFunds(upgradeData.CostFunds);
        GameManager.Instance.RemoveWood(upgradeData.CostWood);
        GameManager.Instance.RemoveStone(upgradeData.CostStone);
        GameManager.Instance.RemoveMetal(upgradeData.CostMetal);


        // Instantiate the new building
        GameObject newBuilding = Instantiate(upgradeData.BuildingPrefab, buildingToUpgrade.transform.position, Quaternion.identity);
        newBuilding.name = upgradeData.PlotName + "_Building";
        newBuilding.transform.localScale = new Vector3(pos.width, pos.height, 1f);
        newBuilding.transform.SetParent(buildingToUpgrade.transform.parent);

        BuildingPosition newPos = newBuilding.AddComponent<BuildingPosition>();
        newPos.gridX = pos.gridX;
        newPos.gridY = pos.gridY;
        newPos.width = pos.width;
        newPos.height = pos.height;
        newPos.gridID = pos.gridID;

        BuildingProgress newBp = newBuilding.GetComponent<BuildingProgress>();
        if (newBp != null)
        {
            newBp.Initialize(upgradeData, upgradeData.TurnsToBuild);
        }

        Destroy(buildingToUpgrade);
    }
}

// Tracks which tile a building occupies
public class BuildingPosition : MonoBehaviour
{
    public int gridX, gridY, width, height;
    public string gridID;
}
